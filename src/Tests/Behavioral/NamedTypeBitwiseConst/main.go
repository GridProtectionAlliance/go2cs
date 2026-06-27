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

func main() {
	var t Tag = 0x10
	fmt.Println(uint8(t.Constructed())) // 0x30 = 48
	fmt.Println(uint8(t.Context()))     // 0x90 = 144
}
