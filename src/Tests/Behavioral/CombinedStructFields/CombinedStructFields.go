package main

import "fmt"

// point groups multiple field names per declaration. The converter should emit a
// single combined C# line per group (`internal nint x, y;`) to mirror the Go source.
type point struct {
	x, y       int
	a, b       string
	wide, tall float64
}

// mixed exercises the per-name fallbacks: a group with differing access modifiers
// (Exported, unexported) must stay on separate C# lines, while uniform groups combine.
type mixed struct {
	X, y  int    // differing access -> separate lines
	p, q  int    // uniform -> combined
	label string // single name -> single line
}

func main() {
	p := point{x: 1, y: 2, a: "h", b: "i", wide: 3.5, tall: 4.5}
	fmt.Println(p.x, p.y, p.a, p.b, p.wide, p.tall)

	m := mixed{X: 10, y: 20, p: 30, q: 40, label: "tag"}
	fmt.Println(m.X, m.y, m.p, m.q, m.label)
}
