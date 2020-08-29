// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:49 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\hypot.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Hypot -- sqrt(p*p + q*q), but overflows only if the result does.
        */

        // Hypot returns Sqrt(p*p + q*q), taking care to avoid
        // unnecessary overflow and underflow.
        //
        // Special cases are:
        //    Hypot(±Inf, q) = +Inf
        //    Hypot(p, ±Inf) = +Inf
        //    Hypot(NaN, q) = NaN
        //    Hypot(p, NaN) = NaN
        public static double Hypot(double p, double q)
;

        private static double hypot(double p, double q)
        { 
            // special cases

            if (IsInf(p, 0L) || IsInf(q, 0L)) 
                return Inf(1L);
            else if (IsNaN(p) || IsNaN(q)) 
                return NaN();
                        if (p < 0L)
            {>>MARKER:FUNCTION_Hypot_BLOCK_PREFIX<<
                p = -p;
            }
            if (q < 0L)
            {
                q = -q;
            }
            if (p < q)
            {
                p = q;
                q = p;
            }
            if (p == 0L)
            {
                return 0L;
            }
            q = q / p;
            return p * Sqrt(1L + q * q);
        }
    }
}
