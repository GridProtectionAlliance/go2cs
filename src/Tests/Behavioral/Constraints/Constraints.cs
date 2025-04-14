// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package constraints defines a set of useful constraints to be used
// with type parameters.

using go.runtime;

namespace go;

partial class constraints_package {

[GoType] partial struct Frog {
    public @string Name;
    public @string Color;
}

[GoType] partial interface ConstraintTest1<ΔT> {
    //  Type constraints: string | []int | map[string]int | chan string | *int | [2]int | Frog
    // Derived operators: none
    @string Upper();
}

[GoType("operators = Comparable")]
partial interface ConstraintTest2<ΔT> {
    //  Type constraints: string | chan string | *int | [2]int | Frog
    // Derived operators: ==, !=
    @string Lower();
}

// Signed is a constraint that permits any signed integer type.
// If future releases of Go add new predeclared signed integer types,
// this constraint will be modified to include them.
[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Signed<ΔT> {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

// Unsigned is a constraint that permits any unsigned integer type.
// If future releases of Go add new predeclared unsigned integer types,
// this constraint will be modified to include them.
[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Unsigned<ΔT> {
    //  Type constraints: ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

// Integer is a constraint that permits any integer type.
// If future releases of Go add new predeclared integer types,
// this constraint will be modified to include them.
[GoType("runtime; operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Integer<ΔT> {
    //  Type constraints: Signed | Unsigned
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}


[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface PromotedTest1<ΔT> {
    //  Type constraints: Signed
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType] partial interface PromotedTest2<ΔT> :
    ConstraintTest1<ΔT>
{
    //  Type constraints: ConstraintTest1
    // Derived operators: none
}

[GoType("operators = Comparable")]
partial interface PromotedTest3<ΔT> :
    ConstraintTest2<ΔT>
{
    //  Type constraints: ConstraintTest2
    // Derived operators: ==, !=
}

// Float is a constraint that permits any floating-point type.
// If future releases of Go add new predeclared floating-point types,
// this constraint will be modified to include them.
[GoType("operators = Sum, Arithmetic, Comparable, Ordered")]
partial interface Float<ΔT> {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

// Complex is a constraint that permits any complex numeric type.
// If future releases of Go add new predeclared complex numeric types,
// this constraint will be modified to include them.
[GoType("operators = Sum, Arithmetic, Comparable")]
partial interface Complex<ΔT> {
    //  Type constraints: ~complex64 | ~complex128
    // Derived operators: +, -, *, /, ==, !=
}

// Ordered is a constraint that permits any ordered type: any type
// that supports the operators < <= >= >.
// If future releases of Go add new ordered types,
// this constraint will be modified to include them.
//
// This type is redundant since Go 1.21 introduced [cmp.Ordered].
[GoType("operators = Sum, Comparable, Ordered")]
partial interface Ordered<ΔT> {
    //  Type constraints: Integer | Float | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

} // end constraints_package
