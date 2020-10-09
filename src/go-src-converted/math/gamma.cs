// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:42 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\gamma.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The original C code, the long comment, and the constants
        // below are from http://netlib.sandia.gov/cephes/cprob/gamma.c.
        // The go code is a simplified version of the original C.
        //
        //      tgamma.c
        //
        //      Gamma function
        //
        // SYNOPSIS:
        //
        // double x, y, tgamma();
        // extern int signgam;
        //
        // y = tgamma( x );
        //
        // DESCRIPTION:
        //
        // Returns gamma function of the argument. The result is
        // correctly signed, and the sign (+1 or -1) is also
        // returned in a global (extern) variable named signgam.
        // This variable is also filled in by the logarithmic gamma
        // function lgamma().
        //
        // Arguments |x| <= 34 are reduced by recurrence and the function
        // approximated by a rational function of degree 6/7 in the
        // interval (2,3).  Large arguments are handled by Stirling's
        // formula. Large negative arguments are made positive using
        // a reflection formula.
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain     # trials      peak         rms
        //    DEC      -34, 34      10000       1.3e-16     2.5e-17
        //    IEEE    -170,-33      20000       2.3e-15     3.3e-16
        //    IEEE     -33,  33     20000       9.4e-16     2.2e-16
        //    IEEE      33, 171.6   20000       2.3e-15     3.2e-16
        //
        // Error for arguments outside the test range will be larger
        // owing to error amplification by the exponential function.
        //
        // Cephes Math Library Release 2.8:  June, 2000
        // Copyright 1984, 1987, 1989, 1992, 2000 by Stephen L. Moshier
        //
        // The readme file at http://netlib.sandia.gov/cephes/ says:
        //    Some software in this archive may be from the book _Methods and
        // Programs for Mathematical Functions_ (Prentice-Hall or Simon & Schuster
        // International, 1989) or from the Cephes Mathematical Library, a
        // commercial product. In either event, it is copyrighted by the author.
        // What you see here may be used freely but it comes with no support or
        // guarantee.
        //
        //   The two known misprints in the book are repaired here in the
        // source listings for the gamma function and the incomplete beta
        // integral.
        //
        //   Stephen L. Moshier
        //   moshier@na-net.ornl.gov
        private static array<double> _gamP = new array<double>(new double[] { 1.60119522476751861407e-04, 1.19135147006586384913e-03, 1.04213797561761569935e-02, 4.76367800457137231464e-02, 2.07448227648435975150e-01, 4.94214826801497100753e-01, 9.99999999999999996796e-01 });
        private static array<double> _gamQ = new array<double>(new double[] { -2.31581873324120129819e-05, 5.39605580493303397842e-04, -4.45641913851797240494e-03, 1.18139785222060435552e-02, 3.58236398605498653373e-02, -2.34591795718243348568e-01, 7.14304917030273074085e-02, 1.00000000000000000320e+00 });
        private static array<double> _gamS = new array<double>(new double[] { 7.87311395793093628397e-04, -2.29549961613378126380e-04, -2.68132617805781232825e-03, 3.47222221605458667310e-03, 8.33333333333482257126e-02 });

        // Gamma function computed by Stirling's formula.
        // The pair of results must be multiplied together to get the actual answer.
        // The multiplication is left to the caller so that, if careful, the caller can avoid
        // infinity for 172 <= x <= 180.
        // The polynomial is valid for 33 <= x <= 172; larger values are only used
        // in reciprocal and produce denormalized floats. The lower precision there
        // masks any imprecision in the polynomial.
        private static (double, double) stirling(double x)
        {
            double _p0 = default;
            double _p0 = default;

            if (x > 200L)
            {
                return (Inf(1L), 1L);
            }

            const float SqrtTwoPi = (float)2.506628274631000502417F;
            const float MaxStirling = (float)143.01608F;

            long w = 1L / x;
            w = 1L + w * ((((_gamS[0L] * w + _gamS[1L]) * w + _gamS[2L]) * w + _gamS[3L]) * w + _gamS[4L]);
            var y1 = Exp(x);
            float y2 = 1.0F;
            if (x > MaxStirling)
            { // avoid Pow() overflow
                var v = Pow(x, 0.5F * x - 0.25F);
                y1 = v;
                y2 = v / y1;

            }
            else
            {
                y1 = Pow(x, x - 0.5F) / y1;
            }

            return (y1, SqrtTwoPi * w * y2);

        }

        // Gamma returns the Gamma function of x.
        //
        // Special cases are:
        //    Gamma(+Inf) = +Inf
        //    Gamma(+0) = +Inf
        //    Gamma(-0) = -Inf
        //    Gamma(x) = NaN for integer x < 0
        //    Gamma(-Inf) = NaN
        //    Gamma(NaN) = NaN
        public static double Gamma(double x)
        {
            const float Euler = (float)0.57721566490153286060651209008240243104215933593992F; // A001620
            // special cases
 // A001620
            // special cases

            if (isNegInt(x) || IsInf(x, -1L) || IsNaN(x)) 
                return NaN();
            else if (IsInf(x, 1L)) 
                return Inf(1L);
            else if (x == 0L) 
                if (Signbit(x))
                {
                    return Inf(-1L);
                }

                return Inf(1L);
                        var q = Abs(x);
            var p = Floor(q);
            if (q > 33L)
            {
                if (x >= 0L)
                {
                    var (y1, y2) = stirling(x);
                    return y1 * y2;
                } 
                // Note: x is negative but (checked above) not a negative integer,
                // so x must be small enough to be in range for conversion to int64.
                // If |x| were >= 2⁶³ it would have to be an integer.
                long signgam = 1L;
                {
                    var ip = int64(p);

                    if (ip & 1L == 0L)
                    {
                        signgam = -1L;
                    }

                }

                var z = q - p;
                if (z > 0.5F)
                {
                    p = p + 1L;
                    z = q - p;
                }

                z = q * Sin(Pi * z);
                if (z == 0L)
                {
                    return Inf(signgam);
                }

                var (sq1, sq2) = stirling(q);
                var absz = Abs(z);
                var d = absz * sq1 * sq2;
                if (IsInf(d, 0L))
                {
                    z = Pi / absz / sq1 / sq2;
                }
                else
                {
                    z = Pi / d;
                }

                return float64(signgam) * z;

            } 

            // Reduce argument
            z = 1.0F;
            while (x >= 3L)
            {
                x = x - 1L;
                z = z * x;
            }

            while (x < 0L)
            {
                if (x > -1e-09F)
                {
                    goto small;
                }

                z = z / x;
                x = x + 1L;

            }

            while (x < 2L)
            {
                if (x < 1e-09F)
                {
                    goto small;
                }

                z = z / x;
                x = x + 1L;

            }


            if (x == 2L)
            {
                return z;
            }

            x = x - 2L;
            p = (((((x * _gamP[0L] + _gamP[1L]) * x + _gamP[2L]) * x + _gamP[3L]) * x + _gamP[4L]) * x + _gamP[5L]) * x + _gamP[6L];
            q = ((((((x * _gamQ[0L] + _gamQ[1L]) * x + _gamQ[2L]) * x + _gamQ[3L]) * x + _gamQ[4L]) * x + _gamQ[5L]) * x + _gamQ[6L]) * x + _gamQ[7L];
            return z * p / q;

small:
            if (x == 0L)
            {
                return Inf(1L);
            }

            return z / ((1L + Euler * x) * x);

        }

        private static bool isNegInt(double x)
        {
            if (x < 0L)
            {
                var (_, xf) = Modf(x);
                return xf == 0L;
            }

            return false;

        }
    }
}
