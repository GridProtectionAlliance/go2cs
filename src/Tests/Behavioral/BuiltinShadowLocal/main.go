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

func main() {
	fmt.Println(sumWithLenLocal([]int{10, 20, 30})) // 60 + 3 = 63
	fmt.Println(sumWithLenLocal(nil))               // 0
	fmt.Println(capPlusOne(make([]int, 2, 5)))      // 5 + 1 = 6
}
