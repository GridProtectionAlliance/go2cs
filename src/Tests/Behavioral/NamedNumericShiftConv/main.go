package main

import "fmt"

type arenaIdx uint   // wide: underlying uint -> C# nuint (no int-promotion in shift)
type tag uint8       // narrow: underlying uint8 -> C# byte (promotes to int in shift)
type big uint64      // wide: underlying uint64

const bits = 6

// Converting an untyped-constant SHIFT to a NAMED numeric type — `arenaIdx(1 << bits)` —
// re-types the shift to its resolved (named) type. The [GoType] conversion only accepts the
// type's EXACT underlying basic, never C# `int`, so the shift must be re-typed through the
// underlying: `(arenaIdx)((nuint)1 << bits)`. A bare `(arenaIdx)(1 << bits)` is CS0030.
// Mirrors the runtime's `arenaIdx(1 << arenaBits)`.
func main() {
	var a arenaIdx = arenaIdx(1 << bits)
	var t tag = tag(1 << 3)
	var b big = big(1 << 40)
	fmt.Println(uint(a), uint8(t), uint64(b))
}
