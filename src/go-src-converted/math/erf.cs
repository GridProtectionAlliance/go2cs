// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 08 03:25:11 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\erf.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Floating-point error function and complementary error function.
        */

        // The original C code and the long comment below are
        // from FreeBSD's /usr/src/lib/msun/src/s_erf.c and
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
        //
        // double erf(double x)
        // double erfc(double x)
        //                           x
        //                    2      |\
        //     erf(x)  =  ---------  | exp(-t*t)dt
        //                 sqrt(pi) \|
        //                           0
        //
        //     erfc(x) =  1-erf(x)
        //  Note that
        //              erf(-x) = -erf(x)
        //              erfc(-x) = 2 - erfc(x)
        //
        // Method:
        //      1. For |x| in [0, 0.84375]
        //          erf(x)  = x + x*R(x**2)
        //          erfc(x) = 1 - erf(x)           if x in [-.84375,0.25]
        //                  = 0.5 + ((0.5-x)-x*R)  if x in [0.25,0.84375]
        //         where R = P/Q where P is an odd poly of degree 8 and
        //         Q is an odd poly of degree 10.
        //                                               -57.90
        //                      | R - (erf(x)-x)/x | <= 2
        //
        //
        //         Remark. The formula is derived by noting
        //          erf(x) = (2/sqrt(pi))*(x - x**3/3 + x**5/10 - x**7/42 + ....)
        //         and that
        //          2/sqrt(pi) = 1.128379167095512573896158903121545171688
        //         is close to one. The interval is chosen because the fix
        //         point of erf(x) is near 0.6174 (i.e., erf(x)=x when x is
        //         near 0.6174), and by some experiment, 0.84375 is chosen to
        //         guarantee the error is less than one ulp for erf.
        //
        //      2. For |x| in [0.84375,1.25], let s = |x| - 1, and
        //         c = 0.84506291151 rounded to single (24 bits)
        //              erf(x)  = sign(x) * (c  + P1(s)/Q1(s))
        //              erfc(x) = (1-c)  - P1(s)/Q1(s) if x > 0
        //                        1+(c+P1(s)/Q1(s))    if x < 0
        //              |P1/Q1 - (erf(|x|)-c)| <= 2**-59.06
        //         Remark: here we use the taylor series expansion at x=1.
        //              erf(1+s) = erf(1) + s*Poly(s)
        //                       = 0.845.. + P1(s)/Q1(s)
        //         That is, we use rational approximation to approximate
        //                      erf(1+s) - (c = (single)0.84506291151)
        //         Note that |P1/Q1|< 0.078 for x in [0.84375,1.25]
        //         where
        //              P1(s) = degree 6 poly in s
        //              Q1(s) = degree 6 poly in s
        //
        //      3. For x in [1.25,1/0.35(~2.857143)],
        //              erfc(x) = (1/x)*exp(-x*x-0.5625+R1/S1)
        //              erf(x)  = 1 - erfc(x)
        //         where
        //              R1(z) = degree 7 poly in z, (z=1/x**2)
        //              S1(z) = degree 8 poly in z
        //
        //      4. For x in [1/0.35,28]
        //              erfc(x) = (1/x)*exp(-x*x-0.5625+R2/S2) if x > 0
        //                      = 2.0 - (1/x)*exp(-x*x-0.5625+R2/S2) if -6<x<0
        //                      = 2.0 - tiny            (if x <= -6)
        //              erf(x)  = sign(x)*(1.0 - erfc(x)) if x < 6, else
        //              erf(x)  = sign(x)*(1.0 - tiny)
        //         where
        //              R2(z) = degree 6 poly in z, (z=1/x**2)
        //              S2(z) = degree 7 poly in z
        //
        //      Note1:
        //         To compute exp(-x*x-0.5625+R/S), let s be a single
        //         precision number and s := x; then
        //              -x*x = -s*s + (s-x)*(s+x)
        //              exp(-x*x-0.5626+R/S) =
        //                      exp(-s*s-0.5625)*exp((s-x)*(s+x)+R/S);
        //      Note2:
        //         Here 4 and 5 make use of the asymptotic series
        //                        exp(-x*x)
        //              erfc(x) ~ ---------- * ( 1 + Poly(1/x**2) )
        //                        x*sqrt(pi)
        //         We use rational approximation to approximate
        //              g(s)=f(1/x**2) = log(erfc(x)*x) - x*x + 0.5625
        //         Here is the error bound for R1/S1 and R2/S2
        //              |R1/S1 - f(x)|  < 2**(-62.57)
        //              |R2/S2 - f(x)|  < 2**(-61.52)
        //
        //      5. For inf > x >= 28
        //              erf(x)  = sign(x) *(1 - tiny)  (raise inexact)
        //              erfc(x) = tiny*tiny (raise underflow) if x > 0
        //                      = 2 - tiny if x<0
        //
        //      7. Special case:
        //              erf(0)  = 0, erf(inf)  = 1, erf(-inf) = -1,
        //              erfc(0) = 1, erfc(inf) = 0, erfc(-inf) = 2,
        //              erfc/erf(NaN) is NaN
        private static readonly float erx = (float)8.45062911510467529297e-01F; // 0x3FEB0AC160000000
        // Coefficients for approximation to  erf in [0, 0.84375]
        private static readonly float efx = (float)1.28379167095512586316e-01F; // 0x3FC06EBA8214DB69
        private static readonly float efx8 = (float)1.02703333676410069053e+00F; // 0x3FF06EBA8214DB69
        private static readonly float pp0 = (float)1.28379167095512558561e-01F; // 0x3FC06EBA8214DB68
        private static readonly float pp1 = (float)-3.25042107247001499370e-01F; // 0xBFD4CD7D691CB913
        private static readonly float pp2 = (float)-2.84817495755985104766e-02F; // 0xBF9D2A51DBD7194F
        private static readonly float pp3 = (float)-5.77027029648944159157e-03F; // 0xBF77A291236668E4
        private static readonly float pp4 = (float)-2.37630166566501626084e-05F; // 0xBEF8EAD6120016AC
        private static readonly float qq1 = (float)3.97917223959155352819e-01F; // 0x3FD97779CDDADC09
        private static readonly float qq2 = (float)6.50222499887672944485e-02F; // 0x3FB0A54C5536CEBA
        private static readonly float qq3 = (float)5.08130628187576562776e-03F; // 0x3F74D022C4D36B0F
        private static readonly float qq4 = (float)1.32494738004321644526e-04F; // 0x3F215DC9221C1A10
        private static readonly float qq5 = (float)-3.96022827877536812320e-06F; // 0xBED09C4342A26120
        // Coefficients for approximation to  erf  in [0.84375, 1.25]
        private static readonly float pa0 = (float)-2.36211856075265944077e-03F; // 0xBF6359B8BEF77538
        private static readonly float pa1 = (float)4.14856118683748331666e-01F; // 0x3FDA8D00AD92B34D
        private static readonly float pa2 = (float)-3.72207876035701323847e-01F; // 0xBFD7D240FBB8C3F1
        private static readonly float pa3 = (float)3.18346619901161753674e-01F; // 0x3FD45FCA805120E4
        private static readonly float pa4 = (float)-1.10894694282396677476e-01F; // 0xBFBC63983D3E28EC
        private static readonly float pa5 = (float)3.54783043256182359371e-02F; // 0x3FA22A36599795EB
        private static readonly float pa6 = (float)-2.16637559486879084300e-03F; // 0xBF61BF380A96073F
        private static readonly float qa1 = (float)1.06420880400844228286e-01F; // 0x3FBB3E6618EEE323
        private static readonly float qa2 = (float)5.40397917702171048937e-01F; // 0x3FE14AF092EB6F33
        private static readonly float qa3 = (float)7.18286544141962662868e-02F; // 0x3FB2635CD99FE9A7
        private static readonly float qa4 = (float)1.26171219808761642112e-01F; // 0x3FC02660E763351F
        private static readonly float qa5 = (float)1.36370839120290507362e-02F; // 0x3F8BEDC26B51DD1C
        private static readonly float qa6 = (float)1.19844998467991074170e-02F; // 0x3F888B545735151D
        // Coefficients for approximation to  erfc in [1.25, 1/0.35]
        private static readonly float ra0 = (float)-9.86494403484714822705e-03F; // 0xBF843412600D6435
        private static readonly float ra1 = (float)-6.93858572707181764372e-01F; // 0xBFE63416E4BA7360
        private static readonly float ra2 = (float)-1.05586262253232909814e+01F; // 0xC0251E0441B0E726
        private static readonly float ra3 = (float)-6.23753324503260060396e+01F; // 0xC04F300AE4CBA38D
        private static readonly float ra4 = (float)-1.62396669462573470355e+02F; // 0xC0644CB184282266
        private static readonly float ra5 = (float)-1.84605092906711035994e+02F; // 0xC067135CEBCCABB2
        private static readonly float ra6 = (float)-8.12874355063065934246e+01F; // 0xC054526557E4D2F2
        private static readonly float ra7 = (float)-9.81432934416914548592e+00F; // 0xC023A0EFC69AC25C
        private static readonly float sa1 = (float)1.96512716674392571292e+01F; // 0x4033A6B9BD707687
        private static readonly float sa2 = (float)1.37657754143519042600e+02F; // 0x4061350C526AE721
        private static readonly float sa3 = (float)4.34565877475229228821e+02F; // 0x407B290DD58A1A71
        private static readonly float sa4 = (float)6.45387271733267880336e+02F; // 0x40842B1921EC2868
        private static readonly float sa5 = (float)4.29008140027567833386e+02F; // 0x407AD02157700314
        private static readonly float sa6 = (float)1.08635005541779435134e+02F; // 0x405B28A3EE48AE2C
        private static readonly float sa7 = (float)6.57024977031928170135e+00F; // 0x401A47EF8E484A93
        private static readonly float sa8 = (float)-6.04244152148580987438e-02F; // 0xBFAEEFF2EE749A62
        // Coefficients for approximation to  erfc in [1/.35, 28]
        private static readonly float rb0 = (float)-9.86494292470009928597e-03F; // 0xBF84341239E86F4A
        private static readonly float rb1 = (float)-7.99283237680523006574e-01F; // 0xBFE993BA70C285DE
        private static readonly float rb2 = (float)-1.77579549177547519889e+01F; // 0xC031C209555F995A
        private static readonly float rb3 = (float)-1.60636384855821916062e+02F; // 0xC064145D43C5ED98
        private static readonly float rb4 = (float)-6.37566443368389627722e+02F; // 0xC083EC881375F228
        private static readonly float rb5 = (float)-1.02509513161107724954e+03F; // 0xC09004616A2E5992
        private static readonly float rb6 = (float)-4.83519191608651397019e+02F; // 0xC07E384E9BDC383F
        private static readonly float sb1 = (float)3.03380607434824582924e+01F; // 0x403E568B261D5190
        private static readonly float sb2 = (float)3.25792512996573918826e+02F; // 0x40745CAE221B9F0A
        private static readonly float sb3 = (float)1.53672958608443695994e+03F; // 0x409802EB189D5118
        private static readonly float sb4 = (float)3.19985821950859553908e+03F; // 0x40A8FFB7688C246A
        private static readonly float sb5 = (float)2.55305040643316442583e+03F; // 0x40A3F219CEDF3BE6
        private static readonly float sb6 = (float)4.74528541206955367215e+02F; // 0x407DA874E79FE763
        private static readonly float sb7 = (float)-2.24409524465858183362e+01F; // 0xC03670E242712D62

        // Erf returns the error function of x.
        //
        // Special cases are:
        //    Erf(+Inf) = 1
        //    Erf(-Inf) = -1
        //    Erf(NaN) = NaN
        public static double Erf(double x)
