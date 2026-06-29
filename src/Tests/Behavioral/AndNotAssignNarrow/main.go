// AndNotAssignNarrow guards the `&^=` (AND-NOT / bit-clear) compound assignment on a
// narrow or unsigned integer. C# has no `&^`, so it expands to `flags &= ~X`; the `~`
// promotes its operand to `int`, which is not implicitly convertible to byte/ushort/
// uint/… (CS0266), and for a CONSTANT operand `~X` folds to a negative int constant
// whose checked narrowing overflows (CS0221). The converter emits
// `flags &= unchecked((uint8)~X)` for such LHS types; an `int` LHS (which `int` widens
// to) is left as `&= ~X`. Both an ident LHS and a struct-field (selector) LHS are
// exercised since they route through different emission paths. Mirrors runtime's
// `h.flags &^= hashWriting`.
package main

import "fmt"

const writing = 4

type hmap struct {
	flags uint8
}

func main() {
	h := &hmap{flags: 7}
	h.flags &^= writing  // selector LHS, uint8, named const
	h.flags |= 8         // 3 | 8 = 11
	fmt.Println(h.flags) // 11

	var u uint16 = 0xFFFF
	u &^= 0x0F0    // ident LHS, uint16, literal
	fmt.Println(u) // 65295

	var x uint32 = 0xFFFFFFFF
	x &^= 0xFF
	fmt.Println(x) // 4294967040

	var i int = 0xFF
	i &^= 0x0F      // int LHS: no cast needed
	fmt.Println(i)  // 240
}
