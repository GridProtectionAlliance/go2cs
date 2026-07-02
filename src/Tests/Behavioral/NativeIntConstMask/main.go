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

// A bitwise AND of a native-int (uintptr) with a LARGE literal mask whose value exceeds C# int32:
// convBasicLit emits the literal with a cast (it no longer fits a bare `int`), so it must also be
// cast to the native type or `nuint & nint` is CS0019. Mirrors runtime's `uintptrMask & 0x00ffffffffff`.
func maskAddr(i uintptr) uintptr { return i & 0x00ffffffffff } // mirrors runtime's uintptrMask

// `&^` with a small LITERAL operand: it is rendered `& ~15`, and `~15` promotes to a negative C#
// `int` that cannot convert to an unsigned native type — `nuint & ~15` is CS0019. The complemented
// constant is cast to the native type (`& ~(uintptr)15`). Mirrors runtime's `ptr &^ 15` 16-byte align.
func alignSmall(i uintptr) uintptr { return i &^ 15 }

// A COMPUTED untyped-constant operand in native arithmetic — runtime stkframe.go's
// `arg0 + 4*goarch.PtrSize`: the product of a literal and a named UNTYPED const renders as an
// UntypedInt-typed C# expression, so the whole sum types as UntypedInt and breaks a following
// conversion or `var` inference. The computed operand is cast to the concrete operand's type —
// `base + (uintptr)(4 * ptrWords)`. Pure-literal arithmetic (`x + 2*3`) and Go-conversion
// operands (`float64(…)/…`) need no cast and are left alone.
const ptrWords = 8 // a named UNTYPED int const (no type annotation)

func fieldAddr(base uintptr) uintptr { return base + 4*ptrWords }

func main() {
	fmt.Println(uint64(low(0b1011)))   // 0b1011 & 0b111 = 0b011 = 3
	fmt.Println(uint64(align(0b1011))) // 11 &^ 15 = 0
	fmt.Println(uint64(align(0b11011))) // 27 &^ 15 = 16
	fmt.Println(uint64(fieldAddr(0x1000))) // 0x1000 + 32 = 4128
	// Build a value with a high bit set without using a large literal in a non-bitwise context
	// (those are separate cases); the large mask literal is exercised inside maskAddr's `&`.
	var addr uintptr = 1
	addr <<= 47
	addr |= 0xABCD
	fmt.Println(uint64(maskAddr(addr))) // high bit beyond 40 is masked off, low bits kept
	fmt.Println(uint64(alignSmall(27))) // 27 &^ 15 = 16
}
