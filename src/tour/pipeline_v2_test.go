package main

import (
	"context"
	"os"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

func TestCollectCSharpPrefersSourceFiles(t *testing.T) {
	root := t.TempDir()
	if err := os.WriteFile(filepath.Join(root, "main.cs"), []byte("class Main {}"), 0o600); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(root, "package_info.cs"), []byte("metadata"), 0o600); err != nil {
		t.Fatal(err)
	}

	got, err := collectCSharp(root)
	if err != nil {
		t.Fatal(err)
	}
	if !strings.Contains(got, "class Main") {
		t.Fatal("source file was not returned")
	}
	if strings.Contains(got, "metadata") {
		t.Fatal("package metadata obscures the matching source view")
	}
}

func TestRunStageReportsKilledContext(t *testing.T) {
	ctx, cancel := context.WithCancel(context.Background())
	cancel()

	runner := newPipelineRunner(t.TempDir())
	stage := runner.runStage(ctx, "run", ".NET Run", t.TempDir(), time.Second, "go", "version")
	if stage.Status != "killed" {
		t.Fatalf("status = %q, want killed; output: %s", stage.Status, stage.Output)
	}
	if stage.Output != "Program exited: killed" {
		t.Fatalf("output = %q, want Tour-compatible killed message", stage.Output)
	}
}

func TestConversionIDsAreUnique(t *testing.T) {
	first, err := newConversionID()
	if err != nil {
		t.Fatal(err)
	}
	second, err := newConversionID()
	if err != nil {
		t.Fatal(err)
	}
	if first == second || len(first) != 32 || len(second) != 32 {
		t.Fatalf("unexpected conversion IDs: %q %q", first, second)
	}
}

func TestProgramExitMessageMatchesTour(t *testing.T) {
	if got := programExitMessage(context.Background(), context.Background(), nil); got != "Program exited." {
		t.Fatalf("successful exit = %q", got)
	}

	parent, cancel := context.WithCancel(context.Background())
	cancel()
	if got := programExitMessage(parent, parent, context.Canceled); got != "Program exited: killed" {
		t.Fatalf("killed exit = %q", got)
	}

	command, commandCancel := context.WithDeadline(context.Background(), time.Now().Add(-time.Second))
	defer commandCancel()
	if got := programExitMessage(context.Background(), command, context.DeadlineExceeded); got != "Program exited: process took too long." {
		t.Fatalf("timed-out exit = %q", got)
	}
}

func TestAppendProgramExitMatchesTourSpacing(t *testing.T) {
	if got := appendProgramExit("hello\n", "Program exited."); got != "hello\n\nProgram exited." {
		t.Fatalf("appendProgramExit = %q", got)
	}
	if got := appendProgramExit("", "Program exited: killed"); got != "Program exited: killed" {
		t.Fatalf("empty appendProgramExit = %q", got)
	}
}

func TestFormatTranspileTranscriptListsGeneratedFiles(t *testing.T) {
	root := t.TempDir()
	for name := range map[string]struct{}{
		"main.cs":                {},
		"package_info.cs":        {},
		"tour.local.demo.csproj": {},
	} {
		if err := os.WriteFile(filepath.Join(root, name), []byte("generated"), 0o600); err != nil {
			t.Fatal(err)
		}
	}

	got := formatTranspileTranscript("go2cs diagnostic", root, "passed", "Core source")
	for _, want := range []string{"$ go2cs main.go", "Runtime: Core source", "go2cs diagnostic", "main.cs", "tour.local.demo.csproj", "Transpile completed."} {
		if !strings.Contains(got, want) {
			t.Fatalf("transcript missing %q:\n%s", want, got)
		}
	}
}
