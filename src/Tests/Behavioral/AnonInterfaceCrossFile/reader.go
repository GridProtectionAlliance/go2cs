// File A: declares a function whose parameter is an ANONYMOUS INTERFACE composed of two
// embedded named interfaces (`interface{ Sizer; Namer }`). The converter LIFTS this inline
// interface to a named C# type (e.g. `describe_thing`) recorded in the package-level dynamic
// registry so file B's call site — visited in a different (possibly earlier) file — can
// resolve the same lifted name for both the GoImplement assembly attribute and the pointer
// adapter class. Mirrors internal/trace's `readBatch(r interface{io.Reader; io.ByteReader})`
// lifted in batch.go and cast cross-file at generation.go.
package main

import "fmt"

type Sizer interface {
	Size() int
}

type Namer interface {
	Name() string
}

// describe takes an ANONYMOUS interface embedding both named interfaces. The parameter type
// is the lifted anon interface; a concrete *box is cast to it at the file-B call site.
func describe(thing interface {
	Sizer
	Namer
}) string {
	return fmt.Sprintf("%s(%d)", thing.Name(), thing.Size())
}
