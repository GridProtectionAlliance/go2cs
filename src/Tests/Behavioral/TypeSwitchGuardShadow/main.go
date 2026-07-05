// Regression test: a Go type-switch guard `switch x := x.(type)` whose variable shadows an
// enclosing variable (a parameter here) must rename the guard in the converted C#. The guard's
// per-case variable is recorded in info.Implicits (not info.Defs), so the shadow analysis missed
// it and emitted `case T x:` colliding with the enclosing `x` → CS0136. This is the image/color
// `nYCbCrAModel(c Color)` pattern. The guard is renamed (xΔ1) within the switch; references after
// the switch still resolve to the enclosing variable, matching Go's scoping.
//
// Also guards the EMPTY type-switch case: a Go `case C:` with no body does nothing and BREAKS
// out of the switch (falling to the trailing return). The converted C# case must emit an explicit
// `break;` or it falls through to the next case label (CS0163). The empty case runs no visitStmt,
// so the converter's per-case `lastStatementWasReturn` flag would stay STALE from the prior case's
// `return` and wrongly suppress the break — this is text/template/parse's IsEmptyTree.
package main

import "fmt"

type A struct{ v int }
type B struct{ v int }
type C struct{ v int }

func describe(c any) string {
	switch c := c.(type) { // guard c shadows the parameter c
	case A:
		return fmt.Sprintf("A:%d", c.v) // c is the typed guard
	case C: // EMPTY body — Go breaks out of the switch; the C# case must not fall through to B
	case B:
		return fmt.Sprintf("B:%d", c.v)
	}
	return fmt.Sprintf("other:%v", c) // c is the parameter (guard out of scope)
}

func main() {
	fmt.Println(describe(A{1}))
	fmt.Println(describe(B{2}))
	fmt.Println(describe(C{3})) // empty case → other (C's value falls to the trailing return)
	fmt.Println(describe(99))
}
