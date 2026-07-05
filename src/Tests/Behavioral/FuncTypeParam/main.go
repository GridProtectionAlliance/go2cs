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

// runner holds a func-typed FIELD whose signature names a pointer to a type from a SLASH-bearing
// (multi-segment) import path — the exact shape of testing/quick's
// `Config.Values func([]reflect.Value, *rand.Rand)`. A struct field renders via the string display
// path (getDisplayTypeName), which stringifies the signature as `func(*sync/atomic.Int32) int` and
// then splits the slash-bearing import path on '/', mis-qualifying it (CS0426). The field must
// render structurally as a delegate so the pointer element keeps its `atomic` package-name alias.
type runner struct {
	gen func(*atomic.Int32) int
}

func (r runner) invoke(v *atomic.Int32) int {
	return r.gen(v)
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

	// A struct with a func-typed field over a cross-package (slash-path) pointer type.
	double := runner{gen: func(x *atomic.Int32) int {
		return int(x.Load()) * 2
	}}
	fmt.Println(double.invoke(&v))

	seven := runner{gen: func(x *atomic.Int32) int {
		return 7
	}}
	fmt.Println(seven.invoke(&v))
}
