package main

import "fmt"

// Go zeroes a shift whose count reaches the operand width (a signed right shift sign-extends); C#'s
// native shift MASKS the count. Runtime counts come from a slice so nothing folds to a constant, and
// several are >= width. The converter must route these through golib's Go-semantics Rsh/Lsh guard,
// while a syntactically MASKED / MODULO'd count (provably < width) must stay the native shift.
func main() {
	c := []uint{0, 1, 63, 64, 65, 200}
	var u uint64 = 0x8000000000000001

	// Unsigned 64-bit: count >= 64 -> 0 (both directions).
	for _, k := range c {
		fmt.Println(u>>k, u<<k)
	}

	// Signed right shift sign-extends when count >= width.
	var neg int64 = -8
	var pos int64 = 1 << 40
	fmt.Println(neg>>c[3], neg>>c[5], pos>>c[3]) // -8>>64=-1, -8>>200=-1, (1<<40)>>64=0

	// 32-bit width threshold.
	var u32 uint32 = 0xDEADBEEF
	for _, k := range []uint{31, 32, 40} {
		fmt.Println(u32 >> k)
	}

	// R2 mask / R3 modulo: provably < width, must stay NATIVE and correct.
	fmt.Println(u >> (c[3] & 63)) // 64 & 63 = 0 -> u
	fmt.Println(u >> (c[4] % 64)) // 65 % 64 = 1 -> u >> 1
}
