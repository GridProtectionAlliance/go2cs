// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\atan.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Floating-point arctangent.
        */

        // The original C code, the long comment, and the constants below were
        // from http://netlib.sandia.gov/cephes/cmath/atan.c, available from
        // http://www.netlib.org/cephes/cmath.tgz.
        // The go code is a version of the original C.
        //
        // atan.c
        // Inverse circular tangent (arctangent)
        //
        // SYNOPSIS:
        // double x, y, atan();
        // y = atan( x );
        //
        // DESCRIPTION:
        // Returns radian angle between -pi/2 and +pi/2 whose tangent is x.
        //
        // Range reduction is from three intervals into the interval from zero to 0.66.
        // The approximant uses a rational function of degree 4/5 of the form
        // x + x**3 P(x)/Q(x).
        //
        // ACCURACY:
        //                      Relative error:
        // arithmetic   domain    # trials  peak     rms
        //    DEC       -10, 10   50000     2.4e-17  8.3e-18
        //    IEEE      -10, 10   10^6      1.8e-16  5.0e-17
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

        // xatan evaluates a series valid in the range [0, 0.66].
        private static double xatan(double x)
        {
            const float P0 = (float)-8.750608600031904122785e-01F;
            const float P1 = (float)-1.615753718733365076637e+01F;
            const float P2 = (float)-7.500855792314704667340e+01F;
            const float P3 = (float)-1.228866684490136173410e+02F;
            const float P4 = (float)-6.485021904942025371773e+01F;
            const float Q0 = (float)+2.485846490142306297962e+01F;
            const float Q1 = (float)+1.650270098316988542046e+02F;
            const float Q2 = (float)+4.328810604912902668951e+02F;
            const float Q3 = (float)+4.853903996359136964868e+02F;
            const float Q4 = (float)+1.945506571482613964425e+02F;
            var z = x * x;
            z = z * ((((P0 * z + P1) * z + P2) * z + P3) * z + P4) / (((((z + Q0) * z + Q1) * z + Q2) * z + Q3) * z + Q4);
            z = x * z + x;
            return z;
        }

        // satan reduces its argument (known to be positive)
        // to the range [0, 0.66] and calls xatan.
        private static double satan(double x)
        {
            const float Morebits = (float)6.123233995736765886130e-17F; // pi/2 = PIO2 + Morebits
            const float Tan3pio8 = (float)2.41421356237309504880F; // tan(3*pi/8)
            if (x <= 0.66F)
            {
                return xatan(x);
            }

            if (x > Tan3pio8)
            {
                return Pi / 2L - xatan(1L / x) + Morebits;
            }

            return Pi / 4L + xatan((x - 1L) / (x + 1L)) + 0.5F * Morebits;

        }

        // Atan returns the arctangent, in radians, of x.
        //
        // Special cases are:
        //      Atan(±0) = ±0
        //      Atan(±Inf) = ±Pi/2
        public static double Atan(double x)
;

        private static double atan(double x)
        {
            if (x == 0L)
            {>>MARKER:FUNCTION_Atan_BLOCK_PREFIX<<
                return x;
            }

            if (x > 0L)
            {
                return satan(x);
            }

            return -satan(-x);

        }
    }
}
