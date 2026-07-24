package main

import (
	"bytes"
	"context"
	"crypto/rand"
	"encoding/hex"
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
	maxSourceBytes    = 256 << 10
	maxRequestBytes   = maxSourceBytes + (16 << 10)
	commandTimeout    = 20 * time.Second
	conversionMaxAge  = 30 * time.Minute
	maxSavedArtifacts = 20
)

type convertRequest struct {
	Source  string `json:"source"`
	Name    string `json:"name,omitempty"`
	Runtime string `json:"runtime,omitempty"`
}

type runRequest struct {
	ConversionID string `json:"conversionId"`
}

type convertResult struct {
	ConversionID string      `json:"conversionId,omitempty"`
	CSharp       string      `json:"csharp,omitempty"`
	Project      string      `json:"project,omitempty"`
	ProjectName  string      `json:"projectName,omitempty"`
	Successful   bool        `json:"successful"`
	Runtime      string      `json:"runtime"`
	Stage        stageResult `json:"stage"`
}

type runResult struct {
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

type conversionArtifact struct {
	id        string
	workDir   string
	outputDir string
	project   string
	runtime   runtimeConfiguration
	createdAt time.Time
}

type pipelineRunner struct {
	repoRoot     string
	cacheDir     string
	slots        chan struct{}
	buildMu      sync.Mutex
	mu           sync.Mutex
	saved        map[string]*conversionArtifact
	deployedRoot string
	nugetSource  string
	nugetVersion string
}

func newPipelineRunner(repoRoot string, supplied ...pipelineOptions) *pipelineRunner {
	options := pipelineOptions{}
	if len(supplied) > 0 {
		options = supplied[0]
	}
	options = resolvePipelineOptions(repoRoot, options)
	return &pipelineRunner{
		repoRoot:     repoRoot,
		cacheDir:     filepath.Join(repoRoot, "src", "tour", ".cache"),
		slots:        make(chan struct{}, 2),
		saved:        make(map[string]*conversionArtifact),
		deployedRoot: options.deployedRoot,
		nugetSource:  options.nugetSource,
		nugetVersion: options.nugetVersion,
	}
}

func (p *pipelineRunner) convert(ctx context.Context, request convertRequest) (convertResult, error) {
	source := strings.TrimPrefix(request.Source, "\ufeff")
	if len(source) == 0 {
		return convertResult{}, validationError("the Go editor is empty")
	}
	if len(source) > maxSourceBytes {
		return convertResult{}, validationError("source exceeds the 256 KiB local limit")
	}
	runtime, err := p.resolveRuntime(request.Runtime)
	if err != nil {
		return convertResult{}, err
	}

	if err := p.acquire(ctx); err != nil {
		return convertResult{}, err
	}
	defer p.release()
	p.cleanupExpired()

	workDir, err := os.MkdirTemp("", "tour-go2cs-")
	if err != nil {
		return convertResult{}, err
	}
	keepWorkspace := false
	defer func() {
		if !keepWorkspace {
			_ = os.RemoveAll(workDir)
		}
	}()

	inputDir := filepath.Join(workDir, "go")
	outputDir := filepath.Join(workDir, "cs")
	if err := os.MkdirAll(inputDir, 0o755); err != nil {
		return convertResult{}, err
	}
	if err := os.WriteFile(filepath.Join(inputDir, "main.go"), []byte(source), 0o600); err != nil {
		return convertResult{}, err
	}
	if err := os.WriteFile(filepath.Join(inputDir, "go.mod"), []byte("module tour.local/session\n\ngo 1.23.1\n"), 0o600); err != nil {
		return convertResult{}, err
	}

	go2csBin, toolStage := p.ensureGo2CS(ctx)
	if toolStage != nil && toolStage.Status != "passed" {
		toolStage.ID = "transpile"
		toolStage.Label = "Transpile"
		return convertResult{Stage: *toolStage}, nil
	}

	transpile := p.runStage(ctx, "transpile", "Transpile", p.repoRoot, commandTimeout,
		go2csBin, "-go2cspath", runtime.converterRoot, inputDir, outputDir)
	if toolStage != nil {
		transpile.DurationMS += toolStage.DurationMS
		if toolStage.Output != "" {
			transpile.Output = strings.TrimSpace(toolStage.Output + "\n" + transpile.Output)
		}
	}

	result := convertResult{Runtime: runtime.mode, Stage: transpile}
	result.CSharp, _ = collectCSharp(outputDir)
	project, _ := findProject(outputDir)
	if project != "" {
		if err := prepareRuntimeProject(project, runtime); err != nil {
			result.Stage.Status = "failed"
			result.Stage.Output = appendOutputLine(result.Stage.Output, err.Error())
		}
		projectBytes, readErr := os.ReadFile(project)
		if readErr == nil {
			result.Project = string(projectBytes)
			result.ProjectName = filepath.Base(project)
		}
	}

	if project == "" && result.Stage.Status == "passed" {
		result.Stage.Status = "failed"
		result.Stage.Output = appendOutputLine(result.Stage.Output, "go2cs did not emit a .csproj file.")
	}
	result.Stage.Output = formatTranspileTranscript(result.Stage.Output, outputDir, result.Stage.Status, runtime.label)
	if result.Stage.Status != "passed" || project == "" {
		return result, nil
	}

	id, err := newConversionID()
	if err != nil {
		return convertResult{}, err
	}
	artifact := &conversionArtifact{
		id:        id,
		workDir:   workDir,
		outputDir: outputDir,
		project:   project,
		runtime:   runtime,
		createdAt: time.Now(),
	}
	p.mu.Lock()
	p.saved[id] = artifact
	p.mu.Unlock()

	keepWorkspace = true
	result.ConversionID = id
	result.Successful = true
	return result, nil
}

func (p *pipelineRunner) run(ctx context.Context, request runRequest) (runResult, error) {
	if strings.TrimSpace(request.ConversionID) == "" {
		return runResult{}, validationError("conversionId is required")
	}
	if err := p.acquire(ctx); err != nil {
		return runResult{}, err
	}
	defer p.release()
	p.cleanupExpired()

	p.mu.Lock()
	artifact := p.saved[request.ConversionID]
	p.mu.Unlock()
	if artifact == nil {
		return runResult{}, conversionNotFoundError("that conversion has expired; convert the current Go source again")
	}

	buildArgs := []string{"build", artifact.project, "--nologo", "--verbosity:minimal"}
	if artifact.runtime.projectRoot != "" {
		buildArgs = append(buildArgs, "-p:go2csPath="+artifact.runtime.projectRoot+string(filepath.Separator))
	}
	build := p.runStage(ctx, "build", "Build", artifact.outputDir, commandTimeout, "dotnet", buildArgs...)
	result := runResult{Stages: []stageResult{build}}
	if build.Status != "passed" {
		result.Stages = append(result.Stages, skippedStage("run", ".NET Run"))
		return result, nil
	}

	runArgs := []string{"run", "--no-build", "--project", artifact.project}
	if artifact.runtime.projectRoot != "" {
		runArgs = append(runArgs, "-p:go2csPath="+artifact.runtime.projectRoot+string(filepath.Separator))
	}
	run := p.runStage(ctx, "run", ".NET Run", artifact.outputDir, commandTimeout, "dotnet", runArgs...)
	result.Stages = append(result.Stages, run)
	result.Successful = run.Status == "passed"
	return result, nil
}

func (p *pipelineRunner) acquire(ctx context.Context) error {
	select {
	case p.slots <- struct{}{}:
		return nil
	case <-ctx.Done():
		return ctx.Err()
	}
}

func (p *pipelineRunner) release() {
	<-p.slots
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

func formatTranspileTranscript(output, root, status, runtime string) string {
	lines := []string{"$ go2cs main.go", "Runtime: " + runtime}
	if output = strings.TrimSpace(output); output != "" {
		lines = append(lines, output)
	}
	if files, err := generatedArtifactNames(root); err == nil && len(files) > 0 {
		lines = append(lines, "Generated files:")
		for _, file := range files {
			lines = append(lines, "  "+file)
		}
	}
	switch status {
	case "passed":
		lines = append(lines, "Transpile completed.")
	case "killed":
		lines = append(lines, "Transpile killed.")
	default:
		lines = append(lines, "Transpile failed.")
	}
	return strings.Join(lines, "\n")
}

func generatedArtifactNames(root string) ([]string, error) {
	var files []string
	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			switch entry.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}
			return nil
		}
		extension := strings.ToLower(filepath.Ext(entry.Name()))
		if extension != ".cs" && extension != ".csproj" {
			return nil
		}
		relative, err := filepath.Rel(root, path)
		if err != nil {
			return err
		}
		files = append(files, filepath.ToSlash(relative))
		return nil
	})
	sort.Strings(files)
	return files, err
}

