// ShadowedVarMethodCallLHS guards a shadow-renamed variable used as the receiver of a METHOD CALL in an
// assignment TARGET — `x.get().n = …` (runtime stackpoolalloc's `x := gclinkptr(…)` loop, where the loop
// `x` is renamed `xΔ1` because a func-body `x` is declared AFTER the loop). The `x` is buried inside the
// `x.get()` call, past the selector/index descent the `=` case walks, so the LHS use kept the raw `x` —
// which, being declared later, is CS0841 (use before declaration), or a silent wrong bind. The descent
// now visits a method-call receiver in the LHS chain. Verified vs Go (write-through through the method).
package main

import "fmt"

type box struct{ n int }

func (b *box) get() *box { return b } // pointer-receiver method returning the pointer

//go:noinline
func run() int {
	var arr [3]box
	for i := 0; i < 3; i++ {
		x := &arr[i]       // loop x (shadows the func-body x below -> renamed)
		x.get().n = i * 10 // LHS method-call receiver: the loop x, via x.get()
	}
	x := &arr[1] // func-body x, declared AFTER the loop
	return x.get().n + arr[0].n + arr[2].n
}

func main() {
	fmt.Println(run()) // arr = {0,10,20}; x=&arr[1] -> 10 + 0 + 20 = 30
}
