// Regression test: a `var` declaration nested in a block must be shadow-renamed when it collides
// with a function-level variable of the same name declared LATER in the function. The function-
// level-declaration pre-pass collected nested `var` decls without the function-level gate that
// every other declaration kind has, so a nested `var z` was recorded as *the* function-level `z`,
// masking the real one (`z := x*x`); neither got renamed and both emitted `z` -> CS0136. This is
// the math/j0 J0/Y0 (and j1) `var z float64` + later `z := x*x` pattern.
package main

import (
	"fmt"
	"strings"
)

func f(x int) int {
	if x >= 2 {
		var z int // nested `var` decl; collides with the later function-level z
		if x > 5 {
			z = 100
		} else {
			z = 200
		}
		return z
	}
	z := x * x // function-level, declared textually AFTER the nested `var z`
	return z
}

type tagErr struct{ tag string }

func (e *tagErr) Error() string { return "tag:" + e.tag }

func check(s string) (int, error) {
	if s == "" {
		return 0, &tagErr{tag: "empty"}
	}
	return len(s), nil
}

// g: the inner err is shadow-renamed (the function-level err below is declared LATER); the
// rename must also reach idents inside a nested if-INIT's comma-ok type assert - the init
// walk only visited single-CallExpr RHS shapes, so the asserted `err` kept its raw name
// (fmt convertFloat's `if e, ok := err.(*strconv.NumError); ok`, CS0841/CS8130 x6).
func g(s string) string {
	if n := len(s); n >= 0 {
		v, err := check("")
		if err != nil {
			if e, ok := err.(*tagErr); ok {
				e.tag = "inner"
			}
			return err.Error()
		}
		_ = v
	}
	v, err := check(s)
	if err != nil {
		return "outer"
	}
	return fmt.Sprint(v)
}

// pkgShadow: a local named after an IMPORTED PACKAGE the function uses. Go resolves
// `strings.Repeat` before the local's declaration; C#'s whole-block scoping binds the
// simple name to the local for the entire method, so the package reference would hit the
// not-yet-declared local (internal/zstd fse's `bits := tableBits - highBit` after
// `bits.LeadingZeros16(...)`, CS0841). The local is shadow-renamed (stringsD1).
func pkgShadow(s string) string {
	doubled := strings.Join([]string{s, s}, "-")
	strings := len(doubled)
	return fmt.Sprint(strings)
}

func main() {
	fmt.Println(f(10)) // 100
	fmt.Println(f(3))  // 200
	fmt.Println(f(1))  // 1
	fmt.Println(g("ab")) // tag:inner
	fmt.Println(pkgShadow("xy")) // 5
}
