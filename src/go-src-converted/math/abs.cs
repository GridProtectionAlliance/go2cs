// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 04:45:16 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\abs.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Abs returns the absolute value of x.
        //
        // Special cases are:
        //    Abs(±Inf) = +Inf
        //    Abs(NaN) = NaN
        public static double Abs(double x)
        {
            return Float64frombits(Float64bits(x) & ~(1L << (int)(63L)));
        }
    }
}
