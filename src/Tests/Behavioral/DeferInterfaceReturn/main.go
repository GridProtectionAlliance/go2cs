package main

import "fmt"

// Shape is the interface both return branches below satisfy.
type Shape interface {
	Area() int
}

type Circle struct{ R int }

func (c Circle) Area() int { return 3 * c.R * c.R }

type Square struct{ S int }

func (s Square) Area() int { return s.S * s.S }

// pick has a defer/recover (so the converter wraps its body in a `func(...)` execution context) AND
// returns the Shape interface via TWO unrelated concrete types (Circle, Square). Those return
// expressions have no best-common-type, so C# cannot infer the wrapper's T from the lambda's returns
// — the wrapper must be emitted as `func<Shape>(...)` or overload resolution binds the void
// GoAction overload (CS8030 "anonymous function converted to a void returning delegate cannot return
// a value"). This mirrors go/parser's parseTypeName/parseSimpleStmt shape.
func pick(kind int) Shape {
	defer func() { _ = recover() }()
	if kind == 0 {
		return Circle{R: 2}
	}
	return Square{S: 5}
}

// classify additionally exercises a multi-value heterogeneous return (Stmt-like tuple): the first
// result is the same Shape interface reached through two concrete types, alongside a plain bool.
func classify(kind int) (Shape, bool) {
	defer func() { _ = recover() }()
	if kind == 0 {
		return Circle{R: 3}, true
	}
	return Square{S: 4}, false
}

func main() {
	fmt.Println(pick(0).Area())
	fmt.Println(pick(1).Area())

	s0, ok0 := classify(0)
	fmt.Println(s0.Area(), ok0)

	s1, ok1 := classify(1)
	fmt.Println(s1.Area(), ok1)
}