;

        private static double erf(double x)
        {
            const float VeryTiny = (float)2.848094538889218e-306F; // 0x0080000000000000
            const float Small = (float)1.0F / (1L << (int)(28L)); // 2**-28 
            // special cases

            if (IsNaN(x)) 
                return NaN();
            else if (IsInf(x, 1L)) 
                return 1L;
            else if (IsInf(x, -1L)) 
                return -1L;
                        var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Erf_BLOCK_PREFIX<<
                x = -x;
                sign = true;
            }

            if (x < 0.84375F)
            { // |x| < 0.84375
                double temp = default;
                if (x < Small)
                { // |x| < 2**-28
                    if (x < VeryTiny)
                    {
                        temp = 0.125F * (8.0F * x + efx8 * x); // avoid underflow
                    }
                    else
                    {
                        temp = x + efx * x;
                    }

                }
                else
                {
                    var z = x * x;
                    var r = pp0 + z * (pp1 + z * (pp2 + z * (pp3 + z * pp4)));
                    long s = 1L + z * (qq1 + z * (qq2 + z * (qq3 + z * (qq4 + z * qq5))));
                    var y = r / s;
                    temp = x + x * y;
                }

                if (sign)
                {
                    return -temp;
                }

                return temp;

            }

            if (x < 1.25F)
            { // 0.84375 <= |x| < 1.25
                s = x - 1L;
                var P = pa0 + s * (pa1 + s * (pa2 + s * (pa3 + s * (pa4 + s * (pa5 + s * pa6)))));
                long Q = 1L + s * (qa1 + s * (qa2 + s * (qa3 + s * (qa4 + s * (qa5 + s * qa6)))));
                if (sign)
                {
                    return -erx - P / Q;
                }

                return erx + P / Q;

            }

            if (x >= 6L)
            { // inf > |x| >= 6
                if (sign)
                {
                    return -1L;
                }

                return 1L;

            }

            s = 1L / (x * x);
            double R = default;            double S = default;

            if (x < 1L / 0.35F)
            { // |x| < 1 / 0.35  ~ 2.857143
                R = ra0 + s * (ra1 + s * (ra2 + s * (ra3 + s * (ra4 + s * (ra5 + s * (ra6 + s * ra7))))));
                S = 1L + s * (sa1 + s * (sa2 + s * (sa3 + s * (sa4 + s * (sa5 + s * (sa6 + s * (sa7 + s * sa8)))))));

            }
            else
            { // |x| >= 1 / 0.35  ~ 2.857143
                R = rb0 + s * (rb1 + s * (rb2 + s * (rb3 + s * (rb4 + s * (rb5 + s * rb6)))));
                S = 1L + s * (sb1 + s * (sb2 + s * (sb3 + s * (sb4 + s * (sb5 + s * (sb6 + s * sb7))))));

            }

            z = Float64frombits(Float64bits(x) & 0xffffffff00000000UL); // pseudo-single (20-bit) precision x
            r = Exp(-z * z - 0.5625F) * Exp((z - x) * (z + x) + R / S);
            if (sign)
            {
                return r / x - 1L;
            }

            return 1L - r / x;

        }

        // Erfc returns the complementary error function of x.
        //
        // Special cases are:
        //    Erfc(+Inf) = 0
        //    Erfc(-Inf) = 2
        //    Erfc(NaN) = NaN
        public static double Erfc(double x)
