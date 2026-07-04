// Regression test: a named numeric type combined with an untyped constant in a bitwise op. The
// untyped-const operand must be cast to the named type's UNDERLYING basic type, not the named type
// itself: `t | (uint8)flag`, not `t | (Tag)flag`. An UntypedInt wrapper -> a named type needs two
// user-defined conversions (UntypedInt->uint8->Tag), which C# rejects (CS0030); via the underlying
// it is one explicit + one implicit conversion. This is the x/crypto/cryptobyte/asn1 Tag pattern.
package main

import "fmt"

type Tag uint8

const classConstructed = 0x20 // untyped const
const classContext = 0x80

func (t Tag) Constructed() Tag { return t | classConstructed }
func (t Tag) Context() Tag     { return t | classContext }

// A COMPUTED (arithmetic-shaped) constant mask under a NAMED-numeric bitwise result — runtime
// tagptr_64bit.go's `tp & (1<<taggedPointerBits - 1)` (taggedPointer over uint64): the mask
// renders as an UntypedInt-typed C# expression and `word & int` has no operator (CS0019). The
// mask is cast to the result's UNDERLYING basic — `w & (uint64)((1 << tagBits) - 1)` — after
// which the named operand's implicit underlying conversion binds. Bitwise-shaped constant
// operands are recursively emitted with their own concrete wrap and are excluded (no double
// cast); pure-literal masks convert as C# constants and are also left alone.
type word uint64

const tagBits = 19 // named UNTYPED const (no type annotation), like taggedPointerBits

func (w word) low() word { return w & (1<<tagBits - 1) }

func main() {
	var t Tag = 0x10
	fmt.Println(uint8(t.Constructed())) // 0x30 = 48
	fmt.Println(uint8(t.Context()))     // 0x90 = 144

	var w word = 0xABCDEF012345
	fmt.Println(uint64(w.low())) // low 19 bits: 0x12345 = 74565

	// ^Tag(0) - the all-ones idiom passed to a named-typed parameter (os exec_windows
	// ^syscall.Handle(0), CS1503 x2): the folded constant must re-impose the named type.
	fmt.Println(uint8(mask(^Tag(0)))) // 255

	// The math/big Word-arithmetic shape: a NAMED conversion of a CONSTANT inside a
	// binary expression (`Word(1)<<s - 1`) must keep its named cast - the identity-arm
	// fold rendered the bare literal and the whole expression degraded to int (CS0029
	// x3) - and the generated wrapper's shift/bitwise operators keep the wrapper type
	// (CS0266 x45 without them).
	fmt.Println(uint64(maskFor(4)), uint64(maskFor(19))) // 15 524287

	var wv word = 0xF0
	fmt.Println(uint64(wv&0x30|word(0x01)), uint64(wv^word(0xFF))) // 49 15
}

func mask(t Tag) Tag { return t & 0xFF }

func maskFor(s uint) word {
	m := word(1)<<s - 1
	return m
}

