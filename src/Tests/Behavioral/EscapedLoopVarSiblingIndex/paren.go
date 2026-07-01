// parenIndex exercises a shadow-renamed variable used as an LHS INDEX through a PAREN-rooted target —
// `(*p)[i] = …` (Go must parenthesize: `*p[i]` parses as `*(p[i])`). getIdentifier returns nil for such
// a target, so the LHS-index rename must still run (it descends the ParenExpr/StarExpr chain). Without
// that, `i` (a shadow) kept the enclosing name → wrong index (silent) or a compile error.
package main

//go:noinline
func parenIndex() int {
	s := []int{0, 0, 0}
	p := &s
	i := 100 // the OUTER i (would be the wrong index if the shadow leaked)
	{
		i := 1        // block shadow
		(*p)[i] = 9   // LHS index through (*p) — must write s[1], not s[100]
	}
	_ = i
	return s[1] // 9
}