func appendOutputLine(output, line string) string {
	output = strings.TrimSpace(output)
	if output == "" {
		return line
	}
	return output + "\n" + line
}

func programExitMessage(parent, command context.Context, err error) string {
	switch {
	case err == nil:
		return "Program exited."
	case errors.Is(parent.Err(), context.Canceled):
		return "Program exited: killed"
	case errors.Is(command.Err(), context.DeadlineExceeded):
		return "Program exited: process took too long."
	}

	var exitError *exec.ExitError
	if errors.As(err, &exitError) && exitError.ExitCode() >= 0 {
		return fmt.Sprintf("Program exited: status %d.", exitError.ExitCode())
	}
	return "Program exited: " + strings.TrimSuffix(strings.TrimSpace(err.Error()), ".") + "."
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
		switch {
		case errors.Is(parent.Err(), context.Canceled):
			status = "killed"
			if id != "run" {
				text = appendOutputLine(text, "Killed.")
			}
		case errors.Is(ctx.Err(), context.DeadlineExceeded):
			if id != "run" {
				text = appendOutputLine(text, fmt.Sprintf("Timed out after %s.", timeout))
			}
		case text == "" && id != "run":
			text = err.Error()
		}
	}
	if id == "run" {
		text = appendOutputLine(text, programExitMessage(parent, ctx, err))
	}

	return stageResult{
		ID:         id,
		Label:      label,
		Status:     status,
		Output:     strings.TrimSpace(text),
		DurationMS: time.Since(started).Milliseconds(),
	}
}

