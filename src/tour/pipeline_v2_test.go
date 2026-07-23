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
