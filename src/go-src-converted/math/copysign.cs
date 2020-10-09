// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\copysign.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Copysign returns a value with the magnitude
        // of x and the sign of y.
        public static double Copysign(double x, double y)
        {
            const long sign = (long)1L << (int)(63L);

            return Float64frombits(Float64bits(x) & ~sign | Float64bits(y) & sign);
        }
    }
}
