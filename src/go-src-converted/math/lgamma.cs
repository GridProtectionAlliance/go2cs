// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:54 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\lgamma.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Floating-point logarithm of the Gamma function.
        */

        // The original C code and the long comment below are
        // from FreeBSD's /usr/src/lib/msun/src/e_lgamma_r.c and
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
        // __ieee754_lgamma_r(x, signgamp)
        // Reentrant version of the logarithm of the Gamma function
        // with user provided pointer for the sign of Gamma(x).
        //
        // Method:
        //   1. Argument Reduction for 0 < x <= 8
        //      Since gamma(1+s)=s*gamma(s), for x in [0,8], we may
        //      reduce x to a number in [1.5,2.5] by
        //              lgamma(1+s) = log(s) + lgamma(s)
        //      for example,
        //              lgamma(7.3) = log(6.3) + lgamma(6.3)
        //                          = log(6.3*5.3) + lgamma(5.3)
        //                          = log(6.3*5.3*4.3*3.3*2.3) + lgamma(2.3)
        //   2. Polynomial approximation of lgamma around its
        //      minimum (ymin=1.461632144968362245) to maintain monotonicity.
        //      On [ymin-0.23, ymin+0.27] (i.e., [1.23164,1.73163]), use
        //              Let z = x-ymin;
        //              lgamma(x) = -1.214862905358496078218 + z**2*poly(z)
        //              poly(z) is a 14 degree polynomial.
        //   2. Rational approximation in the primary interval [2,3]
        //      We use the following approximation:
        //              s = x-2.0;
        //              lgamma(x) = 0.5*s + s*P(s)/Q(s)
        //      with accuracy
        //              |P/Q - (lgamma(x)-0.5s)| < 2**-61.71
        //      Our algorithms are based on the following observation
        //
        //                             zeta(2)-1    2    zeta(3)-1    3
        // lgamma(2+s) = s*(1-Euler) + --------- * s  -  --------- * s  + ...
        //                                 2                 3
        //
        //      where Euler = 0.5772156649... is the Euler constant, which
        //      is very close to 0.5.
        //
        //   3. For x>=8, we have
        //      lgamma(x)~(x-0.5)log(x)-x+0.5*log(2pi)+1/(12x)-1/(360x**3)+....
        //      (better formula:
        //         lgamma(x)~(x-0.5)*(log(x)-1)-.5*(log(2pi)-1) + ...)
        //      Let z = 1/x, then we approximation
        //              f(z) = lgamma(x) - (x-0.5)(log(x)-1)
        //      by
        //                                  3       5             11
        //              w = w0 + w1*z + w2*z  + w3*z  + ... + w6*z
        //      where
        //              |w - f(z)| < 2**-58.74
        //
        //   4. For negative x, since (G is gamma function)
        //              -x*G(-x)*G(x) = pi/sin(pi*x),
        //      we have
        //              G(x) = pi/(sin(pi*x)*(-x)*G(-x))
        //      since G(-x) is positive, sign(G(x)) = sign(sin(pi*x)) for x<0
        //      Hence, for x<0, signgam = sign(sin(pi*x)) and
        //              lgamma(x) = log(|Gamma(x)|)
        //                        = log(pi/(|x*sin(pi*x)|)) - lgamma(-x);
        //      Note: one should avoid computing pi*(-x) directly in the
        //            computation of sin(pi*(-x)).
        //
        //   5. Special Cases
        //              lgamma(2+s) ~ s*(1-Euler) for tiny s
        //              lgamma(1)=lgamma(2)=0
        //              lgamma(x) ~ -log(x) for tiny x
        //              lgamma(0) = lgamma(inf) = inf
        //              lgamma(-integer) = +-inf
        //
        //
        private static array<double> _lgamA = new array<double>(new double[] { 7.72156649015328655494e-02, 3.22467033424113591611e-01, 6.73523010531292681824e-02, 2.05808084325167332806e-02, 7.38555086081402883957e-03, 2.89051383673415629091e-03, 1.19270763183362067845e-03, 5.10069792153511336608e-04, 2.20862790713908385557e-04, 1.08011567247583939954e-04, 2.52144565451257326939e-05, 4.48640949618915160150e-05 });
        private static array<double> _lgamR = new array<double>(new double[] { 1.0, 1.39200533467621045958e+00, 7.21935547567138069525e-01, 1.71933865632803078993e-01, 1.86459191715652901344e-02, 7.77942496381893596434e-04, 7.32668430744625636189e-06 });
        private static array<double> _lgamS = new array<double>(new double[] { -7.72156649015328655494e-02, 2.14982415960608852501e-01, 3.25778796408930981787e-01, 1.46350472652464452805e-01, 2.66422703033638609560e-02, 1.84028451407337715652e-03, 3.19475326584100867617e-05 });
        private static array<double> _lgamT = new array<double>(new double[] { 4.83836122723810047042e-01, -1.47587722994593911752e-01, 6.46249402391333854778e-02, -3.27885410759859649565e-02, 1.79706750811820387126e-02, -1.03142241298341437450e-02, 6.10053870246291332635e-03, -3.68452016781138256760e-03, 2.25964780900612472250e-03, -1.40346469989232843813e-03, 8.81081882437654011382e-04, -5.38595305356740546715e-04, 3.15632070903625950361e-04, -3.12754168375120860518e-04, 3.35529192635519073543e-04 });
        private static array<double> _lgamU = new array<double>(new double[] { -7.72156649015328655494e-02, 6.32827064025093366517e-01, 1.45492250137234768737e+00, 9.77717527963372745603e-01, 2.28963728064692451092e-01, 1.33810918536787660377e-02 });
        private static array<double> _lgamV = new array<double>(new double[] { 1.0, 2.45597793713041134822e+00, 2.12848976379893395361e+00, 7.69285150456672783825e-01, 1.04222645593369134254e-01, 3.21709242282423911810e-03 });
        private static array<double> _lgamW = new array<double>(new double[] { 4.18938533204672725052e-01, 8.33333333333329678849e-02, -2.77777777728775536470e-03, 7.93650558643019558500e-04, -5.95187557450339963135e-04, 8.36339918996282139126e-04, -1.63092934096575273989e-03 });

        // Lgamma returns the natural logarithm and sign (-1 or +1) of Gamma(x).
        //
        // Special cases are:
        //    Lgamma(+Inf) = +Inf
        //    Lgamma(0) = +Inf
        //    Lgamma(-integer) = +Inf
        //    Lgamma(-Inf) = -Inf
        //    Lgamma(NaN) = NaN
        public static (double, long) Lgamma(double x)
        {
            const float Ymin = 1.461632144968362245F;
            const long Two52 = 1L << (int)(52L); // 0x4330000000000000 ~4.5036e+15
            const long Two53 = 1L << (int)(53L); // 0x4340000000000000 ~9.0072e+15
            const long Two58 = 1L << (int)(58L); // 0x4390000000000000 ~2.8823e+17
            const float Tiny = 1.0F / (1L << (int)(70L)); // 0x3b90000000000000 ~8.47033e-22
            const float Tc = 1.46163214496836224576e+00F; // 0x3FF762D86356BE3F
            const float Tf = -1.21486290535849611461e-01F; // 0xBFBF19B9BCC38A42
            // Tt = -(tail of Tf)
            const float Tt = -3.63867699703950536541e-18F; // 0xBC50C7CAA48A971F 
            // special cases
            sign = 1L;

            if (IsNaN(x)) 
                lgamma = x;
                return;
            else if (IsInf(x, 0L)) 
                lgamma = x;
                return;
            else if (x == 0L) 
                lgamma = Inf(1L);
                return;
                        var neg = false;
            if (x < 0L)
            {
                x = -x;
                neg = true;
            }
            if (x < Tiny)
            { // if |x| < 2**-70, return -log(|x|)
                if (neg)
                {
                    sign = -1L;
                }
                lgamma = -Log(x);
                return;
            }
            double nadj = default;
            if (neg)
            {
                if (x >= Two52)
                { // |x| >= 2**52, must be -integer
                    lgamma = Inf(1L);
                    return;
                }
                var t = sinPi(x);
                if (t == 0L)
                {
                    lgamma = Inf(1L); // -integer
                    return;
                }
                nadj = Log(Pi / Abs(t * x));
                if (t < 0L)
                {
                    sign = -1L;
                }
            }

            if (x == 1L || x == 2L) // purge off 1 and 2
                lgamma = 0L;
                return;
            else if (x < 2L) // use lgamma(x) = lgamma(x+1) - log(x)
                double y = default;
                long i = default;
                if (x <= 0.9F)
                {
                    lgamma = -Log(x);

                    if (x >= (Ymin - 1L + 0.27F)) // 0.7316 <= x <=  0.9
                        y = 1L - x;
                        i = 0L;
                    else if (x >= (Ymin - 1L - 0.27F)) // 0.2316 <= x < 0.7316
                        y = x - (Tc - 1L);
                        i = 1L;
                    else // 0 < x < 0.2316
                        y = x;
                        i = 2L;
                                    }
                else
                {
                    lgamma = 0L;

                    if (x >= (Ymin + 0.27F)) // 1.7316 <= x < 2
                        y = 2L - x;
                        i = 0L;
                    else if (x >= (Ymin - 0.27F)) // 1.2316 <= x < 1.7316
                        y = x - Tc;
                        i = 1L;
                    else // 0.9 < x < 1.2316
                        y = x - 1L;
                        i = 2L;
                                    }
                switch (i)
                {
                    case 0L: 
                        var z = y * y;
                        var p1 = _lgamA[0L] + z * (_lgamA[2L] + z * (_lgamA[4L] + z * (_lgamA[6L] + z * (_lgamA[8L] + z * _lgamA[10L]))));
                        var p2 = z * (_lgamA[1L] + z * (+_lgamA[3L] + z * (_lgamA[5L] + z * (_lgamA[7L] + z * (_lgamA[9L] + z * _lgamA[11L])))));
                        var p = y * p1 + p2;
                        lgamma += (p - 0.5F * y);
                        break;
                    case 1L: 
                        z = y * y;
                        var w = z * y;
                        p1 = _lgamT[0L] + w * (_lgamT[3L] + w * (_lgamT[6L] + w * (_lgamT[9L] + w * _lgamT[12L]))); // parallel comp
                        p2 = _lgamT[1L] + w * (_lgamT[4L] + w * (_lgamT[7L] + w * (_lgamT[10L] + w * _lgamT[13L])));
                        var p3 = _lgamT[2L] + w * (_lgamT[5L] + w * (_lgamT[8L] + w * (_lgamT[11L] + w * _lgamT[14L])));
                        p = z * p1 - (Tt - w * (p2 + y * p3));
                        lgamma += (Tf + p);
                        break;
                    case 2L: 
                        p1 = y * (_lgamU[0L] + y * (_lgamU[1L] + y * (_lgamU[2L] + y * (_lgamU[3L] + y * (_lgamU[4L] + y * _lgamU[5L])))));
                        p2 = 1L + y * (_lgamV[1L] + y * (_lgamV[2L] + y * (_lgamV[3L] + y * (_lgamV[4L] + y * _lgamV[5L]))));
                        lgamma += (-0.5F * y + p1 / p2);
                        break;
                }
            else if (x < 8L) // 2 <= x < 8
                i = int(x);
                y = x - float64(i);
                p = y * (_lgamS[0L] + y * (_lgamS[1L] + y * (_lgamS[2L] + y * (_lgamS[3L] + y * (_lgamS[4L] + y * (_lgamS[5L] + y * _lgamS[6L]))))));
                long q = 1L + y * (_lgamR[1L] + y * (_lgamR[2L] + y * (_lgamR[3L] + y * (_lgamR[4L] + y * (_lgamR[5L] + y * _lgamR[6L])))));
                lgamma = 0.5F * y + p / q;
                z = 1.0F; // Lgamma(1+s) = Log(s) + Lgamma(s)

                if (i == 7L)
                {
                    z *= (y + 6L);
                    fallthrough = true;
                }
                if (fallthrough || i == 6L)
                {
                    z *= (y + 5L);
                    fallthrough = true;
                }
                if (fallthrough || i == 5L)
                {
                    z *= (y + 4L);
                    fallthrough = true;
                }
                if (fallthrough || i == 4L)
                {
                    z *= (y + 3L);
                    fallthrough = true;
                }
                if (fallthrough || i == 3L)
                {
                    z *= (y + 2L);
                    lgamma += Log(z);
                    goto __switch_break0;
                }

                __switch_break0:;
            else if (x < Two58) // 8 <= x < 2**58
                t = Log(x);
                z = 1L / x;
                y = z * z;
                w = _lgamW[0L] + z * (_lgamW[1L] + y * (_lgamW[2L] + y * (_lgamW[3L] + y * (_lgamW[4L] + y * (_lgamW[5L] + y * _lgamW[6L])))));
                lgamma = (x - 0.5F) * (t - 1L) + w;
            else // 2**58 <= x <= Inf
                lgamma = x * (Log(x) - 1L);
                        if (neg)
            {
                lgamma = nadj - lgamma;
            }
            return;
        }

        // sinPi(x) is a helper function for negative x
        private static double sinPi(double x)
        {
            const long Two52 = 1L << (int)(52L); // 0x4330000000000000 ~4.5036e+15
            const long Two53 = 1L << (int)(53L); // 0x4340000000000000 ~9.0072e+15
            if (x < 0.25F)
            {
                return -Sin(Pi * x);
            } 

            // argument reduction
            var z = Floor(x);
            long n = default;
            if (z != x)
            { // inexact
                x = Mod(x, 2L);
                n = int(x * 4L);
            }
            else
            {
                if (x >= Two53)
                { // x must be even
                    x = 0L;
                    n = 0L;
                }
                else
                {
                    if (x < Two52)
                    {
                        z = x + Two52; // exact
                    }
                    n = int(1L & Float64bits(z));
                    x = float64(n);
                    n <<= 2L;
                }
            }
            switch (n)
            {
                case 0L: 
                    x = Sin(Pi * x);
                    break;
                case 1L: 

                case 2L: 
                    x = Cos(Pi * (0.5F - x));
                    break;
                case 3L: 

                case 4L: 
                    x = Sin(Pi * (1L - x));
                    break;
                case 5L: 

                case 6L: 
                    x = -Cos(Pi * (x - 1.5F));
                    break;
                default: 
                    x = Sin(Pi * (x - 2L));
                    break;
            }
            return -x;
        }
    }
}
