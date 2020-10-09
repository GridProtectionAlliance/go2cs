// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\cbrt.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The go code is a modified version of the original C code from
        // http://www.netlib.org/fdlibm/s_cbrt.c and came with this notice.
        //
        // ====================================================
        // Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
        //
        // Developed at SunSoft, a Sun Microsystems, Inc. business.
        // Permission to use, copy, modify, and distribute this
        // software is freely granted, provided that this notice
        // is preserved.
        // ====================================================

        // Cbrt returns the cube root of x.
        //
        // Special cases are:
        //    Cbrt(±0) = ±0
        //    Cbrt(±Inf) = ±Inf
        //    Cbrt(NaN) = NaN
        public static double Cbrt(double x)
;

        private static double cbrt(double x)
        {
            const long B1 = (long)715094163L; // (682-0.03306235651)*2**20
            const long B2 = (long)696219795L; // (664-0.03306235651)*2**20
            const float C = (float)5.42857142857142815906e-01F; // 19/35     = 0x3FE15F15F15F15F1
            const float D = (float)-7.05306122448979611050e-01F; // -864/1225 = 0xBFE691DE2532C834
            const float E = (float)1.41428571428571436819e+00F; // 99/70     = 0x3FF6A0EA0EA0EA0F
            const float F = (float)1.60714285714285720630e+00F; // 45/28     = 0x3FF9B6DB6DB6DB6E
            const float G = (float)3.57142857142857150787e-01F; // 5/14      = 0x3FD6DB6DB6DB6DB7
            const float SmallestNormal = (float)2.22507385850720138309e-308F; // 2**-1022  = 0x0010000000000000 
            // special cases

            if (x == 0L || IsNaN(x) || IsInf(x, 0L)) 
                return x;
                        var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Cbrt_BLOCK_PREFIX<<
                x = -x;
                sign = true;
            } 

            // rough cbrt to 5 bits
            var t = Float64frombits(Float64bits(x) / 3L + B1 << (int)(32L));
            if (x < SmallestNormal)
            { 
                // subnormal number
                t = float64(1L << (int)(54L)); // set t= 2**54
                t *= x;
                t = Float64frombits(Float64bits(t) / 3L + B2 << (int)(32L));

            } 

            // new cbrt to 23 bits
            var r = t * t / x;
            var s = C + r * t;
            t *= G + F / (s + E + D / s); 

            // chop to 22 bits, make larger than cbrt(x)
            t = Float64frombits(Float64bits(t) & (0xFFFFFFFFCUL << (int)(28L)) + 1L << (int)(30L)); 

            // one step newton iteration to 53 bits with error less than 0.667ulps
            s = t * t; // t*t is exact
            r = x / s;
            var w = t + t;
            r = (r - t) / (w + r); // r-s is exact
            t = t + t * r; 

            // restore the sign bit
            if (sign)
            {
                t = -t;
            }

            return t;

        }
    }
}
