package main

import (
	"fmt"
	"sync/atomic"
)

// A struct ARRAY field whose element type comes from a MULTI-SEGMENT-PATH package
// (`[3]atomic.Int32` / `[2]atomic.Uint64`, atomic = sync/atomic) must keep its `array<>`
// wrapper. The field type was rendered from the package-qualified string
// `[3]sync/atomic.Int32`, whose cross-package slash-strip ALSO removed the leading `[3]`,
// dropping the wrapper — emitting `atomic.Int32 c = new(3)` instead of
// `array<atomic.Int32> c = new(3)`, so the array `new(3)` initializer mis-bound the element
// type's constructor (CS1503 int->NilType). `len()` over the field compiles only when the
// wrapper is present. Mirrors the runtime's trace fields `[2]atomic.Pointer[profBuf]` and
// `[4]atomic.UnsafePointer`. (Element method calls on an array element are a separate,
// independent matter — an indexed-element atomic receiver — so they are not exercised here.)
type counters struct {
	c [3]atomic.Int32
	d [2]atomic.Uint64
}

func main() {
	var x counters
	fmt.Println(len(x.c), len(x.d))
}
