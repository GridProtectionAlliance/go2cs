// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\dim.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Dim returns the maximum of x-y or 0.
        //
        // Special cases are:
        //    Dim(+Inf, +Inf) = NaN
        //    Dim(-Inf, -Inf) = NaN
        //    Dim(x, NaN) = Dim(NaN, x) = NaN
        public static double Dim(double x, double y)
        { 
            // The special cases result in NaN after the subtraction:
            //      +Inf - +Inf = NaN
            //      -Inf - -Inf = NaN
            //       NaN - y    = NaN
            //         x - NaN  = NaN
            var v = x - y;
            if (v <= 0L)
            { 
                // v is negative or 0
                return 0L;

            }
            return v;

        }

        // Max returns the larger of x or y.
        //
        // Special cases are:
        //    Max(x, +Inf) = Max(+Inf, x) = +Inf
        //    Max(x, NaN) = Max(NaN, x) = NaN
        //    Max(+0, ±0) = Max(±0, +0) = +0
        //    Max(-0, -0) = -0
        public static double Max(double x, double y)
;

        private static double max(double x, double y)
        { 
            // special cases

            if (IsInf(x, 1L) || IsInf(y, 1L)) 
                return Inf(1L);
            else if (IsNaN(x) || IsNaN(y)) 
                return NaN();
            else if (x == 0L && x == y) 
                if (Signbit(x))
                {>>MARKER:FUNCTION_Max_BLOCK_PREFIX<<
                    return y;
                }

                return x;
                        if (x > y)
            {
                return x;
            }

            return y;

        }

        // Min returns the smaller of x or y.
        //
        // Special cases are:
        //    Min(x, -Inf) = Min(-Inf, x) = -Inf
        //    Min(x, NaN) = Min(NaN, x) = NaN
        //    Min(-0, ±0) = Min(±0, -0) = -0
        public static double Min(double x, double y)
;

        private static double min(double x, double y)
        { 
            // special cases

            if (IsInf(x, -1L) || IsInf(y, -1L)) 
                return Inf(-1L);
            else if (IsNaN(x) || IsNaN(y)) 
                return NaN();
            else if (x == 0L && x == y) 
                if (Signbit(x))
                {>>MARKER:FUNCTION_Min_BLOCK_PREFIX<<
                    return x;
                }

                return y;
                        if (x < y)
            {
                return x;
            }

            return y;

        }
    }
}
