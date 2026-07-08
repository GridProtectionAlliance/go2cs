package main

import "fmt"

// Builder is a value type. With is a CAPTURE-MODE method: it passes the receiver pointer into a
// callback, so the receiver's address escapes the call — exactly like cryptobyte.Builder.AddASN1 in
// crypto/x509. The converter therefore routes a call on a heap-boxed value receiver through the
// receiver BOX (`Ꮡb.With(...)`), rather than the deref-aliased value form.
type Builder struct{ n int }

func (b *Builder) With(f func(*Builder)) { f(b) }
func (b *Builder) Sum() int              { return b.n }

// compute reproduces crypto/x509 marshalCertificate's shape: an INNER closure declares its own
// `var b Builder` and calls the capture-mode method `b.With` (routed through the receiver box),
// while the ENCLOSING function ALSO declares a `var b Builder` — declared LATER. In C# a lambda
// cannot declare a local named the same as one in the enclosing method scope (CS0136), so the inner
// `b` is SHADOW-renamed (`bΔ1`) and its heap box becomes `ᏑbΔ1`. The capture-mode call must reference
// that renamed box (`ᏑbΔ1.With`), not the raw `Ꮡb` — the latter collides with the outer `b`'s box
// declared later in the method (CS0841/CS0103). Guards the receiver-box shadow-rename fix in
// convSelectorExpr.
func compute() int {
	inner := func() int {
		var b Builder
		b.With(func(b *Builder) { b.n += 1 })
		b.With(func(b *Builder) { b.n += 2 })
		b.With(func(b *Builder) { b.n += 3 })
		return b.Sum()
	}

	a := inner()

	var b Builder
	b.With(func(b *Builder) { b.n += a })
	b.With(func(b *Builder) { b.n += 10 })
	return b.Sum()
}

func main() {
	fmt.Println(compute())
}
