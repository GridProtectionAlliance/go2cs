package main

import "fmt"

type node struct{ val int }

// onlyNilCheck's pointer param is used ONLY in a nil comparison (renders as the box `Ꮡp == nil`),
// so its deref VALUE alias is dead. Before the fix, `ref var p = ref Ꮡp.Value` NRE'd on a nil arg.
func onlyNilCheck(p *node) string {
	if p == nil {
		return "nil"
	}
	return "set"
}

// inner/passThrough: the param is forwarded as a POINTER (box) only — value alias dead.
func inner(p *node) bool      { return p == nil }
func passThrough(p *node) bool { return inner(p) }

// usesValue DEREFERENCES its param — the value alias must be KEPT.
func usesValue(p *node) int { return p.val }

func main() {
	fmt.Println(onlyNilCheck(nil))         // nil  (NRE'd before the fix)
	fmt.Println(onlyNilCheck(&node{}))     // set
	fmt.Println(passThrough(nil))          // true (NRE'd before the fix)
	fmt.Println(passThrough(&node{}))      // false
	fmt.Println(usesValue(&node{val: 9}))  // 9
}
