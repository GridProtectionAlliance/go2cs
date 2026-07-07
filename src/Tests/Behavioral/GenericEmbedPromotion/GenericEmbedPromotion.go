// Guards PROMOTION of a GENERIC-INSTANTIATION embed — the crypto/elliptic
// `p256Curve struct { nistCurve[*nistec.P256Point] }` shape. A non-generic struct EMBEDS a
// concrete instantiation of a generic wrapper whose type parameter carries a SELF-REFERENTIAL
// generic method-set constraint (`curve[Point point[Point]]`). Because the C# box `ж<p224>`
// cannot nominally implement `point<ж<p224>>`, the converter substitutes a generated proxy
// `p224жpoint : point<itself>` for the type argument — so the embed reads
// `curve<p224жpoint>`, whose marker glyph (ж) is EMBEDDED in the type-argument identifier.
//
// The generator must promote the generic wrapper's INTERNAL fields (`name`, `newPoint`, `seed`)
// and METHODS (`Tag`, `SeedLabel`, `LabelOf`, `Fresh`) onto the embedding struct, rewriting the
// wrapper's type PARAMETER `Point` to the instantiation's type ARGUMENT `p224жpoint` in every
// promoted field/method signature. A bare-`ж` scan that mistook the mid-identifier marker for a
// pointer prefix mangled the embed's simple name (`curve<…ж…>` → `oint.Value`) so nothing
// promoted (CS1061/CS1929/CS1501); resolving the instantiation to its generic declaration and
// substituting the type argument fixes it.
package main

import "fmt"

// point is a self-referential generic constraint, mirroring crypto/elliptic's nistPoint[T].
type point[T any] interface {
	label() string
	combine(T) T
	restore([]byte) (T, error)
}

// p224 is a concrete point type (mirroring nistec.P224Point); its methods are POINTER-receiver,
// so *p224 satisfies point[*p224] and drives the generated proxy p224жpoint.
type p224 struct{ v int }

func (p *p224) label() string                   { return "p224" }
func (p *p224) combine(o *p224) *p224            { return &p224{v: p.v + o.v} }
func (p *p224) restore(b []byte) (*p224, error)  { return &p224{v: int(b[0])}, nil }

func newP224() *p224 { return &p224{v: 1} }

// curve is a generic wrapper over a point, mirroring nistCurve[Point nistPoint[Point]]. Its
// INTERNAL fields and methods must promote onto any struct that embeds a concrete instantiation.
type curve[Point point[Point]] struct {
	name     string
	newPoint func() Point
	seed     Point
}

func (c *curve[Point]) Tag() string { return c.name }

func (c *curve[Point]) SeedLabel() string { return c.seed.label() }

// LabelOf takes the type PARAMETER as a parameter — its promoted forwarder must render the
// parameter type as the instantiation's proxy type argument.
func (c *curve[Point]) LabelOf(p Point) string { return p.label() }

// Fresh calls the newPoint func field and the tuple-returning constraint method.
func (c *curve[Point]) Fresh() string {
	p := c.newPoint()
	r, _ := p.restore([]byte{7})
	return r.label()
}

// Named is a NON-generic interface that p224Curve satisfies THROUGH its promoted methods (mirrors
// p256Curve implementing elliptic.Curve via nistCurve's promoted methods). Calling through it forces
// the generated interface adapter to bind the promoted method shims on ж<p224Curve>, so a regression
// that stops promoting the embed's methods fails to compile (CS1929) rather than silently passing.
type Named interface {
	Tag() string
	SeedLabel() string
	Fresh() string
}

// p224Curve EMBEDS a concrete instantiation (mirrors p256Curve embedding nistCurve[*P256Point]);
// curve's internal fields + methods must promote onto it.
type p224Curve struct {
	curve[*p224]
}

func main() {
	pc := &p224Curve{curve[*p224]{name: "p224c", newPoint: newP224, seed: &p224{v: 3}}}

	// Promoted INTERNAL field, and a promoted method whose PARAMETER is the type parameter
	// (passed the promoted, proxy-typed field) — exercises type-argument substitution.
	fmt.Println(pc.name)
	fmt.Println(pc.LabelOf(pc.seed))

	// Promoted METHODS reached through the non-generic interface adapter.
	var n Named = pc
	fmt.Println(n.Tag())
	fmt.Println(n.SeedLabel())
	fmt.Println(n.Fresh())
}
