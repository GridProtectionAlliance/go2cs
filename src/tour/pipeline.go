package main

import (
	"bytes"
	"context"
	"errors"
	"fmt"
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strings"
	"sync"
	"time"
)

const (
	maxSourceBytes  = 256 << 10
	maxRequestBytes = maxSourceBytes + (16 << 10)
	commandTimeout  = 20 * time.Second
)

type pipelineRequest struct {
	Source string `json:"source"`
	Name   string `json:"name,omitempty"`
}

type pipelineResult struct {
	CSharp     string        `json:"csharp,omitempty"`
	Project    string        `json:"project,omitempty"`
	Successful bool          `json:"successful"`
	Stages     []stageResult `json:"stages"`
}

type stageResult struct {
	ID         string `json:"id"`
	Label      string `json:"label"`
	Status     string `json:"status"`
	Output     string `json:"output,omitempty"`
	DurationMS int64  `json:"durationMs"`
}

type pipelineRunner struct {
	repoRoot string
	cacheDir string
	slots    chan struct{}
	buildMu  sync.Mutex
}

func newPipelineRunner(repoRoot string) *pipelineRunner {
	return &pipelineRunner{
		repoRoot: repoRoot,
		cacheDir: filepath.Join(repoRoot, "src", "tour", ".cache"),
		slots:    make(chan struct{}, 2),
	}
}

func (p *pipelineRunner) run(ctx context.Context, request pipelineRequest) (pipelineResult, error) {
	source := strings.TrimPrefix(request.Source, "\ufeff")
	if len(source) == 0 {
		return pipelineResult{}, validationError("the Go editor is empty")
	}
	if len(source) > maxSourceBytes {
		return pipelineResult{}, validationError("source exceeds the 256 KiB local limit")
	}

	select {
	case p.slots <- struct{}{}:
		defer func() { <-p.slots }()
	case <-ctx.Done():
		return pipelineResult{}, ctx.Err()
	}

	workDir, err := os.MkdirTemp("", "tour-go2cs-")
	if err != nil {
		return pipelineResult{}, err
	}
	defer os.RemoveAll(workDir)

	inputDir := filepath.Join(workDir, "go")
	outputDir := filepath.Join(workDir, "cs")
	if err := os.MkdirAll(inputDir, 0o755); err != nil {
		return pipelineResult{}, err
	}

	if err := os.WriteFile(filepath.Join(inputDir, "main.go"), []byte(source), 0o600); err != nil {
		return pipelineResult{}, err
	}
	if err := os.WriteFile(filepath.Join(inputDir, "go.mod"), []byte("module tour.local/session\n\ngo 1.23.1\n"), 0o600); err != nil {
		return pipelineResult{}, err
	}

	result := pipelineResult{}
	result.Stages = append(result.Stages, p.runStage(ctx, "go", "Go run", inputDir, commandTimeout, "go", "run", "."))

	go2csBin, toolStage := p.ensureGo2CS(ctx)
	if toolStage != nil {
		result.Stages = append(result.Stages, *toolStage)
		if toolStage.Status == "failed" {
			result.Stages = append(result.Stages, skippedStage("transpile", "go2cs transpile"), skippedStage("build", ".NET build"), skippedStage("run", ".NET run"))
			return result, nil
		}
	}

	transpile := p.runStage(ctx, "transpile", "go2cs transpile", p.repoRoot, commandTimeout,
		go2csBin, "-go2cspath", filepath.Join(p.repoRoot, "src"), inputDir, outputDir)
	result.Stages = append(result.Stages, transpile)

	result.CSharp, _ = collectCSharp(outputDir)
	project, _ := findProject(outputDir)
	if project != "" {
		if relative, err := filepath.Rel(outputDir, project); err == nil {
			result.Project = filepath.ToSlash(relative)
		}
	}

	if transpile.Status != "passed" || project == "" {
		if project == "" && transpile.Status == "passed" {
			result.Stages[len(result.Stages)-1].Status = "failed"
			result.Stages[len(result.Stages)-1].Output += "\ngo2cs did not emit a .csproj file."
		}
		result.Stages = append(result.Stages, skippedStage("build", ".NET build"), skippedStage("run", ".NET run"))
		return result, nil
	}

	build := p.runStage(ctx, "build", ".NET build", outputDir, commandTimeout,
		"dotnet", "build", project, "--nologo", "--verbosity:minimal", "-p:go2csPath="+filepath.Join(p.repoRoot, "src")+string(filepath.Separator))
	result.Stages = append(result.Stages, build)
	if build.Status != "passed" {
		result.Stages = append(result.Stages, skippedStage("run", ".NET run"))
		return result, nil
	}

	run := p.runStage(ctx, "run", ".NET run", outputDir, commandTimeout,
		"dotnet", "run", "--no-build", "--project", project, "-p:go2csPath="+filepath.Join(p.repoRoot, "src")+string(filepath.Separator))
	result.Stages = append(result.Stages, run)
	result.Successful = transpile.Status == "passed" && build.Status == "passed" && run.Status == "passed"
	return result, nil
}

