// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:08:01 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\abs.go


namespace go;

public static partial class math_package {

    // Abs returns the absolute value of x.
    //
    // Special cases are:
    //    Abs(±Inf) = +Inf
    //    Abs(NaN) = NaN
public static double Abs(double x) {
    return Float64frombits(Float64bits(x) & ~(1 << 63));
}

} // end math_package
