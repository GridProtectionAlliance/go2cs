// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\asinh.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The original C code, the long comment, and the constants
        // below are from FreeBSD's /usr/src/lib/msun/src/s_asinh.c
        // and came with this notice. The go code is a simplified
        // version of the original C.
        //
        // ====================================================
        // Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
        //
        // Developed at SunPro, a Sun Microsystems, Inc. business.
        // Permission to use, copy, modify, and distribute this
        // software is freely granted, provided that this notice
        // is preserved.
        // ====================================================
        //
        //
        // asinh(x)
        // Method :
        //    Based on
        //            asinh(x) = sign(x) * log [ |x| + sqrt(x*x+1) ]
        //    we have
        //    asinh(x) := x  if  1+x*x=1,
        //             := sign(x)*(log(x)+ln2)) for large |x|, else
        //             := sign(x)*log(2|x|+1/(|x|+sqrt(x*x+1))) if|x|>2, else
        //             := sign(x)*log1p(|x| + x**2/(1 + sqrt(1+x**2)))
        //

        // Asinh returns the inverse hyperbolic sine of x.
        //
        // Special cases are:
        //    Asinh(±0) = ±0
        //    Asinh(±Inf) = ±Inf
        //    Asinh(NaN) = NaN
        public static double Asinh(double x)
;

        private static double asinh(double x)
        {
            const float Ln2 = (float)6.93147180559945286227e-01F; // 0x3FE62E42FEFA39EF
            const float NearZero = (float)1.0F / (1L << (int)(28L)); // 2**-28
            const long Large = (long)1L << (int)(28L); // 2**28 
            // special cases
            if (IsNaN(x) || IsInf(x, 0L))
            {>>MARKER:FUNCTION_Asinh_BLOCK_PREFIX<<
                return x;
            }

            var sign = false;
            if (x < 0L)
            {
                x = -x;
                sign = true;
            }

            double temp = default;

            if (x > Large) 
                temp = Log(x) + Ln2; // |x| > 2**28
            else if (x > 2L) 
                temp = Log(2L * x + 1L / (Sqrt(x * x + 1L) + x)); // 2**28 > |x| > 2.0
            else if (x < NearZero) 
                temp = x; // |x| < 2**-28
            else 
                temp = Log1p(x + x * x / (1L + Sqrt(1L + x * x))); // 2.0 > |x| > 2**-28
                        if (sign)
            {
                temp = -temp;
            }

            return temp;

        }
    }
}
