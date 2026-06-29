package main

import "fmt"

// A computed untyped-CONSTANT mask (`(1 << shift) - 1`, `^(blockSize - 1)`) in a bitwise op whose
// result is a NATIVE-int type (uintptr/uint/int → C# nuint/nint/uintptr) is emitted as a bare C#
// `int` expression — the constant involves a native const (`shift`, emitted as a static readonly, so
// the whole expression is not a C# compile-time constant). So `i & ((1<<shift)-1)` is `nuint & int`
// → CS0019. The converter casts such a computed-constant operand to the native result type:
// `(uintptr)i & (uintptr)((1<<shift)-1)`. Mirrors runtime's `arenaIndex`/`alignDown` masks.

const shift uintptr = 3      // a native-int const (uintptr) -> C# static readonly, non-const
const blockSize uintptr = 16 // ditto

func low(i uintptr) uintptr   { return i & ((1 << shift) - 1) } // mask the low `shift` bits
func align(i uintptr) uintptr { return i &^ (blockSize - 1) }   // round down to a blockSize boundary

func main() {
	fmt.Println(uint64(low(0b1011)))   // 0b1011 & 0b111 = 0b011 = 3
	fmt.Println(uint64(align(0b1011))) // 11 &^ 15 = 0
	fmt.Println(uint64(align(0b11011))) // 27 &^ 15 = 16
}
