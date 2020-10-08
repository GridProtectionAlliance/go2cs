// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 08 03:25:20 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\logb.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Logb returns the binary exponent of x.
        //
        // Special cases are:
        //    Logb(±Inf) = +Inf
        //    Logb(0) = -Inf
        //    Logb(NaN) = NaN
        public static double Logb(double x)
        { 
            // special cases

            if (x == 0L) 
                return Inf(-1L);
            else if (IsInf(x, 0L)) 
                return Inf(1L);
            else if (IsNaN(x)) 
                return x;
                        return float64(ilogb(x));

        }

        // Ilogb returns the binary exponent of x as an integer.
        //
        // Special cases are:
        //    Ilogb(±Inf) = MaxInt32
        //    Ilogb(0) = MinInt32
        //    Ilogb(NaN) = MaxInt32
        public static long Ilogb(double x)
        { 
            // special cases

            if (x == 0L) 
                return MinInt32;
            else if (IsNaN(x)) 
                return MaxInt32;
            else if (IsInf(x, 0L)) 
                return MaxInt32;
                        return ilogb(x);

        }

        // logb returns the binary exponent of x. It assumes x is finite and
        // non-zero.
        private static long ilogb(double x)
        {
            var (x, exp) = normalize(x);
            return int((Float64bits(x) >> (int)(shift)) & mask) - bias + exp;
        }
    }
}
