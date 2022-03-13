// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:42:06 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\unsafe.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class math_package {

// Float32bits returns the IEEE 754 binary representation of f,
// with the sign bit of f and the result in the same bit position.
// Float32bits(Float32frombits(x)) == x.
public static uint Float32bits(float f) {
    return new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_f));
}

// Float32frombits returns the floating-point number corresponding
// to the IEEE 754 binary representation b, with the sign bit of b
// and the result in the same bit position.
// Float32frombits(Float32bits(x)) == x.
public static float Float32frombits(uint b) {
    return new ptr<ptr<ptr<float>>>(@unsafe.Pointer(_addr_b));
}

// Float64bits returns the IEEE 754 binary representation of f,
// with the sign bit of f and the result in the same bit position,
// and Float64bits(Float64frombits(x)) == x.
public static ulong Float64bits(double f) {
    return new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_f));
}

// Float64frombits returns the floating-point number corresponding
// to the IEEE 754 binary representation b, with the sign bit of b
// and the result in the same bit position.
// Float64frombits(Float64bits(x)) == x.
public static double Float64frombits(ulong b) {
    return new ptr<ptr<ptr<double>>>(@unsafe.Pointer(_addr_b));
}

} // end math_package
