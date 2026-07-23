package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestCollectCSharp(t *testing.T) {
	root := t.TempDir()
	if err := os.WriteFile(filepath.Join(root, "b.cs"), []byte("class B {}"), 0o600); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(root, "a.cs"), []byte("class A {}"), 0o600); err != nil {
		t.Fatal(err)
	}
	if err := os.Mkdir(filepath.Join(root, "obj"), 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(root, "obj", "ignored.cs"), []byte("ignore"), 0o600); err != nil {
		t.Fatal(err)
	}

	got, err := collectCSharp(root)
	if err != nil {
		t.Fatal(err)
	}
	if strings.Index(got, "a.cs") > strings.Index(got, "b.cs") {
		t.Fatal("generated files are not stable-sorted")
	}
	if strings.Contains(got, "ignore") {
		t.Fatal("obj output was included")
	}
}

func TestFindProject(t *testing.T) {
	root := t.TempDir()
	project := filepath.Join(root, "tour.csproj")
	if err := os.WriteFile(project, []byte("<Project />"), 0o600); err != nil {
		t.Fatal(err)
	}
	got, err := findProject(root)
	if err != nil {
		t.Fatal(err)
	}
	if got != project {
		t.Fatalf("findProject = %q, want %q", got, project)
	}
}

func TestHTMLescape(t *testing.T) {
	got := htmlEscape(`<bad title="x">&`)
	if strings.Contains(got, "<bad") || !strings.Contains(got, "&amp;") {
		t.Fatalf("htmlEscape returned unsafe text: %q", got)
	}
}
