// Regression test: a composite literal of a *named* array or slice type (e.g. `type d [3]rune`,
// `type s []int`) must construct the lowered struct via its underlying-collection constructor —
// `new d(new rune[]{...}.array())` — not a C# collection-initializer `new d{...}`, which fails
// with CS1061 (the generated array/slice struct has no Add). This is the unicode `CaseRange.Delta`
// (`type d [MaxCase]rune`) table pattern: `d{0, upperLower, 0}`.
package main

import "fmt"

// named array type (the unicode `d` pattern)
type triple [3]rune

// named slice type
type nums []int

func main() {
	// named array composite, positional
	t := triple{7, 32, 9}
	fmt.Println(t[0], t[1], t[2])

	// named array composite with an arithmetic element (mirrors unicode's `305 - 73`)
	u := triple{0, 305 - 73, 0}
	fmt.Println(u[1])

	// named slice composite
	n := nums{1, 2, 3, 4}
	fmt.Println(len(n), n[3])

	// named array composite as a function-argument position (mirrors unicode's
	// `new CaseRange(..., d{0, 32, 0})`)
	fmt.Println(first(triple{11, 22, 33}))
}

func first(t triple) rune {
	return t[0]
}
