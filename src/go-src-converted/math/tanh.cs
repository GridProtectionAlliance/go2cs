// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:59 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\tanh.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // The original C code, the long comment, and the constants
        // below were from http://netlib.sandia.gov/cephes/cmath/sin.c,
        // available from http://www.netlib.org/cephes/cmath.tgz.
        // The go code is a simplified version of the original C.
        //      tanh.c
        //
        //      Hyperbolic tangent
        //
        // SYNOPSIS:
        //
        // double x, y, tanh();
        //
        // y = tanh( x );
        //
        // DESCRIPTION:
        //
        // Returns hyperbolic tangent of argument in the range MINLOG to MAXLOG.
        //      MAXLOG = 8.8029691931113054295988e+01 = log(2**127)
        //      MINLOG = -8.872283911167299960540e+01 = log(2**-128)
        //
        // A rational function is used for |x| < 0.625.  The form
        // x + x**3 P(x)/Q(x) of Cody & Waite is employed.
        // Otherwise,
        //      tanh(x) = sinh(x)/cosh(x) = 1  -  2/(exp(2x) + 1).
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain     # trials      peak         rms
        //    IEEE      -2,2        30000       2.5e-16     5.8e-17
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
        //
        private static array<double> tanhP = new array<double>(new double[] { -9.64399179425052238628E-1, -9.92877231001918586564E1, -1.61468768441708447952E3 });
        private static array<double> tanhQ = new array<double>(new double[] { 1.12811678491632931402E2, 2.23548839060100448583E3, 4.84406305325125486048E3 });

        // Tanh returns the hyperbolic tangent of x.
        //
        // Special cases are:
        //    Tanh(±0) = ±0
        //    Tanh(±Inf) = ±1
        //    Tanh(NaN) = NaN
        public static double Tanh(double x)
;

        private static double tanh(double x)
        {
            const float MAXLOG = 8.8029691931113054295988e+01F; // log(2**127)
 // log(2**127)
            var z = Abs(x);

            if (z > 0.5F * MAXLOG) 
                if (x < 0L)
                {>>MARKER:FUNCTION_Tanh_BLOCK_PREFIX<<
                    return -1L;
                }
                return 1L;
            else if (z >= 0.625F) 
                var s = Exp(2L * z);
                z = 1L - 2L / (s + 1L);
                if (x < 0L)
                {
                    z = -z;
                }
            else 
                if (x == 0L)
                {
                    return x;
                }
                s = x * x;
                z = x + x * s * ((tanhP[0L] * s + tanhP[1L]) * s + tanhP[2L]) / (((s + tanhQ[0L]) * s + tanhQ[1L]) * s + tanhQ[2L]);
                        return z;
        }
    }
}
