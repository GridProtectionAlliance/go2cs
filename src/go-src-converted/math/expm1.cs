// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:41 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\expm1.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The original C code, the long comment, and the constants
        // below are from FreeBSD's /usr/src/lib/msun/src/s_expm1.c
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
        // expm1(x)
        // Returns exp(x)-1, the exponential of x minus 1.
        //
        // Method
        //   1. Argument reduction:
        //      Given x, find r and integer k such that
        //
        //               x = k*ln2 + r,  |r| <= 0.5*ln2 ~ 0.34658
        //
        //      Here a correction term c will be computed to compensate
        //      the error in r when rounded to a floating-point number.
        //
        //   2. Approximating expm1(r) by a special rational function on
        //      the interval [0,0.34658]:
        //      Since
        //          r*(exp(r)+1)/(exp(r)-1) = 2+ r**2/6 - r**4/360 + ...
        //      we define R1(r*r) by
        //          r*(exp(r)+1)/(exp(r)-1) = 2+ r**2/6 * R1(r*r)
        //      That is,
        //          R1(r**2) = 6/r *((exp(r)+1)/(exp(r)-1) - 2/r)
        //                   = 6/r * ( 1 + 2.0*(1/(exp(r)-1) - 1/r))
        //                   = 1 - r**2/60 + r**4/2520 - r**6/100800 + ...
        //      We use a special Reme algorithm on [0,0.347] to generate
        //      a polynomial of degree 5 in r*r to approximate R1. The
        //      maximum error of this polynomial approximation is bounded
        //      by 2**-61. In other words,
        //          R1(z) ~ 1.0 + Q1*z + Q2*z**2 + Q3*z**3 + Q4*z**4 + Q5*z**5
        //      where   Q1  =  -1.6666666666666567384E-2,
        //              Q2  =   3.9682539681370365873E-4,
        //              Q3  =  -9.9206344733435987357E-6,
        //              Q4  =   2.5051361420808517002E-7,
        //              Q5  =  -6.2843505682382617102E-9;
        //      (where z=r*r, and the values of Q1 to Q5 are listed below)
        //      with error bounded by
        //          |                  5           |     -61
        //          | 1.0+Q1*z+...+Q5*z   -  R1(z) | <= 2
        //          |                              |
        //
        //      expm1(r) = exp(r)-1 is then computed by the following
        //      specific way which minimize the accumulation rounding error:
        //                             2     3
        //                            r     r    [ 3 - (R1 + R1*r/2)  ]
        //            expm1(r) = r + --- + --- * [--------------------]
        //                            2     2    [ 6 - r*(3 - R1*r/2) ]
        //
        //      To compensate the error in the argument reduction, we use
        //              expm1(r+c) = expm1(r) + c + expm1(r)*c
        //                         ~ expm1(r) + c + r*c
        //      Thus c+r*c will be added in as the correction terms for
        //      expm1(r+c). Now rearrange the term to avoid optimization
        //      screw up:
        //                      (      2                                    2 )
        //                      ({  ( r    [ R1 -  (3 - R1*r/2) ]  )  }    r  )
        //       expm1(r+c)~r - ({r*(--- * [--------------------]-c)-c} - --- )
        //                      ({  ( 2    [ 6 - r*(3 - R1*r/2) ]  )  }    2  )
        //                      (                                             )
        //
        //                 = r - E
        //   3. Scale back to obtain expm1(x):
        //      From step 1, we have
        //         expm1(x) = either 2**k*[expm1(r)+1] - 1
        //                  = or     2**k*[expm1(r) + (1-2**-k)]
        //   4. Implementation notes:
        //      (A). To save one multiplication, we scale the coefficient Qi
        //           to Qi*2**i, and replace z by (x**2)/2.
        //      (B). To achieve maximum accuracy, we compute expm1(x) by
        //        (i)   if x < -56*ln2, return -1.0, (raise inexact if x!=inf)
        //        (ii)  if k=0, return r-E
        //        (iii) if k=-1, return 0.5*(r-E)-0.5
        //        (iv)  if k=1 if r < -0.25, return 2*((r+0.5)- E)
        //                     else          return  1.0+2.0*(r-E);
        //        (v)   if (k<-2||k>56) return 2**k(1-(E-r)) - 1 (or exp(x)-1)
        //        (vi)  if k <= 20, return 2**k((1-2**-k)-(E-r)), else
        //        (vii) return 2**k(1-((E+2**-k)-r))
        //
        // Special cases:
        //      expm1(INF) is INF, expm1(NaN) is NaN;
        //      expm1(-INF) is -1, and
        //      for finite argument, only expm1(0)=0 is exact.
        //
        // Accuracy:
        //      according to an error analysis, the error is always less than
        //      1 ulp (unit in the last place).
        //
        // Misc. info.
        //      For IEEE double
        //          if x >  7.09782712893383973096e+02 then expm1(x) overflow
        //
        // Constants:
        // The hexadecimal values are the intended ones for the following
        // constants. The decimal values may be used, provided that the
        // compiler will convert from decimal to binary accurately enough
        // to produce the hexadecimal values shown.
        //

        // Expm1 returns e**x - 1, the base-e exponential of x minus 1.
        // It is more accurate than Exp(x) - 1 when x is near zero.
        //
        // Special cases are:
        //    Expm1(+Inf) = +Inf
        //    Expm1(-Inf) = -1
        //    Expm1(NaN) = NaN
        // Very large values overflow to -1 or +Inf.
        public static double Expm1(double x)
