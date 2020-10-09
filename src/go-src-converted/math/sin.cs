// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:46 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\sin.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Floating-point sine and cosine.
        */

        // The original C code, the long comment, and the constants
        // below were from http://netlib.sandia.gov/cephes/cmath/sin.c,
        // available from http://www.netlib.org/cephes/cmath.tgz.
        // The go code is a simplified version of the original C.
        //
        //      sin.c
        //
        //      Circular sine
        //
        // SYNOPSIS:
        //
        // double x, y, sin();
        // y = sin( x );
        //
        // DESCRIPTION:
        //
        // Range reduction is into intervals of pi/4.  The reduction error is nearly
        // eliminated by contriving an extended precision modular arithmetic.
        //
        // Two polynomial approximating functions are employed.
        // Between 0 and pi/4 the sine is approximated by
        //      x  +  x**3 P(x**2).
        // Between pi/4 and pi/2 the cosine is represented as
        //      1  -  x**2 Q(x**2).
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain      # trials      peak         rms
        //    DEC       0, 10       150000       3.0e-17     7.8e-18
        //    IEEE -1.07e9,+1.07e9  130000       2.1e-16     5.4e-17
        //
        // Partial loss of accuracy begins to occur at x = 2**30 = 1.074e9.  The loss
        // is not gradual, but jumps suddenly to about 1 part in 10e7.  Results may
        // be meaningless for x > 2**49 = 5.6e14.
        //
        //      cos.c
        //
        //      Circular cosine
        //
        // SYNOPSIS:
        //
        // double x, y, cos();
        // y = cos( x );
        //
        // DESCRIPTION:
        //
        // Range reduction is into intervals of pi/4.  The reduction error is nearly
        // eliminated by contriving an extended precision modular arithmetic.
        //
        // Two polynomial approximating functions are employed.
        // Between 0 and pi/4 the cosine is approximated by
        //      1  -  x**2 Q(x**2).
        // Between pi/4 and pi/2 the sine is represented as
        //      x  +  x**3 P(x**2).
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain      # trials      peak         rms
        //    IEEE -1.07e9,+1.07e9  130000       2.1e-16     5.4e-17
        //    DEC        0,+1.07e9   17000       3.0e-17     7.2e-18
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

        // sin coefficients
        private static array<double> _sin = new array<double>(new double[] { 1.58962301576546568060e-10, -2.50507477628578072866e-8, 2.75573136213857245213e-6, -1.98412698295895385996e-4, 8.33333333332211858878e-3, -1.66666666666666307295e-1 });

        // cos coefficients
        private static array<double> _cos = new array<double>(new double[] { -1.13585365213876817300e-11, 2.08757008419747316778e-9, -2.75573141792967388112e-7, 2.48015872888517045348e-5, -1.38888888888730564116e-3, 4.16666666666665929218e-2 });

        // Cos returns the cosine of the radian argument x.
        //
        // Special cases are:
        //    Cos(±Inf) = NaN
        //    Cos(NaN) = NaN
        public static double Cos(double x)
;

        private static double cos(double x)
        {
            const float PI4A = (float)7.85398125648498535156e-1F; // 0x3fe921fb40000000, Pi/4 split into three parts
            const float PI4B = (float)3.77489470793079817668e-8F; // 0x3e64442d00000000,
            const float PI4C = (float)2.69515142907905952645e-15F; // 0x3ce8469898cc5170, 
            // special cases

            if (IsNaN(x) || IsInf(x, 0L)) 
                return NaN();
            // make argument positive
            var sign = false;
            x = Abs(x);

            ulong j = default;
            double y = default;            double z = default;

            if (x >= reduceThreshold)
            {>>MARKER:FUNCTION_Cos_BLOCK_PREFIX<<
                j, z = trigReduce(x);
            }
            else
            {
                j = uint64(x * (4L / Pi)); // integer part of x/(Pi/4), as integer for tests on the phase angle
                y = float64(j); // integer part of x/(Pi/4), as float

                // map zeros to origin
                if (j & 1L == 1L)
                {
                    j++;
                    y++;
                }

                j &= 7L; // octant modulo 2Pi radians (360 degrees)
                z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
            }

            if (j > 3L)
            {
                j -= 4L;
                sign = !sign;
            }

            if (j > 1L)
            {
                sign = !sign;
            }

            var zz = z * z;
            if (j == 1L || j == 2L)
            {
                y = z + z * zz * ((((((_sin[0L] * zz) + _sin[1L]) * zz + _sin[2L]) * zz + _sin[3L]) * zz + _sin[4L]) * zz + _sin[5L]);
            }
            else
            {
                y = 1.0F - 0.5F * zz + zz * zz * ((((((_cos[0L] * zz) + _cos[1L]) * zz + _cos[2L]) * zz + _cos[3L]) * zz + _cos[4L]) * zz + _cos[5L]);
            }

            if (sign)
            {
                y = -y;
            }

            return y;

        }

        // Sin returns the sine of the radian argument x.
        //
        // Special cases are:
        //    Sin(±0) = ±0
        //    Sin(±Inf) = NaN
        //    Sin(NaN) = NaN
        public static double Sin(double x)
;

        private static double sin(double x)
        {
            const float PI4A = (float)7.85398125648498535156e-1F; // 0x3fe921fb40000000, Pi/4 split into three parts
            const float PI4B = (float)3.77489470793079817668e-8F; // 0x3e64442d00000000,
            const float PI4C = (float)2.69515142907905952645e-15F; // 0x3ce8469898cc5170, 
            // special cases

            if (x == 0L || IsNaN(x)) 
                return x; // return ±0 || NaN()
            else if (IsInf(x, 0L)) 
                return NaN();
            // make argument positive but save the sign
            var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Sin_BLOCK_PREFIX<<
                x = -x;
                sign = true;
            }

            ulong j = default;
            double y = default;            double z = default;

            if (x >= reduceThreshold)
            {
                j, z = trigReduce(x);
            }
            else
            {
                j = uint64(x * (4L / Pi)); // integer part of x/(Pi/4), as integer for tests on the phase angle
                y = float64(j); // integer part of x/(Pi/4), as float

                // map zeros to origin
                if (j & 1L == 1L)
                {
                    j++;
                    y++;
                }

                j &= 7L; // octant modulo 2Pi radians (360 degrees)
                z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
            } 
            // reflect in x axis
            if (j > 3L)
            {
                sign = !sign;
                j -= 4L;
            }

            var zz = z * z;
            if (j == 1L || j == 2L)
            {
                y = 1.0F - 0.5F * zz + zz * zz * ((((((_cos[0L] * zz) + _cos[1L]) * zz + _cos[2L]) * zz + _cos[3L]) * zz + _cos[4L]) * zz + _cos[5L]);
            }
            else
            {
                y = z + z * zz * ((((((_sin[0L] * zz) + _sin[1L]) * zz + _sin[2L]) * zz + _sin[3L]) * zz + _sin[4L]) * zz + _sin[5L]);
            }

            if (sign)
            {
                y = -y;
            }

            return y;

        }
    }
}
