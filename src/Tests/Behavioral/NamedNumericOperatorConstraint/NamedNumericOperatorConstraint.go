// Guards a named-numeric type used as the type argument of a generic constrained by a
// `~integer` type set whose body applies modulus / bitwise / shift operators — the shape of
// internal/trace's `dataTable[EI ~uint64, E]` instantiated with `type stringID uint64`. The
// converter lifts the `~uint64` constraint to IModulus/IBitwise/IShiftOperators; the
// [GoType num:] wrapper for the named type must DECLARE those interfaces (its operators already
// existed) or the instantiation fails CS0315. Kept minimal (a generic func, no maps/generic
// methods) so it isolates the operator-constraint satisfaction.
package main

import "fmt"

type stringID uint64 // named unsigned
type offset int32    // named signed (exercises the signed shift/complement path)

// mix applies every operator the `~integer` constraint lifts: modulus, and (all in
// GetComplementOperator) complement, both shifts, and bitwise and/or/xor — on the type param.
func mix[K ~uint64 | ~int32](id K, step K) K {
	h := id
	h = h ^ (h >> 2)   // shift + xor
	h = h | (step << 1) // shift + or
	h = h & step        // bitwise-and
	h = h % (step + 7)  // modulus
	return h
}

func main() {
	var a stringID = 12345
	var b stringID = 11
	fmt.Println(mix(a, b)) // unsigned named uint64 type argument

	var x offset = -8
	var y offset = 5
	fmt.Println(mix(x, y)) // signed named int32 type argument

	// bare instantiations to force the constraint check on both named types
	fmt.Println(mix(stringID(255), stringID(3)), mix(offset(100), offset(9)))
}
