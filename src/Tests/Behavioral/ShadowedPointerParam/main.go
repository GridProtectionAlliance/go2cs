package main

import "fmt"

// A local variable that SHADOWS a pointer parameter of the same name (`func f(t *T) { … var t *T …
// }`) is renamed (`tΔ2`) but is a plain pointer local (`ж<T> tΔ2`), NOT the parameter's deref-aliased
// box. When passed to a function expecting `*T`, it must be passed as-is (`use(tΔ2)`), not address-
// taken (`use(ᏑtΔ2)` — the spurious `&` references an undefined `ᏑtΔ2` box, CS0103). The defect was
// that identIsParameter matched by NAME, so the shadowing local was wrongly treated as the
// parameter. Mirrors runtime's `mapKeyError2(t *_type, …)` with its inner `var t *_type`.

type T struct{ n int }

func use(t *T) int { return t.n }

func outer(t *T) int {
	r := use(t) // the PARAMETER t -> use(Ꮡt) (deref-aliased param box)
	switch {
	default:
		var t *T = &T{n: 5} // inner t shadows the param; a plain pointer local
		r += use(t)         // the LOCAL t -> use(tΔ2), NOT use(ᏑtΔ2)
		r += (*t).n         // deref the local pointer
	}
	return r
}

func main() {
	fmt.Println(outer(&T{n: 1})) // 1 + 5 + 5 = 11
}
