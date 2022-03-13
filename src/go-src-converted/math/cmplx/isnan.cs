// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2022 March 13 05:42:07 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\isnan.go
namespace go.math;

using math = math_package;

public static partial class cmplx_package {

// IsNaN reports whether either real(x) or imag(x) is NaN
// and neither is an infinity.
public static bool IsNaN(System.Numerics.Complex128 x) {

    if (math.IsInf(real(x), 0) || math.IsInf(imag(x), 0)) 
        return false;
    else if (math.IsNaN(real(x)) || math.IsNaN(imag(x))) 
        return true;
        return false;
}

// NaN returns a complex ``not-a-number'' value.
public static System.Numerics.Complex128 NaN() {
    var nan = math.NaN();
    return complex(nan, nan);
}

} // end cmplx_package
