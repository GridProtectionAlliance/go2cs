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
//
// Taking the address of a cross-package-typed element — `&x.c[i]` — emits the golib element-
// address `…at<E>(i)`. Its element type E is rendered FULLY-QUALIFIED
// (`at<sync.atomic_package.Int32>`), which resolves inside `namespace go;` without a file-local
// `using atomic` alias — a file may index such a field without naming the element type, so it
// would otherwise lack the alias (CS0246 in the runtime's tracecpu).
type counters struct {
	c [3]atomic.Int32
	d [2]atomic.Uint64
}

func touch(p *atomic.Int32) {}

func main() {
	var x counters
	touch(&x.c[0])
	touch(&x.c[2])
	fmt.Println(len(x.c), len(x.d))
}
