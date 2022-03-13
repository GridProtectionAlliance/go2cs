// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2022 March 13 05:42:07 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\phase.go
namespace go.math;

using math = math_package;

public static partial class cmplx_package {

// Phase returns the phase (also called the argument) of x.
// The returned value is in the range [-Pi, Pi].
public static double Phase(System.Numerics.Complex128 x) {
    return math.Atan2(imag(x), real(x));
}

} // end cmplx_package
