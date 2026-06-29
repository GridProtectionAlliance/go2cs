package main

import "fmt"

// A Go conversion to a NAMED numeric type — `traceArg(procs)`, `arenaIdx(x)` — where the argument is
// not already the named type's underlying basic. The [GoType] conversion operator only converts
// between the named type and its EXACT underlying (uint64 / uint), so a plain C# cast
// `(traceArg)int32Expr` is CS0030. The converter coerces through the underlying first:
// `((traceArg)(uint64)procs)`. When the argument IS already the underlying, no extra cast is added.

type traceArg uint64 // underlying uint64
type arenaIdx uint   // underlying uint (C# nuint)

func main() {
	var procs int32 = 5
	a := traceArg(procs) // int32 -> traceArg: ((traceArg)(uint64)procs)
	fmt.Println(uint64(a))

	var x int = 1 << 4
	b := arenaIdx(x) // int -> arenaIdx: ((arenaIdx)(nuint)x)
	fmt.Println(uint(b))

	// A named-to-named conversion (different underlying widths).
	var g traceGoStatus = 3
	c := traceArg(g) // traceGoStatus(uint8) -> traceArg(uint64)
	fmt.Println(uint64(c))

	// Argument already the underlying basic — no extra cast, still works.
	var u uint64 = 9
	d := traceArg(u)
	fmt.Println(uint64(d))
}

type traceGoStatus uint8
