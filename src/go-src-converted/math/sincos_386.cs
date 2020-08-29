// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:58 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\sincos_386.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Sincos returns Sin(x), Cos(x).
        //
        // Special cases are:
        //    Sincos(±0) = ±0, 1
        //    Sincos(±Inf) = NaN, NaN
        //    Sincos(NaN) = NaN, NaN
        public static (double, double) Sincos(double x)
;
    }
}
