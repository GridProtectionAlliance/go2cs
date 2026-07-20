// Guards the fixed-size array COMPOSITE LITERAL length: a `[N]T{…}` literal is N long no matter
// how many elements it writes, and Go zero-fills the rest. The converter used to project the
// literal straight from its written elements (`new byte[]{}.array()`), so `[8]byte{}` became a
// length-0 array and the first index panicked at run time — it COMPILED, and gave a wrong answer.
// A slice literal is genuinely as long as its elements, and is the control here.
package main

import "fmt"

type named [6]byte
type aliased = [5]byte

// Package-level declarations take the same path as locals.
var pkgEmpty = [8]byte{}
var pkgPartial = [8]byte{1, 2}

func main() {
	// Positional, unnamed: the headline case.
	empty := [8]byte{}
	fmt.Println("empty", len(empty), empty[0], empty[7])

	partial := [8]byte{1, 2}
	fmt.Println("partial", len(partial), partial[0], partial[1], partial[2], partial[7])

	// A FULL literal already had the right length; it must keep working.
	full := [3]byte{1, 2, 3}
	fmt.Println("full", len(full), full[0], full[2])

	// An ellipsis literal's length IS its element count.
	ellipsis := [...]byte{4, 5, 6}
	fmt.Println("ellipsis", len(ellipsis), ellipsis[2])

	// Indexed/keyed elements. The zero-index form is its own case: index 0 used to read as
	// "no constant key" and fell to a projection sized by the literal's extent, not by N.
	keyed := [8]byte{5: 1}
	fmt.Println("keyed", len(keyed), keyed[5], keyed[0], keyed[7])

	keyedZero := [8]byte{0: 9}
	fmt.Println("keyedZero", len(keyedZero), keyedZero[0], keyedZero[7])

	// Named and aliased array types wrap the same projection.
	fmt.Println("named empty", len(named{}))
	np := named{1, 2}
	fmt.Println("named partial", len(np), np[0], np[5])

	fmt.Println("alias empty", len(aliased{}))
	ap := aliased{9}
	fmt.Println("alias partial", len(ap), ap[0], ap[4])

	// Package-level.
	fmt.Println("pkg", len(pkgEmpty), len(pkgPartial), pkgPartial[1], pkgPartial[7])

	// Non-byte element types take the same path.
	ints := [4]int{7}
	fmt.Println("ints", len(ints), ints[0], ints[3])

	strs := [3]string{"a"}
	fmt.Println("strs", len(strs), strs[0], strs[2] == "")

	// CONTROL: a slice literal is exactly as long as its elements — it must NOT be padded.
	fmt.Println("slice ctl", len([]byte{}), len([]byte{1, 2}))

	// Writing into the zero-filled tail proves the backing is really N long.
	w := [8]byte{1}
	w[7] = 42
	fmt.Println("write", len(w), w[0], w[6], w[7])
}
