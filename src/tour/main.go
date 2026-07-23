// Tour of go2cs hosts the official Tour of Go beside a live go2cs/.NET pipeline.
package main

import (
	"context"
	"embed"
	"encoding/json"
	"errors"
	"flag"
	"fmt"
	"io/fs"
	"log"
	"net"
	"net/http"
	"os"
	"os/signal"
	"path/filepath"
	"runtime"
	"strings"
	"syscall"
	"time"
)

//go:embed web/*
var webFiles embed.FS

type server struct {
	repoRoot *string
	pipeline *pipelineRunner
	tour     *tourProxy
	static   http.Handler
}

func main() {
	addr := flag.String("addr", "127.0.0.1:4000", "Tour of go2cs address")
	tourAddr := flag.String("tour-addr", "127.0.0.1:3999", "official Tour of Go address")
	repoRoot := flag.String("repo", "", "go2cs repository root (auto-detected by default)")
	noTour := flag.Bool("no-tour", false, "do not start the official Tour process")
	noOpen := flag.Bool("no-open", false, "do not open a browser")
	flag.Parse()

	root, err := resolveRepoRoot(*repoRoot)
	if err != nil {
		log.Fatal(err)
	}

	ctx, stop := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer stop()

	tour := newTourProxy(*tourAddr, root)
	if !*noTour {
		if err := tour.start(ctx); err != nil {
			log.Printf("official Tour unavailable: %v", err)
		}
	}
	defer tour.close()

	staticFS, err := fs.Sub(webFiles, "web")
	if err != nil {
		log.Fatal(err)
	}

	s := &server{
		repoRoot: &root,
		pipeline: newPipelineRunner(root),
		tour:     tour,
		static:   http.FileServer(http.FS(staticFS)),
	}

	httpServer := &http.Server{
		Addr:              *addr,
		Handler:           s.routes(),
		ReadHeaderTimeout: 5 * time.Second,
	}

	listener, err := net.Listen("tcp", *addr)
	if err != nil {
		log.Fatal(err)
	}

	url := "http://" + listener.Addr().String()
	log.Printf("Tour of go2cs is ready at %s", url)
	if !*noOpen {
		go openBrowser(url)
	}

	errs := make(chan error, 1)
	go func() { errs <- httpServer.Serve(listener) }()

	select {
	case <-ctx.Done():
	case err := <-errs:
		if err != nil && !errors.Is(err, http.ErrServerClosed) {
			log.Printf("server stopped: %v", err)
		}
	}

	shutdownCtx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	_ = httpServer.Shutdown(shutdownCtx)
}

func (s *server) routes() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("POST /api/pipeline", s.handlePipeline)
	mux.HandleFunc("GET /api/health", s.handleHealth)
	mux.HandleFunc("GET /assets/go2cs.ico", s.handleImage("go2cs.ico"))
	mux.HandleFunc("GET /assets/go2cs-small.png", s.handleImage("go2cs-small.png"))
	mux.HandleFunc("GET /assets/GopherDotNetBotFrisbee.png", s.handleImage("GopherDotNetBotFrisbee.png"))
	mux.Handle("/tour/", s.tour)
	mux.Handle("/socket", s.tour)
	mux.Handle("/", s.static)
	return withSecurityHeaders(mux)
}

func (s *server) handleHealth(w http.ResponseWriter, _ *http.Request) {
	writeJSON(w, http.StatusOK, map[string]any{
		"ok":       true,
		"tour":     s.tour.available(),
		"repoRoot": *s.repoRoot,
	})
}

func (s *server) handlePipeline(w http.ResponseWriter, r *http.Request) {
	r.Body = http.MaxBytesReader(w, r.Body, maxRequestBytes)
	var request pipelineRequest
	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(&request); err != nil {
		writeJSON(w, http.StatusBadRequest, apiError{Error: "invalid request: " + err.Error()})
		return
	}

	result, err := s.pipeline.run(r.Context(), request)
	if err != nil {
		var validation validationError
		if errors.As(err, &validation) {
			writeJSON(w, http.StatusBadRequest, apiError{Error: validation.Error()})
			return
		}
		writeJSON(w, http.StatusInternalServerError, apiError{Error: err.Error()})
		return
	}

	writeJSON(w, http.StatusOK, result)
}

func (s *server) handleImage(name string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		path := filepath.Join(*s.repoRoot, "docs", "images", name)
		http.ServeFile(w, r, path)
	}
}

func writeJSON(w http.ResponseWriter, status int, value any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(value)
}

func withSecurityHeaders(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("X-Content-Type-Options", "nosniff")
		w.Header().Set("Referrer-Policy", "no-referrer")
		w.Header().Set("Permissions-Policy", "camera=(), microphone=(), geolocation=()")
		next.ServeHTTP(w, r)
	})
}

func resolveRepoRoot(explicit string) (string, error) {
	if explicit != "" {
		return validateRepoRoot(explicit)
	}

	candidates := []string{}
	if cwd, err := os.Getwd(); err == nil {
		candidates = append(candidates, cwd)
	}
	if executable, err := os.Executable(); err == nil {
		candidates = append(candidates, filepath.Dir(executable))
	}

	for _, candidate := range candidates {
		for {
			if root, err := validateRepoRoot(candidate); err == nil {
				return root, nil
			}
			parent := filepath.Dir(candidate)
			if parent == candidate {
				break
			}
			candidate = parent
		}
	}

	return "", fmt.Errorf("could not locate the go2cs repository; start from src/tour or pass -repo")
}

func validateRepoRoot(root string) (string, error) {
	absolute, err := filepath.Abs(root)
	if err != nil {
		return "", err
	}
	for _, required := range []string{
		filepath.Join("src", "go2cs", "go.mod"),
		filepath.Join("docs", "images", "go2cs.ico"),
	} {
		if _, err := os.Stat(filepath.Join(absolute, required)); err != nil {
			return "", fmt.Errorf("%s is not a go2cs repository root", absolute)
		}
	}
	return absolute, nil
}

func openBrowser(url string) {
	var name string
	var args []string
	switch runtime.GOOS {
	case "windows":
		name, args = "rundll32", []string{"url.dll,FileProtocolHandler", url}
	case "darwin":
		name, args = "open", []string{url}
	default:
		name, args = "xdg-open", []string{url}
	}
	_ = startDetached(name, args...)
}

func startDetached(name string, args ...string) error {
	command := newCommand(context.Background(), name, args...)
	command.Stdout = nil
	command.Stderr = nil
	return command.Start()
}

type apiError struct {
	Error string `json:"error"`
}

type validationError string

func (e validationError) Error() string { return string(e) }

func cleanDisplayPath(value, root string) string {
	if value == "" {
		return value
	}
	clean := strings.ReplaceAll(value, root, "<repo>")
	return strings.ReplaceAll(clean, filepath.ToSlash(root), "<repo>")
}
