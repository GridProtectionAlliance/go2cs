// ConstShadowsParam guards a block-scoped `const` that shadows an enclosing parameter (or var) of the
// same name — the runtime lock_sema `notetsleep_internal` shape (`func(...ns int64...)` with an inner
// `const ns = 10e6`). Go allows the shadow; C# does not (CS0136), and the converter's shadow-rename
// pass had IGNORED consts (only *types.Var were renamed), so both the param and the inner const emitted
// as `ns`, colliding. The fix renames the shadowing const (and its uses) to `nsΔ1`, leaving the param
// `ns`. This test verifies the inner uses bind the CONST value and the outer uses bind the PARAM.
package main

import "fmt"

//go:noinline
func f(ns int64) int64 {
	total := ns
	if ns < 0 {
		const ns = 10 // block const shadows the param ns
		total += int64(ns) + int64(ns)
	}
	return total + ns // param ns again
}

func main() {
	fmt.Println(f(5))  // 5<0 false -> 5 + 5 = 10
	fmt.Println(f(-3)) // -3<0 true -> total=-3+10+10=17; 17 + (-3) = 14
}
