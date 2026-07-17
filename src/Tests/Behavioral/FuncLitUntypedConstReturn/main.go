// FuncLitUntypedConstReturn guards a func literal with a DECLARED single numeric result whose
// return arms reference NAMED untyped constants, in natural-inference (assignment) position —
// strings/bytes TestMap's `maxRune := func(rune) rune { return unicode.MaxRune }`. The const
// reference emits as a golib Untyped* wrapper whose implicit conversions run BOTH ways with
// every numeric type, so C# either infers the wrapper delegate (Func<rune, UntypedInt> —
// CS1503 at the invariant-delegate call site) or, with mixed const/typed arms, finds no best
// common type at all (CS8917). The converter now states the declared return type explicitly
// (`var maxFn = rune (rune _) => maxRune`). A literal passed directly as a call argument is
// target-typed by the delegate parameter and stays unprefixed. Verified vs Go.
package main

import "fmt"

const maxRune = '\U0010FFFF' // untyped rune; mirrors unicode.MaxRune
const runeSelf = 0x80        // untyped int; mirrors utf8.RuneSelf
const bigConst = 1 << 40     // untyped int, beyond int32

func apply(f func(rune) rune, r rune) rune { return f(r) }

func main() {
	// Single arm returning a named untyped constant: without the explicit return type the
	// lambda infers Func<rune, UntypedInt> and the apply call rejects it (CS1503).
	maxFn := func(rune) rune { return maxRune }
	fmt.Println(apply(maxFn, 'a'))

	// Mixed arms: untyped-const refs + the typed parameter — no unique best common type (CS8917).
	encode := func(r rune) rune {
		if r == runeSelf {
			return maxRune
		}
		if r == maxRune {
			return runeSelf
		}
		return r
	}
	fmt.Println(apply(encode, runeSelf))
	fmt.Println(apply(encode, maxRune))
	fmt.Println(apply(encode, 'x'))

	// Mixed const/literal arms over a WIDER declared result (int64).
	pick := func(neg bool) int64 {
		if neg {
			return -1
		}
		return bigConst
	}
	fmt.Println(pick(false), pick(true))

	// Literal-only arms keep inferred typing (concrete C# types already; no prefix, no churn).
	shrink := func(r rune) rune { return 'a' }
	fmt.Println(apply(shrink, maxRune))

	// Argument position is target-typed by the delegate parameter — no inference to fail, no prefix.
	fmt.Println(apply(func(rune) rune { return maxRune }, 'b'))
}
