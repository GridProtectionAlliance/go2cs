// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2022 March 13 05:42:07 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\isinf.go
namespace go.math;

using math = math_package;

public static partial class cmplx_package {

// IsInf reports whether either real(x) or imag(x) is an infinity.
public static bool IsInf(System.Numerics.Complex128 x) {
    if (math.IsInf(real(x), 0) || math.IsInf(imag(x), 0)) {
        return true;
    }
    return false;
}

// Inf returns a complex infinity, complex(+Inf, +Inf).
public static System.Numerics.Complex128 Inf() {
    var inf = math.Inf(1);
    return complex(inf, inf);
}

} // end cmplx_package
