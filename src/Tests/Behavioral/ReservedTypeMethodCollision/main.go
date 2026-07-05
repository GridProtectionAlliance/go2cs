// ReservedTypeMethodCollision guards the converter's handling of a package-level
// type whose name is a golib-reserved word (here "slice") that ALSO collides with a
// method of the same name on another type. Both reserved-rename toward "Δslice"; the
// converter must give the TYPE a distinct name ("Δsliceᴛ") so the nested type and the
// extension method do not collide in C# (CS0102). This mirrors runtime's
// `type slice struct{…}` (the GC slice header) vs `func (*userArena) slice(…)`.
package main

import "fmt"

// slice is a user type named like golib's reserved slice<T>.
type slice struct {
	lo, hi int
}

func (s slice) span() int { return s.hi - s.lo }

// builder has a method ALSO named slice — the type-vs-method collision.
type builder struct {
	base int
}

func (b *builder) slice(n int) slice {
	return slice{lo: b.base, hi: b.base + n}
}

func main() {
	b := &builder{base: 10}
	s := b.slice(5)
	fmt.Println(s.lo, s.hi) // 10 15
	fmt.Println(s.span())   // 5
	fmt.Println(s)          // {10 15}

	sv := Sieve{vals: []int{1, 2, 3, 4, 5, 6}}
	var even Filter = func(n int) bool { return n%2 == 0 }
	fmt.Println(sv.Filter(even)) // 3
}

// Filter is a NON-reserved defined FUNC TYPE whose name collides with a method (Sieve.Filter).
// It Δ-prefixes to ΔFilter at every USE; the DELEGATE DECLARATION must be Δ-prefixed too, or it
// duplicates the method (CS0102) and the ΔFilter uses cannot resolve it (CS0246) — go/ast's
// `type Filter func(string) bool` vs `(CommentMap).Filter`. No reserved-word marker here: the
// method keeps the plain `Filter` and only the type becomes `ΔFilter`.
type Filter func(int) bool

type Sieve struct{ vals []int }

func (sv Sieve) Filter(f Filter) int {
	count := 0
	for _, v := range sv.vals {
		if f(v) {
			count++
		}
	}
	return count
}
