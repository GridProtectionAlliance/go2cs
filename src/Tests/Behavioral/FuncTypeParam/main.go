package main

import (
	"fmt"
	"sync/atomic"
)

// A func-typed parameter whose signature names a type from a SLASH-bearing package path
// (sync/atomic). The func-type rendering previously chopped the type string at the last '/'
// (`func(*sync/atomic.Int32) int` → `*sync/atomic.Int32) int`), producing un-compilable C#. The
// runtime package hits this (e.g. mprof.go's memProfileInternal takes a func(profilerecord.…)).
func apply(v *atomic.Int32, f func(*atomic.Int32) int) int {
	return f(v)
}

// A plain Action-style func param (no result) over a cross-package type.
func consume(v *atomic.Int32, sink func(*atomic.Int32)) {
	sink(v)
}

func main() {
	var v atomic.Int32
	v.Store(10)

	n := apply(&v, func(x *atomic.Int32) int {
		return int(x.Load()) + 5
	})
	fmt.Println(n)

	consume(&v, func(x *atomic.Int32) {
		x.Store(99)
	})
	fmt.Println(v.Load())
}
