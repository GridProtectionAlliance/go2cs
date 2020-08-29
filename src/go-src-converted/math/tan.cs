// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:59 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\tan.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Floating-point tangent.
        */

        // The original C code, the long comment, and the constants
        // below were from http://netlib.sandia.gov/cephes/cmath/sin.c,
        // available from http://www.netlib.org/cephes/cmath.tgz.
        // The go code is a simplified version of the original C.
        //
        //      tan.c
        //
        //      Circular tangent
        //
        // SYNOPSIS:
        //
        // double x, y, tan();
        // y = tan( x );
        //
        // DESCRIPTION:
        //
        // Returns the circular tangent of the radian argument x.
        //
        // Range reduction is modulo pi/4.  A rational function
        //       x + x**3 P(x**2)/Q(x**2)
        // is employed in the basic interval [0, pi/4].
        //
        // ACCURACY:
        //                      Relative error:
        // arithmetic   domain     # trials      peak         rms
        //    DEC      +-1.07e9      44000      4.1e-17     1.0e-17
        //    IEEE     +-1.07e9      30000      2.9e-16     8.1e-17
        //
        // Partial loss of accuracy begins to occur at x = 2**30 = 1.074e9.  The loss
        // is not gradual, but jumps suddenly to about 1 part in 10e7.  Results may
        // be meaningless for x > 2**49 = 5.6e14.
        // [Accuracy loss statement from sin.go comments.]
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

        // tan coefficients
        private static array<double> _tanP = new array<double>(new double[] { -1.30936939181383777646E4, 1.15351664838587416140E6, -1.79565251976484877988E7 });
        private static array<double> _tanQ = new array<double>(new double[] { 1.00000000000000000000E0, 1.36812963470692954678E4, -1.32089234440210967447E6, 2.50083801823357915839E7, -5.38695755929454629881E7 });

        // Tan returns the tangent of the radian argument x.
        //
        // Special cases are:
        //    Tan(±0) = ±0
        //    Tan(±Inf) = NaN
        //    Tan(NaN) = NaN
        public static double Tan(double x)
;

        private static double tan(double x)
        {
            const float PI4A = 7.85398125648498535156E-1F; // 0x3fe921fb40000000, Pi/4 split into three parts
            const float PI4B = 3.77489470793079817668E-8F; // 0x3e64442d00000000,
            const float PI4C = 2.69515142907905952645E-15F; // 0x3ce8469898cc5170,
            const float M4PI = 1.273239544735162542821171882678754627704620361328125F; // 4/pi 
            // special cases

            if (x == 0L || IsNaN(x)) 
                return x; // return ±0 || NaN()
            else if (IsInf(x, 0L)) 
                return NaN();
            // make argument positive but save the sign
            var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Tan_BLOCK_PREFIX<<
                x = -x;
                sign = true;
            }
            var j = int64(x * M4PI); // integer part of x/(Pi/4), as integer for tests on the phase angle
            var y = float64(j); // integer part of x/(Pi/4), as float

            /* map zeros and singularities to origin */
            if (j & 1L == 1L)
            {
                j++;
                y++;
            }
            var z = ((x - y * PI4A) - y * PI4B) - y * PI4C;
            var zz = z * z;

            if (zz > 1e-14F)
            {
                y = z + z * (zz * (((_tanP[0L] * zz) + _tanP[1L]) * zz + _tanP[2L]) / ((((zz + _tanQ[1L]) * zz + _tanQ[2L]) * zz + _tanQ[3L]) * zz + _tanQ[4L]));
            }
            else
            {
                y = z;
            }
            if (j & 2L == 2L)
            {
                y = -1L / y;
            }
            if (sign)
            {
                y = -y;
            }
            return y;
        }
    }
}
