// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cmplx provides basic constants and mathematical functions for
// complex numbers. Special case handling conforms to the C99 standard
// Annex G IEC 60559-compatible complex arithmetic.

// package cmplx -- go2cs converted at 2022 March 13 05:42:06 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\abs.go
namespace go.math;

using math = math_package;

public static partial class cmplx_package {

// Abs returns the absolute value (also called the modulus) of x.
public static double Abs(System.Numerics.Complex128 x) {
    return math.Hypot(real(x), imag(x));
}

} // end cmplx_package
