// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 08 03:25:19 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\ldexp.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Ldexp is the inverse of Frexp.
        // It returns frac × 2**exp.
        //
        // Special cases are:
        //    Ldexp(±0, exp) = ±0
        //    Ldexp(±Inf, exp) = ±Inf
        //    Ldexp(NaN, exp) = NaN
        public static double Ldexp(double frac, long exp)
;

        private static double ldexp(double frac, long exp)
        { 
            // special cases

            if (frac == 0L) 
                return frac; // correctly return -0
            else if (IsInf(frac, 0L) || IsNaN(frac)) 
                return frac;
                        var (frac, e) = normalize(frac);
            exp += e;
            var x = Float64bits(frac);
            exp += int(x >> (int)(shift)) & mask - bias;
            if (exp < -1075L)
            {>>MARKER:FUNCTION_Ldexp_BLOCK_PREFIX<<
                return Copysign(0L, frac); // underflow
            }

            if (exp > 1023L)
            { // overflow
                if (frac < 0L)
                {
                    return Inf(-1L);
                }

                return Inf(1L);

            }

            double m = 1L;
            if (exp < -1022L)
            { // denormal
                exp += 53L;
                m = 1.0F / (1L << (int)(53L)); // 2**-53
            }

            x &= mask << (int)(shift);
            x |= uint64(exp + bias) << (int)(shift);
            return m * Float64frombits(x);

        }
    }
}
