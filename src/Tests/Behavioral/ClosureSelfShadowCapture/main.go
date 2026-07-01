// ClosureSelfShadowCapture guards the closure self-shadow-initializer case. When a closure captures an
// outer variable `s` and its body declares an inner `s := f(s)` (the inner shadows the captured outer;
// the RHS uses the OUTER s), the converter's lambda-capture map — keyed by NAME — used to rename BOTH the
// captured use and the distinct inner binding to the same capture name `sʗN`, so the inner decl's RHS
// bound to itself (`var sʗ3 = …(~sʗ3)…`) → CS0841. This is runtime mgcsweep's `systemstack(func(){ s :=
// spanOf(uintptr(unsafe.Pointer(s.largeType))) … })`. The capture name is now applied only when an ident
// resolves to the exact captured object; the inner binding keeps its own (shadow-renamed) name.
package main

import "fmt"

type span struct {
	largeType int
	val       int
}

// onstack mirrors runtime `systemstack`: a func literal passed as a call argument.
//
//go:noinline
func onstack(f func()) { f() }

//go:noinline
func run() int {
	s := &span{largeType: 5, val: 100} // outer s (a pointer -> captured)
	total := 0
	onstack(func() {
		// inner `s :=` self-shadows the captured outer s; the RHS uses the OUTER s (mgcsweep shape:
		// `s := spanOf(uintptr(unsafe.Pointer(s.largeType)))`).
		s := &span{largeType: s.largeType * 2, val: s.val + 1}
		total += s.largeType + s.val // inner: 10 + 101 = 111
	})
	total += s.val // outer s still 100
	return total   // 111 + 100 = 211
}

func main() {
	fmt.Println(run())
}
