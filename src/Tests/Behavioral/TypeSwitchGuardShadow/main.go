// Regression test: a Go type-switch guard `switch x := x.(type)` whose variable shadows an
// enclosing variable (a parameter here) must rename the guard in the converted C#. The guard's
// per-case variable is recorded in info.Implicits (not info.Defs), so the shadow analysis missed
// it and emitted `case T x:` colliding with the enclosing `x` → CS0136. This is the image/color
// `nYCbCrAModel(c Color)` pattern. The guard is renamed (xΔ1) within the switch; references after
// the switch still resolve to the enclosing variable, matching Go's scoping.
package main

import "fmt"

type A struct{ v int }
type B struct{ v int }

func describe(c any) string {
	switch c := c.(type) { // guard c shadows the parameter c
	case A:
		return fmt.Sprintf("A:%d", c.v) // c is the typed guard
	case B:
		return fmt.Sprintf("B:%d", c.v)
	}
	return fmt.Sprintf("other:%v", c) // c is the parameter (guard out of scope)
}

func main() {
	fmt.Println(describe(A{1}))
	fmt.Println(describe(B{2}))
	fmt.Println(describe(99))
}