func (p *pipelineRunner) ensureGo2CS(ctx context.Context) (string, *stageResult) {
	if configured := strings.TrimSpace(os.Getenv("GO2CS_BIN")); configured != "" {
		if _, err := os.Stat(configured); err == nil {
			return configured, nil
		}
	}
	if found, err := exec.LookPath("go2cs"); err == nil {
		return found, nil
	}

	name := "go2cs"
	if filepath.Separator == '\\' {
		name += ".exe"
	}
	target := filepath.Join(p.cacheDir, name)
	if _, err := os.Stat(target); err == nil {
		return target, nil
	}

	p.buildMu.Lock()
	defer p.buildMu.Unlock()
	if _, err := os.Stat(target); err == nil {
		return target, nil
	}
	if err := os.MkdirAll(p.cacheDir, 0o755); err != nil {
		stage := failedStage("tool", "Build go2cs", err.Error())
		return target, &stage
	}

	stage := p.runStage(ctx, "tool", "Build go2cs", filepath.Join(p.repoRoot, "src", "go2cs"),
		2*time.Minute, "go", "build", "-o", target, ".")
	return target, &stage
}

func (p *pipelineRunner) runStage(parent context.Context, id, label, dir string, timeout time.Duration, name string, args ...string) stageResult {
	started := time.Now()
	ctx, cancel := context.WithTimeout(parent, timeout)
	defer cancel()

	command := newCommand(ctx, name, args...)
	command.Dir = dir
	hideCommandWindow(command)
	var output bytes.Buffer
	command.Stdout = &output
	command.Stderr = &output
	err := command.Run()

	text := cleanDisplayPath(output.String(), p.repoRoot)
	status := "passed"
	if err != nil {
		status = "failed"
		if ctx.Err() != nil {
			text += fmt.Sprintf("\nTimed out after %s.", timeout)
		} else if text == "" {
			text = err.Error()
		}
	}

	return stageResult{
		ID:         id,
		Label:      label,
		Status:     status,
		Output:     strings.TrimSpace(text),
		DurationMS: time.Since(started).Milliseconds(),
	}
}

func skippedStage(id, label string) stageResult {
	return stageResult{ID: id, Label: label, Status: "skipped", Output: "Skipped because an earlier stage failed."}
}

func failedStage(id, label, output string) stageResult {
	return stageResult{ID: id, Label: label, Status: "failed", Output: output}
}

func collectCSharp(root string) (string, error) {
	var files []string
	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			if entry.Name() == "obj" || entry.Name() == "bin" {
				return filepath.SkipDir
			}
			return nil
		}
		if strings.HasSuffix(strings.ToLower(entry.Name()), ".cs") {
			files = append(files, path)
		}
		return nil
	})
	if err != nil {
		return "", err
	}
	sort.Strings(files)

	var result strings.Builder
	for _, file := range files {
		content, err := os.ReadFile(file)
		if err != nil {
			return "", err
		}
		relative, _ := filepath.Rel(root, file)
		if result.Len() > 0 {
			result.WriteString("\n\n")
		}
		result.WriteString("// ── ")
		result.WriteString(filepath.ToSlash(relative))
		result.WriteString(" ──\n")
		result.Write(content)
	}
	return result.String(), nil
}

func findProject(root string) (string, error) {
	var projects []string
	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if !entry.IsDir() && strings.HasSuffix(strings.ToLower(entry.Name()), ".csproj") {
			projects = append(projects, path)
		}
		return nil
	})
	if err != nil {
		return "", err
	}
	sort.Strings(projects)
	if len(projects) == 0 {
		return "", errors.New("no generated project")
	}
	return projects[0], nil
}
