// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 08 03:25:09 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\acosh.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The original C code, the long comment, and the constants
        // below are from FreeBSD's /usr/src/lib/msun/src/e_acosh.c
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
        // __ieee754_acosh(x)
        // Method :
        //    Based on
        //            acosh(x) = log [ x + sqrt(x*x-1) ]
        //    we have
        //            acosh(x) := log(x)+ln2,    if x is large; else
        //            acosh(x) := log(2x-1/(sqrt(x*x-1)+x)) if x>2; else
        //            acosh(x) := log1p(t+sqrt(2.0*t+t*t)); where t=x-1.
        //
        // Special cases:
        //    acosh(x) is NaN with signal if x<1.
        //    acosh(NaN) is NaN without signal.
        //

        // Acosh returns the inverse hyperbolic cosine of x.
        //
        // Special cases are:
        //    Acosh(+Inf) = +Inf
        //    Acosh(x) = NaN if x < 1
        //    Acosh(NaN) = NaN
        public static double Acosh(double x)
;

        private static double acosh(double x)
        {
            const float Ln2 = (float)6.93147180559945286227e-01F; // 0x3FE62E42FEFA39EF
            const long Large = (long)1L << (int)(28L); // 2**28 
            // first case is special case

            if (x < 1L || IsNaN(x)) 
                return NaN();
            else if (x == 1L) 
                return 0L;
            else if (x >= Large) 
                return Log(x) + Ln2; // x > 2**28
            else if (x > 2L) 
                return Log(2L * x - 1L / (x + Sqrt(x * x - 1L))); // 2**28 > x > 2
                        var t = x - 1L;
            return Log1p(t + Sqrt(2L * t + t * t)); // 2 >= x > 1
        }
    }
}
