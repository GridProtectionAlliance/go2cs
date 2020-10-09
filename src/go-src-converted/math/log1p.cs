// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:45 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\log1p.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The original C code, the long comment, and the constants
        // below are from FreeBSD's /usr/src/lib/msun/src/s_log1p.c
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
        // double log1p(double x)
        //
        // Method :
        //   1. Argument Reduction: find k and f such that
        //                      1+x = 2**k * (1+f),
        //         where  sqrt(2)/2 < 1+f < sqrt(2) .
        //
        //      Note. If k=0, then f=x is exact. However, if k!=0, then f
        //      may not be representable exactly. In that case, a correction
        //      term is need. Let u=1+x rounded. Let c = (1+x)-u, then
        //      log(1+x) - log(u) ~ c/u. Thus, we proceed to compute log(u),
        //      and add back the correction term c/u.
        //      (Note: when x > 2**53, one can simply return log(x))
        //
        //   2. Approximation of log1p(f).
        //      Let s = f/(2+f) ; based on log(1+f) = log(1+s) - log(1-s)
        //               = 2s + 2/3 s**3 + 2/5 s**5 + .....,
        //               = 2s + s*R
        //      We use a special Reme algorithm on [0,0.1716] to generate
        //      a polynomial of degree 14 to approximate R The maximum error
        //      of this polynomial approximation is bounded by 2**-58.45. In
        //      other words,
        //                      2      4      6      8      10      12      14
        //          R(z) ~ Lp1*s +Lp2*s +Lp3*s +Lp4*s +Lp5*s  +Lp6*s  +Lp7*s
        //      (the values of Lp1 to Lp7 are listed in the program)
        //      and
        //          |      2          14          |     -58.45
        //          | Lp1*s +...+Lp7*s    -  R(z) | <= 2
        //          |                             |
        //      Note that 2s = f - s*f = f - hfsq + s*hfsq, where hfsq = f*f/2.
        //      In order to guarantee error in log below 1ulp, we compute log
        //      by
        //              log1p(f) = f - (hfsq - s*(hfsq+R)).
        //
        //   3. Finally, log1p(x) = k*ln2 + log1p(f).
        //                        = k*ln2_hi+(f-(hfsq-(s*(hfsq+R)+k*ln2_lo)))
        //      Here ln2 is split into two floating point number:
        //                   ln2_hi + ln2_lo,
        //      where n*ln2_hi is always exact for |n| < 2000.
        //
        // Special cases:
        //      log1p(x) is NaN with signal if x < -1 (including -INF) ;
        //      log1p(+INF) is +INF; log1p(-1) is -INF with signal;
        //      log1p(NaN) is that NaN with no signal.
        //
        // Accuracy:
        //      according to an error analysis, the error is always less than
        //      1 ulp (unit in the last place).
        //
        // Constants:
        // The hexadecimal values are the intended ones for the following
        // constants. The decimal values may be used, provided that the
        // compiler will convert from decimal to binary accurately enough
        // to produce the hexadecimal values shown.
        //
        // Note: Assuming log() return accurate answer, the following
        //       algorithm can be used to compute log1p(x) to within a few ULP:
        //
        //              u = 1+x;
        //              if(u==1.0) return x ; else
        //                         return log(u)*(x/(u-1.0));
        //
        //       See HP-15C Advanced Functions Handbook, p.193.

        // Log1p returns the natural logarithm of 1 plus its argument x.
        // It is more accurate than Log(1 + x) when x is near zero.
        //
        // Special cases are:
        //    Log1p(+Inf) = +Inf
        //    Log1p(±0) = ±0
        //    Log1p(-1) = -Inf
        //    Log1p(x < -1) = NaN
        //    Log1p(NaN) = NaN
        public static double Log1p(double x)
