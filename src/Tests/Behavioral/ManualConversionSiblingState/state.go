package main

// schedlike mirrors the runtime.schedt shape: a field whose type is an ANONYMOUS struct. The
// converter lifts the anonymous struct to a package-level C# type and registers the lifted name
// in the package-wide registry — state a marker-skipped visit must still contribute so sibling
// files can resolve cross-file references to it.
type schedlike struct {
	disable struct {
		user bool
		n    int32
	}
	label string
}

// Package-level state consumed by the SIBLING file (main.go): a global whose nested
// anonymous-struct field gets addressed there, and a package var assigned there.
var sched schedlike

var newprocs int32
