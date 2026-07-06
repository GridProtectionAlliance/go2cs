// Guards the generic-over-interface constraint lowering: a type parameter constrained
// by a REGULAR method-set interface (`totalArea[S Shape]`) emits `where S : Shape`
// against the arity-0 interface (NOT the phantom CRTP `Shape<S>, new()` — CS0308), and
// a call site instantiating the type parameter with a POINTER type projects the slice
// element-wise through the generated pointer adapter (`widen<ж<Circle>, Shape>(...)`),
// since the `ж<Circle>` box does not implement the interface — its adapter does.
// Mirrors go/ast's `walkList[N Node](v Visitor, list []N)`.
package main

import "fmt"

type Shape interface {
	Area() float64
	Name() string
}

// Round embeds Shape — an interface type argument through inheritance mirrors
// ast.Stmt/Expr/Spec/Decl instantiating walkList's N.
type Round interface {
	Shape
	Diameter() float64
}

type Circle struct {
	R float64
}

func (c *Circle) Area() float64 {
	return 3.0 * c.R * c.R
}

func (c *Circle) Name() string {
	return "circle"
}

func (c *Circle) Diameter() float64 {
	return 2.0 * c.R
}

type Square struct {
	S float64
}

func (s *Square) Area() float64 {
	return s.S * s.S
}

func (s *Square) Name() string {
	return "square"
}

// totalArea calls a constraint method ON the type parameter (richer than walkList).
func totalArea[S Shape](shapes []S) float64 {
	var sum float64
	for _, s := range shapes {
		sum += s.Area()
	}
	return sum
}

// walkAll mirrors go/ast's walkList exactly: the type parameter only WIDENS to the
// interface inside the body.
func walkAll[S Shape](shapes []S) {
	for _, s := range shapes {
		show(s)
	}
}

func show(s Shape) {
	fmt.Printf("%s: %.2f\n", s.Name(), s.Area())
}

func main() {
	circles := []*Circle{&Circle{R: 1}, &Circle{R: 2}} // pointer instantiation — adapter-projected
	squares := []*Square{&Square{S: 3}}                // second pointer type — distinct adapter
	shapes := []Shape{&Circle{R: 1}, &Square{S: 2}}    // interface instantiation — direct
	rounds := []Round{&Circle{R: 4}}                   // embedded-interface instantiation

	fmt.Printf("circles: %.2f\n", totalArea(circles))
	fmt.Printf("squares: %.2f\n", totalArea(squares))
	fmt.Printf("shapes: %.2f\n", totalArea(shapes))
	fmt.Printf("rounds: %.2f\n", totalArea(rounds))

	walkAll(circles)
	walkAll(shapes)
	walkAll(rounds)
}
