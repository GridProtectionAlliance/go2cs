// Regression test: an `unsafe.Pointer` parameter whose name is a C# keyword.
//
// When an identifier of type `unsafe.Pointer` is used in a pointer context (e.g. a
// comparison operand), the converter emits the box-value deref form `name.val`. It built
// that string from the raw Go name, so a parameter named `new` — a reserved C# keyword —
// came out as `new.val`, which C# parses as the `new` operator (CS1526). This is exactly
// how internal/runtime/atomic's `CompareAndSwap(old, new unsafe.Pointer)` failed to compile.
// The fix sanitizes the name first, so it is emitted as `@new.val`.
package main

import (
	"fmt"
	"unsafe"
)

// `new` and `type` are C# keywords; both are valid Go identifiers used here as
// unsafe.Pointer parameters in comparison contexts (which trigger the `.val` deref).
func sameAs(old, new unsafe.Pointer) bool {
	return new == old
}

func notNil(new unsafe.Pointer) bool {
	return new != nil
}

func main() {
	a, b := 1, 2
	pa := unsafe.Pointer(&a)
	pb := unsafe.Pointer(&b)

	fmt.Println(sameAs(pa, pa)) // true
	fmt.Println(sameAs(pa, pb)) // false
	fmt.Println(notNil(pa))     // true
	fmt.Println(notNil(nil))    // false
}
