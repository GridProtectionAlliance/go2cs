package main

import "fmt"

// Go evaluates a constant expression in ARBITRARY PRECISION and requires only the FINAL value to
// be representable in the target type -- a SUBexpression is free to overflow it. `[]int32{1<<31 - 1}`
// is legal Go: the inner shift is 2147483648 (past int32), the difference 2147483647 is not.
//
// The converter folds such an out-of-int32-range subexpression to a C# `long` literal (C# would
// otherwise compute the operators in int32 and overflow at compile time), which widens the WHOLE
// element rendering to `long`. `long` converts implicitly to none of the narrower integer targets,
// so the emission must narrow back to the target type exactly once -- otherwise the element fails
// to compile (CS0266 at a composite element or assignment, CS1503 at an argument). This test pins
// both the values and the shapes that must NOT change: a subexpression that stays inside int32
// keeps its readable operator form, and a whole value already past int32 folds outright.
func main() {
	// int32 elements: two controls whose subexpressions stay in range, then the overflowing ones.
	i32 := []int32{0, 1, 1 << 20, 1<<20 + 1, 1<<31 - 2, 1<<31 - 1}

	// int64 elements: `long` IS the widened width, so these need no narrowing at all.
	i64 := []int64{1 << 60, 1<<62 + 1, 1<<63 - 1}

	// Narrower / unsigned / native-width targets all take the same narrowing.
	i16 := []int16{1<<15 - 1}
	u32 := []uint32{1<<32 - 1}
	u64 := []uint64{1<<40 + 1, 1<<64 - 1}
	ptr := []uintptr{1<<40 + 1}
	ints := []int{1<<31 - 1}

	// The same shape away from a composite literal: a typed assignment, an explicit conversion
	// (which must not double the cast), and a function argument.
	var n32 int32 = 1<<31 - 1
	c32 := int32(1<<31 - 1)
	var nptr uintptr = 1<<40 + 1

	for _, v := range i32 {
		fmt.Println(v)
	}

	for _, v := range i64 {
		fmt.Println(v)
	}

	for _, v := range i16 {
		fmt.Println(v)
	}

	for _, v := range u32 {
		fmt.Println(v)
	}

	for _, v := range u64 {
		fmt.Println(v)
	}

	for _, v := range ptr {
		fmt.Println(v)
	}

	for _, v := range ints {
		fmt.Println(v)
	}

	fmt.Println(n32, c32, nptr)
	showInt32(1<<31 - 2)
	showUint32(1<<32 - 1)
}

func showInt32(v int32) {
	fmt.Println(v)
}

func showUint32(v uint32) {
	fmt.Println(v)
}
