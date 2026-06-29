package main

import "fmt"

// Width is an UNTYPED numeric constant. It is emitted as a `static readonly UntypedInt`
// wrapper in C# — NOT a C# compile-time `const` — so it is invalid as a C# `switch`
// case label (CS9135). A switch whose case label references it must therefore fall back
// to the if-else (`==`) emission form. This mirrors the runtime's `case goarch.PtrSize:`.
const Width = 8

func classify(n uintptr) string {
	switch n {
	case Width:
		return "width"
	default:
		return "other"
	}
}

// matchVar switches against plain VARIABLE case labels (runtime values, not constants).
// These are likewise invalid as C# switch case labels and must use the if-else form.
// This mirrors the runtime's `case bad:` / `case frame.fp:` shape.
func matchVar(p, a, b uintptr) string {
	switch p {
	case a:
		return "A"
	case b:
		return "B"
	default:
		return "?"
	}
}

func main() {
	fmt.Println(classify(8))
	fmt.Println(classify(3))
	fmt.Println(matchVar(1, 1, 2))
	fmt.Println(matchVar(2, 1, 2))
	fmt.Println(matchVar(9, 1, 2))
}