;

        private static double log1p(double x)
        {
            const float Sqrt2M1 = (float)4.142135623730950488017e-01F; // Sqrt(2)-1 = 0x3fda827999fcef34
            const float Sqrt2HalfM1 = (float)-2.928932188134524755992e-01F; // Sqrt(2)/2-1 = 0xbfd2bec333018866
            const float Small = (float)1.0F / (1L << (int)(29L)); // 2**-29 = 0x3e20000000000000
            const float Tiny = (float)1.0F / (1L << (int)(54L)); // 2**-54
            const long Two53 = (long)1L << (int)(53L); // 2**53
            const float Ln2Hi = (float)6.93147180369123816490e-01F; // 3fe62e42fee00000
            const float Ln2Lo = (float)1.90821492927058770002e-10F; // 3dea39ef35793c76
            const float Lp1 = (float)6.666666666666735130e-01F; // 3FE5555555555593
            const float Lp2 = (float)3.999999999940941908e-01F; // 3FD999999997FA04
            const float Lp3 = (float)2.857142874366239149e-01F; // 3FD2492494229359
            const float Lp4 = (float)2.222219843214978396e-01F; // 3FCC71C51D8E78AF
            const float Lp5 = (float)1.818357216161805012e-01F; // 3FC7466496CB03DE
            const float Lp6 = (float)1.531383769920937332e-01F; // 3FC39A09D078C69F
            const float Lp7 = (float)1.479819860511658591e-01F; // 3FC2F112DF3E5244 

            // special cases

            if (x < -1L || IsNaN(x)) // includes -Inf
                return NaN();
            else if (x == -1L) 
                return Inf(-1L);
            else if (IsInf(x, 1L)) 
                return Inf(1L);
                        var absx = x;
            if (absx < 0L)
            {>>MARKER:FUNCTION_Log1p_BLOCK_PREFIX<<
                absx = -absx;
            }

            double f = default;
            ulong iu = default;
            long k = 1L;
            if (absx < Sqrt2M1)
            { //  |x| < Sqrt(2)-1
                if (absx < Small)
                { // |x| < 2**-29
                    if (absx < Tiny)
                    { // |x| < 2**-54
                        return x;

                    }

                    return x - x * x * 0.5F;

                }

                if (x > Sqrt2HalfM1)
                { // Sqrt(2)/2-1 < x
                    // (Sqrt(2)/2-1) < x < (Sqrt(2)-1)
                    k = 0L;
                    f = x;
                    iu = 1L;

                }

            }

            double c = default;
            if (k != 0L)
            {
                double u = default;
                if (absx < Two53)
                { // 1<<53
                    u = 1.0F + x;
                    iu = Float64bits(u);
                    k = int((iu >> (int)(52L)) - 1023L); 
                    // correction term
                    if (k > 0L)
                    {
                        c = 1.0F - (u - x);
                    }
                    else
                    {
                        c = x - (u - 1.0F);
                    }

                    c /= u;

                }
                else
                {
                    u = x;
                    iu = Float64bits(u);
                    k = int((iu >> (int)(52L)) - 1023L);
                    c = 0L;
                }

                iu &= 0x000fffffffffffffUL;
                if (iu < 0x0006a09e667f3bcdUL)
                { // mantissa of Sqrt(2)
                    u = Float64frombits(iu | 0x3ff0000000000000UL); // normalize u
                }
                else
                {
                    k++;
                    u = Float64frombits(iu | 0x3fe0000000000000UL); // normalize u/2
                    iu = (0x0010000000000000UL - iu) >> (int)(2L);

                }

                f = u - 1.0F; // Sqrt(2)/2 < u < Sqrt(2)
            }

            float hfsq = 0.5F * f * f;
            double s = default;            double R = default;            double z = default;

            if (iu == 0L)
            { // |f| < 2**-20
                if (f == 0L)
                {
                    if (k == 0L)
                    {
                        return 0L;
                    }

                    c += float64(k) * Ln2Lo;
                    return float64(k) * Ln2Hi + c;

                }

                R = hfsq * (1.0F - 0.66666666666666666F * f); // avoid division
                if (k == 0L)
                {
                    return f - R;
                }

                return float64(k) * Ln2Hi - ((R - (float64(k) * Ln2Lo + c)) - f);

            }

            s = f / (2.0F + f);
            z = s * s;
            R = z * (Lp1 + z * (Lp2 + z * (Lp3 + z * (Lp4 + z * (Lp5 + z * (Lp6 + z * Lp7))))));
            if (k == 0L)
            {
                return f - (hfsq - s * (hfsq + R));
            }

            return float64(k) * Ln2Hi - ((hfsq - (s * (hfsq + R) + (float64(k) * Ln2Lo + c))) - f);

        }
    }
}
