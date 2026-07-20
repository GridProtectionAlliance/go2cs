// Regression test: a local variable whose name shadows a Go built-in function that
// is also called in the same function (e.g. `len := len(buf)` in hash/maphash's
// rthash).
//
// Go permits this — the call still resolves to the predeclared built-in, and after the
// declaration the name refers to the local. But in C# the built-in is a `using static
// go.builtin` method, so a same-named local shadows it: the call `len(...)` binds to the
// (non-invocable) local, producing CS0149 ("method name expected") and CS0841 ("use
// before declaration"). The converter now renames such a local (to `lenΔ1`) while leaving
// the built-in call as `len`, so both resolve correctly.
package main

import "fmt"

// `len` shadows the len built-in, which is also called on the right-hand side.
func sumWithLenLocal(buf []int) int {
	if len(buf) == 0 {
		return 0
	}
	len := len(buf) // local `len` initialized from the built-in `len`
	total := 0
	for i := 0; i < len; i++ {
		total += buf[i]
	}
	return total + len // `len` here is the local
}

// `cap` shadows the cap built-in similarly.
func capPlusOne(s []int) int {
	cap := cap(s)
	return cap + 1
}

// signame is a PACKAGE function whose name a local shadows in its own initializer — the
// runtime panic.go shape `signame := signame(gp.sig)`. Go starts the shadow AFTER the
// initializer, so the call resolves to the function; C# scopes the local over its own
// initializer, so without the rename the call binds the (non-invocable) string local
// (CS0149). The local renames (signameΔ1); the call stays `signame(...)`.
func signame(sig int) string {
	if sig == 9 {
		return "SIGKILL"
	}
	return "SIG?"
}

func describeSignal(sig int) string {
	signame := signame(sig) // local shadows the package func in its own initializer
	if signame != "" {
		return "[" + signame + "]"
	}
	return "none"
}

type arr [3]int

// The built-ins still mean the built-ins in a scope that does NOT shadow them.
func unshadowed() (int, int) {
	s := make([]int, 2, 5)
	return len(s), cap(s)
}

// shadowedCalls declares locals that shadow built-ins and then CALLS them through those
// names — the math/big test shape `make := func(z *Int) *Int { … }; make(test.z)`. Go
// resolves each call to the local, not the built-in. The converter's built-in arms are keyed
// on the identifier's NAME, so before they were gated on the name actually resolving to the
// universe built-in these emitted built-in semantics instead: `make(21)` became `new nint()`
// and `new(7)` became `@new<nint>()` (both dropping the argument, CS1503/CS1929), `panic(5)`
// became the STATEMENT `throw panic(5)` inside an expression, `print`/`println` picked up a
// spurious variadic cast, and `len`/`cap` on a pointer-to-named-array gained a `.Value` deref.
func shadowedCalls() {
	a := arr{1, 2, 3}

	make := func(n int) int { return n * 2 }
	new := func(n int) int { return n * 3 }
	panic := func(n int) int { return n * 4 }
	print := func(n int) int { return n * 5 }
	println := func(n int) int { return n * 6 }
	len := func(p *arr) int { return p[0] + 100 }
	cap := func(p *arr) int { return p[1] + 200 }

	fmt.Println("shadowed", make(21), new(7), panic(5))
	fmt.Println("shadowed", print(4), println(3), len(&a), cap(&a))
}

func main() {
	fmt.Println(sumWithLenLocal([]int{10, 20, 30})) // 60 + 3 = 63
	fmt.Println(sumWithLenLocal(nil))               // 0
	fmt.Println(capPlusOne(make([]int, 2, 5)))      // 5 + 1 = 6
	fmt.Println(describeSignal(9))                  // [SIGKILL]
	fmt.Println(describeSignal(1))                  // [SIG?]

	shadowedCalls()

	l, c := unshadowed()
	fmt.Println("builtin", l, c) // builtin 2 5
}
