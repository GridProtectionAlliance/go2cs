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

	// A subexpression past INT64 ENTIRELY under a NATIVE-WIDTH target. `1 << 63` is
	// 9223372036854775808, so no signed `long` fold can carry it, and `nuint` has no implicit
	// conversion from `ulong` -- the emission must widen past int64 AND narrow to the target in
	// one step. Without it C# computes the element in int32 (CS0029 `int` -> the target, and
	// CS0019 when a named-const operand has already forced the other side wide). math/big's
	// `nat{..., 1 + 1<<(_W-1), _M ^ (1 << (_W - 1))}` (int_test.go TestQuoStepD6) is exactly this
	// shape, where Word is a NAMED type over uintptr -- covered here alongside the plain targets.
	words := []Word{1 + 1<<(_W-1), _M ^ (1 << (_W - 1))}
	wide := []uintptr{1 + 1<<63, _M ^ (1 << (_W - 1))}
	uns := []uint{1 + 1<<63}

	// uint64 already folded this shape (bare `UL`, no cast); pinned so that emission stays put.
	u64wide := []uint64{1 + 1<<63}

	for _, v := range words {
		fmt.Println(v)
	}

	for _, v := range wide {
		fmt.Println(v)
	}

	for _, v := range uns {
		fmt.Println(v)
	}

	for _, v := range u64wide {
		fmt.Println(v)
	}
}

// Word mirrors math/big's Word -- a NAMED type whose underlying type is uintptr, so its elements
// take the native-width fold through a [GoType] wrapper rather than a primitive target.
type Word uintptr

const (
	_W = 64
	_B = 1 << _W
	_M = _B - 1
)

func showInt32(v int32) {
	fmt.Println(v)
}

func showUint32(v uint32) {
	fmt.Println(v)
}
