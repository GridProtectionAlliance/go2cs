// Regression test: an EXPORTED named func type (a C# delegate) whose signature references an
// UNEXPORTED type — x/text/unicode/bidi's `type Option func(*options)`, where `options` is
// package-private.
//
// Go allows an exported func type to name an unexported type in its signature. The converter emits
// the exported type as a `public delegate`, but the unexported `options` struct is `internal`, so
// the public delegate's parameter is less accessible than the delegate — CS0059. The converter must
// PUBLICIZE the unexported type (emit it `public`) when it appears in an exported func type's
// signature, the same way it already does for an exported struct field or method parameter.
package main

import "fmt"

// options is unexported; without publicizing it is `internal` and the public Option delegate below
// cannot reference it (CS0059).
type options struct {
	level int
}

// Option is an EXPORTED func type -> a public C# delegate over `ж<options>`.
type Option func(*options)

func withLevel(n int) Option {
	return func(o *options) { o.level = n }
}

func apply(opts ...Option) int {
	o := &options{}
	for _, opt := range opts {
		opt(o)
	}
	return o.level
}

func main() {
	fmt.Println(apply(withLevel(5), withLevel(9)))
}
