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

// A SLICE of POINTERS to a cross-package type - `[]*atomic.Int32` - was rendered from the
// package-qualified string `[]*sync/atomic.Int32`, whose cross-package slash-strip ate
// everything before the last slash INCLUDING the `*`, silently dropping the pointer marker
// (`slice<atomic.Int32>` instead of `slice<ж<atomic.Int32>>` - reflect's `[]*abi.Type`
// fields/asserts, CS1503 x16 plus type asserts that compiled against the WRONG type). The
// type assert and the var declaration below both exercise that render path.
func last(vals any) *atomic.Int32 {
	ptrs := vals.([]*atomic.Int32)
	return ptrs[len(ptrs)-1]
}

func main() {
	var x counters
	touch(&x.c[0])
	touch(&x.c[2])

	var ptrs []*atomic.Int32
	for i := range x.c {
		ptrs = append(ptrs, &x.c[i])
	}
	last(ptrs).Store(7)
	fmt.Println(len(x.c), len(x.d), x.c[2].Load())
}