;

        private static double expm1(double x)
        {
            const float Othreshold = (float)7.09782712893383973096e+02F; // 0x40862E42FEFA39EF
            const float Ln2X56 = (float)3.88162421113569373274e+01F; // 0x4043687a9f1af2b1
            const float Ln2HalfX3 = (float)1.03972077083991796413e+00F; // 0x3ff0a2b23f3bab73
            const float Ln2Half = (float)3.46573590279972654709e-01F; // 0x3fd62e42fefa39ef
            const float Ln2Hi = (float)6.93147180369123816490e-01F; // 0x3fe62e42fee00000
            const float Ln2Lo = (float)1.90821492927058770002e-10F; // 0x3dea39ef35793c76
            const float InvLn2 = (float)1.44269504088896338700e+00F; // 0x3ff71547652b82fe
            const float Tiny = (float)1.0F / (1L << (int)(54L)); // 2**-54 = 0x3c90000000000000
            // scaled coefficients related to expm1
            const float Q1 = (float)-3.33333333333331316428e-02F; // 0xBFA11111111110F4
            const float Q2 = (float)1.58730158725481460165e-03F; // 0x3F5A01A019FE5585
            const float Q3 = (float)-7.93650757867487942473e-05F; // 0xBF14CE199EAADBB7
            const float Q4 = (float)4.00821782732936239552e-06F; // 0x3ED0CFCA86E65239
            const float Q5 = (float)-2.01099218183624371326e-07F; // 0xBE8AFDB76E09C32D 

            // special cases

            if (IsInf(x, 1L) || IsNaN(x)) 
                return x;
            else if (IsInf(x, -1L)) 
                return -1L;
                        var absx = x;
            var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Expm1_BLOCK_PREFIX<<
                absx = -absx;
                sign = true;
            } 

            // filter out huge argument
            if (absx >= Ln2X56)
            { // if |x| >= 56 * ln2
                if (sign)
                {
                    return -1L; // x < -56*ln2, return -1
                }

                if (absx >= Othreshold)
                { // if |x| >= 709.78...
                    return Inf(1L);

                }

            } 

            // argument reduction
            double c = default;
            long k = default;
            if (absx > Ln2Half)
            { // if  |x| > 0.5 * ln2
                double hi = default;                double lo = default;

                if (absx < Ln2HalfX3)
                { // and |x| < 1.5 * ln2
                    if (!sign)
                    {
                        hi = x - Ln2Hi;
                        lo = Ln2Lo;
                        k = 1L;
                    }
                    else
                    {
                        hi = x + Ln2Hi;
                        lo = -Ln2Lo;
                        k = -1L;
                    }

                }
                else
                {
                    if (!sign)
                    {
                        k = int(InvLn2 * x + 0.5F);
                    }
                    else
                    {
                        k = int(InvLn2 * x - 0.5F);
                    }

                    var t = float64(k);
                    hi = x - t * Ln2Hi; // t * Ln2Hi is exact here
                    lo = t * Ln2Lo;

                }

                x = hi - lo;
                c = (hi - x) - lo;

            }
            else if (absx < Tiny)
            { // when |x| < 2**-54, return x
                return x;

            }
            else
            {
                k = 0L;
            } 

            // x is now in primary range
            float hfx = 0.5F * x;
            var hxs = x * hfx;
            long r1 = 1L + hxs * (Q1 + hxs * (Q2 + hxs * (Q3 + hxs * (Q4 + hxs * Q5))));
            t = 3L - r1 * hfx;
            var e = hxs * ((r1 - t) / (6.0F - x * t));
            if (k == 0L)
            {
                return x - (x * e - hxs); // c is 0
            }

            e = (x * (e - c) - c);
            e -= hxs;

            if (k == -1L) 
                return 0.5F * (x - e) - 0.5F;
            else if (k == 1L) 
                if (x < -0.25F)
                {
                    return -2L * (e - (x + 0.5F));
                }

                return 1L + 2L * (x - e);
            else if (k <= -2L || k > 56L) // suffice to return exp(x)-1
                long y = 1L - (e - x);
                y = Float64frombits(Float64bits(y) + uint64(k) << (int)(52L)); // add k to y's exponent
                return y - 1L;
                        if (k < 20L)
            {
                t = Float64frombits(0x3ff0000000000000UL - (0x20000000000000UL >> (int)(uint(k)))); // t=1-2**-k
                y = t - (e - x);
                y = Float64frombits(Float64bits(y) + uint64(k) << (int)(52L)); // add k to y's exponent
                return y;

            }

            t = Float64frombits(uint64(0x3ffUL - k) << (int)(52L)); // 2**-k
            y = x - (e + t);
            y++;
            y = Float64frombits(Float64bits(y) + uint64(k) << (int)(52L)); // add k to y's exponent
            return y;

        }
    }
}
