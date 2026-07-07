// Guards the GENERIC self-referential constraint-proxy conversion — the crypto/elliptic
// nistCurve[Point nistPoint[Point]] shape. A generic struct whose type parameter carries a
// SELF-REFERENTIAL generic method-set constraint (`curve[Point point[Point]]`) implements a
// non-generic interface (`Curve`) via POINTER receiver and is instantiated more than one way
// (`curve[*p224]`, `curve[*p384]`). Because the C# box `ж<p224>` cannot nominally implement
// `point<ж<p224>>`, the converter substitutes a generated proxy `p224жpoint : point<itself>`
// for the type argument (implicit ж↔proxy conversions marshal every T-boundary), the generic
// struct implements the interface through ONE generic adapter `curveжCurve<Point>`, and a
// `newPoint func() Point` field's method-group initializer is re-wrapped as a lambda so the proxy
// return converts. Also exercises a tuple-returning constraint method (`restore([]byte) (T, error)`).
package main

import "fmt"

// point is a self-referential generic constraint, mirroring crypto/elliptic's nistPoint[T].
type point[T any] interface {
	label() string
	combine(T) T
	restore([]byte) (T, error)
}

// p224 and p384 are two concrete point types (mirroring nistec.P224Point / P384Point); their
// methods are POINTER-receiver, so *p224 / *p384 satisfy point[*p224] / point[*p384].
type p224 struct{ v int }

func (p *p224) label() string                 { return "p224" }
func (p *p224) combine(o *p224) *p224          { return &p224{v: p.v + o.v} }
func (p *p224) restore(b []byte) (*p224, error) { return &p224{v: int(b[0])}, nil }

func newP224() *p224 { return &p224{v: 1} }

type p384 struct{ v int }

func (p *p384) label() string                 { return "p384" }
func (p *p384) combine(o *p384) *p384          { return &p384{v: p.v * o.v} }
func (p *p384) restore(b []byte) (*p384, error) { return &p384{v: int(b[0])}, nil }

func newP384() *p384 { return &p384{v: 2} }

// curve is a generic Curve implementation over a point, mirroring
// nistCurve[Point nistPoint[Point]]. It carries a newPoint FUNC field (mirroring nistCurve's
// newPoint func() Point) plus a base value of the constrained type parameter.
type curve[Point point[Point]] struct {
	name     string
	newPoint func() Point
	base     Point
}

// Curve is the non-generic interface curve implements via POINTER receiver.
type Curve interface {
	Name() string
	BaseLabel() string
	Combined() string
	Fresh() string
}

func (c *curve[Point]) Name() string { return c.name }

func (c *curve[Point]) BaseLabel() string { return c.base.label() }

// Combined uses the constraint method that consumes AND returns the type parameter.
func (c *curve[Point]) Combined() string {
	return c.base.combine(c.base).label()
}

// Fresh calls the newPoint func field and the tuple-returning constraint method.
func (c *curve[Point]) Fresh() string {
	p := c.newPoint()
	r, _ := p.restore([]byte{7})
	return r.label()
}

func main() {
	var c1 Curve = &curve[*p224]{name: "c224", newPoint: newP224, base: &p224{v: 3}}
	var c2 Curve = &curve[*p384]{name: "c384", newPoint: newP384, base: &p384{v: 5}}

	fmt.Println(c1.Name(), c1.BaseLabel(), c1.Combined(), c1.Fresh())
	fmt.Println(c2.Name(), c2.BaseLabel(), c2.Combined(), c2.Fresh())

	for _, c := range []Curve{c1, c2} {
		fmt.Println(c.Name(), "->", c.BaseLabel(), "->", c.Combined(), "->", c.Fresh())
	}
}