func (p *pipelineRunner) cleanupExpired() {
	now := time.Now()
	var remove []*conversionArtifact

	p.mu.Lock()
	for id, artifact := range p.saved {
		if now.Sub(artifact.createdAt) > conversionMaxAge {
			remove = append(remove, artifact)
			delete(p.saved, id)
		}
	}
	if len(p.saved) > maxSavedArtifacts {
		artifacts := make([]*conversionArtifact, 0, len(p.saved))
		for _, artifact := range p.saved {
			artifacts = append(artifacts, artifact)
		}
		sort.Slice(artifacts, func(i, j int) bool {
			return artifacts[i].createdAt.Before(artifacts[j].createdAt)
		})
		for len(p.saved) > maxSavedArtifacts {
			artifact := artifacts[0]
			artifacts = artifacts[1:]
			remove = append(remove, artifact)
			delete(p.saved, artifact.id)
		}
	}
	p.mu.Unlock()

	for _, artifact := range remove {
		_ = os.RemoveAll(artifact.workDir)
	}
}

func (p *pipelineRunner) close() {
	p.mu.Lock()
	artifacts := make([]*conversionArtifact, 0, len(p.saved))
	for _, artifact := range p.saved {
		artifacts = append(artifacts, artifact)
	}
	p.saved = make(map[string]*conversionArtifact)
	p.mu.Unlock()
	for _, artifact := range artifacts {
		_ = os.RemoveAll(artifact.workDir)
	}
}

func newConversionID() (string, error) {
	value := make([]byte, 16)
	if _, err := rand.Read(value); err != nil {
		return "", err
	}
	return hex.EncodeToString(value), nil
}

func skippedStage(id, label string) stageResult {
	return stageResult{ID: id, Label: label, Status: "skipped", Output: "Skipped because an earlier stage failed."}
}

func failedStage(id, label, output string) stageResult {
	return stageResult{ID: id, Label: label, Status: "failed", Output: output}
}

func collectCSharp(root string) (string, error) {
	var sourceFiles []string
	var fallbackFiles []string
	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			if entry.Name() == "obj" || entry.Name() == "bin" || entry.Name() == "Generated" {
				return filepath.SkipDir
			}
			return nil
		}
		if !strings.HasSuffix(strings.ToLower(entry.Name()), ".cs") {
			return nil
		}
		fallbackFiles = append(fallbackFiles, path)
		name := strings.ToLower(entry.Name())
		if name != "package_info.cs" && name != "go2cs_test_host.cs" {
			sourceFiles = append(sourceFiles, path)
		}
		return nil
	})
	if err != nil {
		return "", err
	}
	if len(sourceFiles) == 0 {
		sourceFiles = fallbackFiles
	}
	sort.Strings(sourceFiles)

	var result strings.Builder
	for _, file := range sourceFiles {
		content, err := os.ReadFile(file)
		if err != nil {
			return "", err
		}
		if result.Len() > 0 {
			relative, _ := filepath.Rel(root, file)
			result.WriteString("\n\n// -- ")
			result.WriteString(filepath.ToSlash(relative))
			result.WriteString(" --\n")
		}
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

type conversionNotFoundError string

func (e conversionNotFoundError) Error() string { return string(e) }
