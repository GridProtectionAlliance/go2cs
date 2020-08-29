// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:47 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\exp.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Exp returns e**x, the base-e exponential of x.
        //
        // Special cases are:
        //    Exp(+Inf) = +Inf
        //    Exp(NaN) = NaN
        // Very large values overflow to 0 or +Inf.
        // Very small values underflow to 1.
        public static double Exp(double x)
;

        // The original C code, the long comment, and the constants
        // below are from FreeBSD's /usr/src/lib/msun/src/e_exp.c
        // and came with this notice. The go code is a simplified
        // version of the original C.
        //
        // ====================================================
        // Copyright (C) 2004 by Sun Microsystems, Inc. All rights reserved.
        //
        // Permission to use, copy, modify, and distribute this
        // software is freely granted, provided that this notice
        // is preserved.
        // ====================================================
        //
        //
        // exp(x)
        // Returns the exponential of x.
        //
        // Method
        //   1. Argument reduction:
        //      Reduce x to an r so that |r| <= 0.5*ln2 ~ 0.34658.
        //      Given x, find r and integer k such that
        //
        //               x = k*ln2 + r,  |r| <= 0.5*ln2.
        //
        //      Here r will be represented as r = hi-lo for better
        //      accuracy.
        //
        //   2. Approximation of exp(r) by a special rational function on
        //      the interval [0,0.34658]:
        //      Write
        //          R(r**2) = r*(exp(r)+1)/(exp(r)-1) = 2 + r*r/6 - r**4/360 + ...
        //      We use a special Remez algorithm on [0,0.34658] to generate
        //      a polynomial of degree 5 to approximate R. The maximum error
        //      of this polynomial approximation is bounded by 2**-59. In
        //      other words,
        //          R(z) ~ 2.0 + P1*z + P2*z**2 + P3*z**3 + P4*z**4 + P5*z**5
        //      (where z=r*r, and the values of P1 to P5 are listed below)
        //      and
        //          |                  5          |     -59
        //          | 2.0+P1*z+...+P5*z   -  R(z) | <= 2
        //          |                             |
        //      The computation of exp(r) thus becomes
        //                             2*r
        //              exp(r) = 1 + -------
        //                            R - r
        //                                 r*R1(r)
        //                     = 1 + r + ----------- (for better accuracy)
        //                                2 - R1(r)
        //      where
        //                               2       4             10
        //              R1(r) = r - (P1*r  + P2*r  + ... + P5*r   ).
        //
        //   3. Scale back to obtain exp(x):
        //      From step 1, we have
        //         exp(x) = 2**k * exp(r)
        //
        // Special cases:
        //      exp(INF) is INF, exp(NaN) is NaN;
        //      exp(-INF) is 0, and
        //      for finite argument, only exp(0)=1 is exact.
        //
        // Accuracy:
        //      according to an error analysis, the error is always less than
        //      1 ulp (unit in the last place).
        //
        // Misc. info.
        //      For IEEE double
        //          if x >  7.09782712893383973096e+02 then exp(x) overflow
        //          if x < -7.45133219101941108420e+02 then exp(x) underflow
        //
        // Constants:
        // The hexadecimal values are the intended ones for the following
        // constants. The decimal values may be used, provided that the
        // compiler will convert from decimal to binary accurately enough
        // to produce the hexadecimal values shown.

        private static double exp(double x)
        {
            const float Ln2Hi = 6.93147180369123816490e-01F;
            const float Ln2Lo = 1.90821492927058770002e-10F;
            const float Log2e = 1.44269504088896338700e+00F;

            const float Overflow = 7.09782712893383973096e+02F;
            const float Underflow = -7.45133219101941108420e+02F;
            const float NearZero = 1.0F / (1L << (int)(28L)); // 2**-28 

            // special cases

            if (IsNaN(x) || IsInf(x, 1L)) 
                return x;
            else if (IsInf(x, -1L)) 
                return 0L;
            else if (x > Overflow) 
                return Inf(1L);
            else if (x < Underflow) 
                return 0L;
            else if (-NearZero < x && x < NearZero) 
                return 1L + x;
            // reduce; computed as r = hi - lo for extra precision.
            long k = default;

            if (x < 0L) 
                k = int(Log2e * x - 0.5F);
            else if (x > 0L) 
                k = int(Log2e * x + 0.5F);
                        var hi = x - float64(k) * Ln2Hi;
            var lo = float64(k) * Ln2Lo; 

            // compute
            return expmulti(hi, lo, k);
        }

        // Exp2 returns 2**x, the base-2 exponential of x.
        //
        // Special cases are the same as Exp.
        public static double Exp2(double x)
;

        private static double exp2(double x)
        {
            const float Ln2Hi = 6.93147180369123816490e-01F;
            const float Ln2Lo = 1.90821492927058770002e-10F;

            const float Overflow = 1.0239999999999999e+03F;
            const float Underflow = -1.0740e+03F; 

            // special cases

            if (IsNaN(x) || IsInf(x, 1L)) 
                return x;
            else if (IsInf(x, -1L)) 
                return 0L;
            else if (x > Overflow) 
                return Inf(1L);
            else if (x < Underflow) 
                return 0L;
            // argument reduction; x = r×lg(e) + k with |r| ≤ ln(2)/2.
            // computed as r = hi - lo for extra precision.
            long k = default;

            if (x > 0L) 
                k = int(x + 0.5F);
            else if (x < 0L) 
                k = int(x - 0.5F);
                        var t = x - float64(k);
            var hi = t * Ln2Hi;
            var lo = -t * Ln2Lo; 

            // compute
            return expmulti(hi, lo, k);
        }

        // exp1 returns e**r × 2**k where r = hi - lo and |r| ≤ ln(2)/2.
        private static double expmulti(double hi, double lo, long k)
        {
            const float P1 = 1.66666666666666657415e-01F; /* 0x3FC55555; 0x55555555 */
            const float P2 = -2.77777777770155933842e-03F; /* 0xBF66C16C; 0x16BEBD93 */
            const float P3 = 6.61375632143793436117e-05F; /* 0x3F11566A; 0xAF25DE2C */
            const float P4 = -1.65339022054652515390e-06F; /* 0xBEBBBD41; 0xC5D26BF1 */
            const float P5 = 4.13813679705723846039e-08F; /* 0x3E663769; 0x72BEA4D0 */

            var r = hi - lo;
            var t = r * r;
            var c = r - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));
            long y = 1L - ((lo - (r * c) / (2L - c)) - hi); 
            // TODO(rsc): make sure Ldexp can handle boundary k
            return Ldexp(y, k);
        }
    }
}
