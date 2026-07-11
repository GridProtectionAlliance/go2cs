package main

import "testing"

// TestPackageQualifiedName locks the imported-type-alias namespace qualification for non-stdlib deps:
// go.<result>_package must be the imported package's converted class. The last segment is the Go
// package name (which can differ from the import-path segment, e.g. go-isatty is `package isatty`),
// and a single-segment module (namespace == the root) yields just the package name.
func TestPackageQualifiedName(t *testing.T) {
	cases := []struct {
		namespace string
		pkgName   string
		want      string
	}{
		{"go.github.com.google", "uuid", "github.com.google.uuid"},
		{"go.github.com.mattn", "isatty", "github.com.mattn.isatty"},   // path segment is go-isatty; package is isatty
		{"go.github.com.mattn", "colorable", "github.com.mattn.colorable"}, // go-colorable -> colorable
		{"go.example.com", "lib", "example.com.lib"},
		{"go", "foo", "foo"}, // single-segment module: namespace is the bare root
	}

	for _, tc := range cases {
		if got := packageQualifiedName(tc.namespace, tc.pkgName); got != tc.want {
			t.Errorf("packageQualifiedName(%q, %q) = %q, want %q", tc.namespace, tc.pkgName, got, tc.want)
		}
	}
}
