// main.go imports ONLY fmt and references unsafe.Pointer only through the inferred scalar
// local `p` (never writing `unsafe.` textually) — the preempt.go pattern. The composite
// `[]unsafe.Pointer` and blank-import variants live in their own consumer files so each
// alias-emission path is exercised in isolation.
package main

import "fmt"

func main() {
	var base int64 = 100
	p := ptrOf(&base) // p : unsafe.Pointer (inferred) -> @unsafe.Pointer, no unsafe import

	fmt.Println(p == nil)                  // false
	fmt.Println(p != nil)                  // true
	fmt.Println(isNil(p))                  // false
	fmt.Println(compositeLen())            // 1
	fmt.Println(blankImportInfersUnsafe()) // false
}