;

        private static double erfc(double x)
        {
            const float Tiny = (float)1.0F / (1L << (int)(56L)); // 2**-56
            // special cases
 // 2**-56
            // special cases

            if (IsNaN(x)) 
                return NaN();
            else if (IsInf(x, 1L)) 
                return 0L;
            else if (IsInf(x, -1L)) 
                return 2L;
                        var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Erfc_BLOCK_PREFIX<<
                x = -x;
                sign = true;
            }

            if (x < 0.84375F)
            { // |x| < 0.84375
                double temp = default;
                if (x < Tiny)
                { // |x| < 2**-56
                    temp = x;

                }
                else
                {
                    var z = x * x;
                    var r = pp0 + z * (pp1 + z * (pp2 + z * (pp3 + z * pp4)));
                    long s = 1L + z * (qq1 + z * (qq2 + z * (qq3 + z * (qq4 + z * qq5))));
                    var y = r / s;
                    if (x < 0.25F)
                    { // |x| < 1/4
                        temp = x + x * y;

                    }
                    else
                    {
                        temp = 0.5F + (x * y + (x - 0.5F));
                    }

                }

                if (sign)
                {
                    return 1L + temp;
                }

                return 1L - temp;

            }

            if (x < 1.25F)
            { // 0.84375 <= |x| < 1.25
                s = x - 1L;
                var P = pa0 + s * (pa1 + s * (pa2 + s * (pa3 + s * (pa4 + s * (pa5 + s * pa6)))));
                long Q = 1L + s * (qa1 + s * (qa2 + s * (qa3 + s * (qa4 + s * (qa5 + s * qa6)))));
                if (sign)
                {
                    return 1L + erx + P / Q;
                }

                return 1L - erx - P / Q;


            }

            if (x < 28L)
            { // |x| < 28
                s = 1L / (x * x);
                double R = default;                double S = default;

                if (x < 1L / 0.35F)
                { // |x| < 1 / 0.35 ~ 2.857143
                    R = ra0 + s * (ra1 + s * (ra2 + s * (ra3 + s * (ra4 + s * (ra5 + s * (ra6 + s * ra7))))));
                    S = 1L + s * (sa1 + s * (sa2 + s * (sa3 + s * (sa4 + s * (sa5 + s * (sa6 + s * (sa7 + s * sa8)))))));

                }
                else
                { // |x| >= 1 / 0.35 ~ 2.857143
                    if (sign && x > 6L)
                    {
                        return 2L; // x < -6
                    }

                    R = rb0 + s * (rb1 + s * (rb2 + s * (rb3 + s * (rb4 + s * (rb5 + s * rb6)))));
                    S = 1L + s * (sb1 + s * (sb2 + s * (sb3 + s * (sb4 + s * (sb5 + s * (sb6 + s * sb7))))));

                }

                z = Float64frombits(Float64bits(x) & 0xffffffff00000000UL); // pseudo-single (20-bit) precision x
                r = Exp(-z * z - 0.5625F) * Exp((z - x) * (z + x) + R / S);
                if (sign)
                {
                    return 2L - r / x;
                }

                return r / x;

            }

            if (sign)
            {
                return 2L;
            }

            return 0L;

        }
    }
}
