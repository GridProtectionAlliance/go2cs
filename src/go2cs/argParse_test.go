package main

import (
	"flag"
	"reflect"
	"testing"
)

// TestParseArgsInterspersed verifies flags are honored even when they appear AFTER positional
// arguments — Go's flag package otherwise stops parsing at the first non-flag token. This is the fix
// for `go2cs -recurse . -go2cspath dir` silently dropping -go2cspath (and writing to the default
// output root instead of the one the user passed).
func TestParseArgsInterspersed(t *testing.T) {
	cases := []struct {
		name    string
		args    []string
		wantPos []string
		wantRec bool
		wantOut string
	}{
		{"flag after positional", []string{"-recurse", ".", "-go2cspath", "out"}, []string{"."}, true, "out"},
		{"flags first (unchanged)", []string{"-recurse", "-go2cspath", "out", "."}, []string{"."}, true, "out"},
		{"two positionals then flag", []string{"a.go", "b.cs", "-go2cspath", "out"}, []string{"a.go", "b.cs"}, false, "out"},
		{"stdlib-style filter after flag", []string{"-recurse", "fmt", "io", "strings"}, []string{"fmt", "io", "strings"}, true, ""},
		{"equals form after positional", []string{".", "-go2cspath=out"}, []string{"."}, false, "out"},
		{"no positionals", []string{"-recurse"}, nil, true, ""},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			fs := flag.NewFlagSet("test", flag.ContinueOnError)
			rec := fs.Bool("recurse", false, "")
			out := fs.String("go2cspath", "", "")

			pos, err := parseArgsInterspersed(fs, tc.args)

			if err != nil {
				t.Fatalf("unexpected error: %v", err)
			}

			if !reflect.DeepEqual(pos, tc.wantPos) {
				t.Errorf("positionals = %#v, want %#v", pos, tc.wantPos)
			}

			if *rec != tc.wantRec {
				t.Errorf("recurse = %v, want %v", *rec, tc.wantRec)
			}

			if *out != tc.wantOut {
				t.Errorf("go2cspath = %q, want %q", *out, tc.wantOut)
			}
		})
	}
}
