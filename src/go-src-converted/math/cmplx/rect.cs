// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2022 March 13 05:42:07 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\rect.go
namespace go.math;

using math = math_package;

public static partial class cmplx_package {

// Rect returns the complex number x with polar coordinates r, θ.
public static System.Numerics.Complex128 Rect(double r, double θ) {
    var (s, c) = math.Sincos(θ);
    return complex(r * c, r * s);
}

} // end cmplx_package
