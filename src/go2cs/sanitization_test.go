package main

import "testing"

// TestReplaceInvalidIdentifierChars covers the import-path characters (hyphen, tilde) that are legal
// in a Go path element but illegal in a C# identifier; dots and valid characters are preserved.
func TestReplaceInvalidIdentifierChars(t *testing.T) {
	cases := map[string]string{
		"go-cmp":      "go_cmp",
		"go-isatty":   "go_isatty",
		"foo~bar":     "foo_bar",
		"a-b~c":       "a_b_c",
		"clean":       "clean",
		"github.com":  "github.com", // dot (separator) preserved
		"under_score": "under_score",
	}

	for in, want := range cases {
		if got := replaceInvalidIdentifierChars(in); got != want {
			t.Errorf("replaceInvalidIdentifierChars(%q) = %q, want %q", in, got, want)
		}
	}
}

// TestGetCoreSanitizedIdentifierPaths locks the namespace-segment sanitization for real-world import
// paths: a hyphen in any segment becomes an underscore, and a keyword embedded after a dot in a
// segment (the `in` of gopkg.in) is @-escaped. Stdlib-style segments are unchanged.
func TestGetCoreSanitizedIdentifierPaths(t *testing.T) {
	cases := map[string]string{
		"go-cmp":        "go_cmp",        // intermediate-segment hyphen (github.com/google/go-cmp/...)
		"go-sql-driver": "go_sql_driver", // multiple hyphens
		"gopkg.in":      "gopkg.@in",     // embedded C# keyword after a dot -> gopkg.@in
		"github.com":    "github.com",    // no keyword sub-token
		"internal":      "@internal",     // `internal` is a C# keyword -> escaped (stdlib internal/abi)
		"fmt":           "fmt",
	}

	for in, want := range cases {
		if got := getCoreSanitizedIdentifier(in); got != want {
			t.Errorf("getCoreSanitizedIdentifier(%q) = %q, want %q", in, got, want)
		}
	}
}
