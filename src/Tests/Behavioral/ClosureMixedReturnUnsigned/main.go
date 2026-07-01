package main

import "fmt"

// casePC mirrors runtime/select.go's `casePC := func(casi int) uintptr { if pcs == nil { return 0 };
// return pcs[casi] }`. The literal `return 0` is typed `int`; `return pcs[casi]` is typed `uintptr`.
// C# infers a lambda's delegate type from its return-expression *types*, and `int` has no
// best-common-type with `nuint` (uintptr) / `uint` / `ulong` — so `var casePC = …` fails to compile
// (CS8917) unless the converter casts the literal to the result type. This test exercises that shape
// across uintptr / uint64 / uint32 / uint, plus a signed control that must stay uncast.
//
//go:noinline
func run() {
	// uintptr — the exact runtime/select.go shape
	pcs := []uintptr{10, 20, 30}
	casePC := func(casi int) uintptr {
		if pcs == nil {
			return 0
		}
		return pcs[casi]
	}
	fmt.Println(casePC(0), casePC(2)) // 10 30

	// uint64
	vals := []uint64{100, 200}
	getU64 := func(i int) uint64 {
		if i < 0 {
			return 0
		}
		return vals[i]
	}
	fmt.Println(getU64(1), getU64(-1)) // 200 0

	// uint32
	tab := []uint32{7, 8, 9}
	getU32 := func(i int) uint32 {
		if i >= len(tab) {
			return 0
		}
		return tab[i]
	}
	fmt.Println(getU32(2), getU32(5)) // 9 0

	// plain uint
	sizes := []uint{4, 16, 64}
	getU := func(i int) uint {
		if i < 0 {
			return 0
		}
		return sizes[i]
	}
	fmt.Println(getU(0), getU(-1)) // 4 0

	// signed control: a lambda returning int with a literal must NOT be cast (int shares a common
	// type with the other return) — guards against over-firing / behavior change.
	pick := func(b bool) int {
		if b {
			return -1
		}
		return 42
	}
	fmt.Println(pick(true), pick(false)) // -1 42
}

func main() {
	run()
}
