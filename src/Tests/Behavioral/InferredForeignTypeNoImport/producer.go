// InferredForeignTypeNoImport guards the GENERIC form of the cross-package type-reference alias
// emission (the non-unsafe.Pointer arm of the same mechanism — see UnsafePointerInferredNoImport). A
// cross-package NAMED type renders in short-alias form (`strings.Reader`), which resolves only via the
// file-local alias `using strings = strings_package;`. That alias is emitted when a file imports the
// package under its canonical name; a consumer file can reference the type WITHOUT importing it — a
// same-package function returns the foreign type, so the caller infers a local of that type but never
// writes `strings.` and need not import `strings`. This producer file DOES import strings; main.go (the
// consumer) imports only fmt. Checks are a nil comparison and an int (no reliance on foreign-type
// formatting), so output is deterministic.
package main

import "strings"

//go:noinline
func makeReader() *strings.Reader { return strings.NewReader("hi") }
