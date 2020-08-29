// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Copy of math/sqrt.go, here for use by ARM softfloat.
// Modified to not use any floating point arithmetic so
// that we don't clobber any floating-point registers
// while emulating the sqrt instruction.

// package runtime -- go2cs converted at 2020 August 29 08:20:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sqrt.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // The original C code and the long comment below are
        // from FreeBSD's /usr/src/lib/msun/src/e_sqrt.c and
        // came with this notice. The go code is a simplified
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
        // __ieee754_sqrt(x)
        // Return correctly rounded sqrt.
        //           -----------------------------------------
        //           | Use the hardware sqrt if you have one |
        //           -----------------------------------------
        // Method:
        //   Bit by bit method using integer arithmetic. (Slow, but portable)
        //   1. Normalization
        //      Scale x to y in [1,4) with even powers of 2:
        //      find an integer k such that  1 <= (y=x*2**(2k)) < 4, then
        //              sqrt(x) = 2**k * sqrt(y)
        //   2. Bit by bit computation
        //      Let q  = sqrt(y) truncated to i bit after binary point (q = 1),
        //           i                                                   0
        //                                     i+1         2
        //          s  = 2*q , and      y  =  2   * ( y - q  ).          (1)
        //           i      i            i                 i
        //
        //      To compute q    from q , one checks whether
        //                  i+1       i
        //
        //                            -(i+1) 2
        //                      (q + 2      )  <= y.                     (2)
        //                        i
        //                                                            -(i+1)
        //      If (2) is false, then q   = q ; otherwise q   = q  + 2      .
        //                             i+1   i             i+1   i
        //
        //      With some algebraic manipulation, it is not difficult to see
        //      that (2) is equivalent to
        //                             -(i+1)
        //                      s  +  2       <= y                       (3)
        //                       i                i
        //
        //      The advantage of (3) is that s  and y  can be computed by
        //                                    i      i
        //      the following recurrence formula:
        //          if (3) is false
        //
        //          s     =  s  ,       y    = y   ;                     (4)
        //           i+1      i          i+1    i
        //
        //      otherwise,
        //                         -i                      -(i+1)
        //          s     =  s  + 2  ,  y    = y  -  s  - 2              (5)
        //           i+1      i          i+1    i     i
        //
        //      One may easily use induction to prove (4) and (5).
        //      Note. Since the left hand side of (3) contain only i+2 bits,
        //            it does not necessary to do a full (53-bit) comparison
        //            in (3).
        //   3. Final rounding
        //      After generating the 53 bits result, we compute one more bit.
        //      Together with the remainder, we can decide whether the
        //      result is exact, bigger than 1/2ulp, or less than 1/2ulp
        //      (it will never equal to 1/2ulp).
        //      The rounding mode can be detected by checking whether
        //      huge + tiny is equal to huge, and whether huge - tiny is
        //      equal to huge for some floating point number "huge" and "tiny".
        //
        //
        // Notes:  Rounding mode detection omitted.
        private static readonly ulong float64Mask = 0x7FFUL;
        private static readonly long float64Shift = 64L - 11L - 1L;
        private static readonly long float64Bias = 1023L;
        private static readonly ulong float64NaN = 0x7FF8000000000001UL;
        private static readonly ulong float64Inf = 0x7FF0000000000000UL;
        private static readonly float maxFloat64 = 1.797693134862315708145274237317043567981e+308F; // 2**1023 * (2**53 - 1) / 2**52

        // isnanu returns whether ix represents a NaN floating point number.
        private static bool isnanu(ulong ix)
        {
            var exp = (ix >> (int)(float64Shift)) & float64Mask;
            var sig = ix << (int)((64L - float64Shift)) >> (int)((64L - float64Shift));
            return exp == float64Mask && sig != 0L;
        }

        private static ulong sqrt(ulong ix)
        { 
            // special cases

            if (ix == 0L || ix == 1L << (int)(63L)) // x == 0
                return ix;
            else if (isnanu(ix)) // x != x
                return ix;
            else if (ix & (1L << (int)(63L)) != 0L) // x < 0
                return float64NaN;
            else if (ix == float64Inf) // x > MaxFloat
                return ix;
            // normalize x
            var exp = int((ix >> (int)(float64Shift)) & float64Mask);
            if (exp == 0L)
            { // subnormal x
                while (ix & (1L << (int)(float64Shift)) == 0L)
                {
                    ix <<= 1L;
                    exp--;
                }

                exp++;
            }
            exp -= float64Bias; // unbias exponent
            ix &= float64Mask << (int)(float64Shift);
            ix |= 1L << (int)(float64Shift);
            if (exp & 1L == 1L)
            { // odd exp, double x to make it even
                ix <<= 1L;
            }
            exp >>= 1L; // exp = exp/2, exponent of square root
            // generate sqrt(x) bit by bit
            ix <<= 1L;
            ulong q = default;            ulong s = default; // q = sqrt(x)
 // q = sqrt(x)
            var r = uint64(1L << (int)((float64Shift + 1L))); // r = moving bit from MSB to LSB
            while (r != 0L)
            {
                var t = s + r;
                if (t <= ix)
                {
                    s = t + r;
                    ix -= t;
                    q += r;
                }
                ix <<= 1L;
                r >>= 1L;
            } 
            // final rounding
 
            // final rounding
            if (ix != 0L)
            { // remainder, result not exact
                q += q & 1L; // round according to extra bit
            }
            ix = q >> (int)(1L) + uint64(exp - 1L + float64Bias) << (int)(float64Shift); // significand + biased exponent
            return ix;
        }
    }
}
