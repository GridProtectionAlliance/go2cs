// IncDecPointerField guards `++`/`--` applied to a FIELD reached through a pointer
// LOCAL. Increment/decrement reads and writes its operand, so it must be assignable —
// a field accessed through the value-returning `~` dereference is an rvalue
// (`(~mp).ncgocall++` is CS1059). The converter emits the assignable `.val` form,
// `mp.val.ncgocall++`. Mirrors runtime's `(~mp).ncgocall++` in cgocall.
package main

import "fmt"

type inner struct{ k int }

type counter struct {
	n   int
	sub inner
}

//go:noinline
func get(c *counter) *counter { return c }

func main() {
	base := &counter{n: 5}
	base.sub.k = 3
	c := get(base) // c is a *counter LOCAL (heap box, not a ref-aliased parameter)

	c.n++     // direct field ++ through the pointer local
	c.n++     // 7
	c.sub.k-- // nested field -- through the pointer local

	// Read back through the original pointer to confirm the writes reached real storage.
	fmt.Println(base.n, base.sub.k) // 7 2
}
