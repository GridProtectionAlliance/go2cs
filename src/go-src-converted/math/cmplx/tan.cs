// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2020 August 29 08:45:02 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Go\src\math\cmplx\tan.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class cmplx_package
    {
        // The original C code, the long comment, and the constants
        // below are from http://netlib.sandia.gov/cephes/c9x-complex/clog.c.
        // The go code is a simplified version of the original C.
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

        // Complex circular tangent
        //
        // DESCRIPTION:
        //
        // If
        //     z = x + iy,
        //
        // then
        //
        //           sin 2x  +  i sinh 2y
        //     w  =  --------------------.
        //            cos 2x  +  cosh 2y
        //
        // On the real axis the denominator is zero at odd multiples
        // of PI/2.  The denominator is evaluated by its Taylor
        // series near these points.
        //
        // ctan(z) = -i ctanh(iz).
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain     # trials      peak         rms
        //    DEC       -10,+10      5200       7.1e-17     1.6e-17
        //    IEEE      -10,+10     30000       7.2e-16     1.2e-16
        // Also tested by ctan * ccot = 1 and catan(ctan(z))  =  z.

        // Tan returns the tangent of x.
        public static System.Numerics.Complex128 Tan(System.Numerics.Complex128 x)
        {
            var d = math.Cos(2L * real(x)) + math.Cosh(2L * imag(x));
            if (math.Abs(d) < 0.25F)
            {
                d = tanSeries(x);
            }
            if (d == 0L)
            {
                return Inf();
            }
            return complex(math.Sin(2L * real(x)) / d, math.Sinh(2L * imag(x)) / d);
        }

        // Complex hyperbolic tangent
        //
        // DESCRIPTION:
        //
        // tanh z = (sinh 2x  +  i sin 2y) / (cosh 2x + cos 2y) .
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain     # trials      peak         rms
        //    IEEE      -10,+10     30000       1.7e-14     2.4e-16

        // Tanh returns the hyperbolic tangent of x.
        public static System.Numerics.Complex128 Tanh(System.Numerics.Complex128 x)
        {
            var d = math.Cosh(2L * real(x)) + math.Cos(2L * imag(x));
            if (d == 0L)
            {
                return Inf();
            }
            return complex(math.Sinh(2L * real(x)) / d, math.Sin(2L * imag(x)) / d);
        }

        // Program to subtract nearest integer multiple of PI
        private static double reducePi(double x)
        {
 
            // extended precision value of PI:
            const float DP1 = 3.14159265160560607910E0F; // ?? 0x400921fb54000000
            const float DP2 = 1.98418714791870343106E-9F; // ?? 0x3e210b4610000000
            const float DP3 = 1.14423774522196636802E-17F; // ?? 0x3c6a62633145c06e
            var t = x / math.Pi;
            if (t >= 0L)
            {
                t += 0.5F;
            }
            else
            {
                t -= 0.5F;
            }
            t = float64(int64(t)); // int64(t) = the multiple
            return ((x - t * DP1) - t * DP2) - t * DP3;
        }

        // Taylor series expansion for cosh(2y) - cos(2x)
        private static double tanSeries(System.Numerics.Complex128 z)
        {
            const float MACHEP = 1.0F / (1L << (int)(53L));

            var x = math.Abs(2L * real(z));
            var y = math.Abs(2L * imag(z));
            x = reducePi(x);
            x = x * x;
            y = y * y;
            float x2 = 1.0F;
            float y2 = 1.0F;
            float f = 1.0F;
            float rn = 0.0F;
            float d = 0.0F;
            while (true)
            {
                rn++;
                f *= rn;
                rn++;
                f *= rn;
                x2 *= x;
                y2 *= y;
                var t = y2 + x2;
                t /= f;
                d += t;

                rn++;
                f *= rn;
                rn++;
                f *= rn;
                x2 *= x;
                y2 *= y;
                t = y2 - x2;
                t /= f;
                d += t;
                if (!(math.Abs(t / d) > MACHEP))
                { 
                    // Caution: Use ! and > instead of <= for correct behavior if t/d is NaN.
                    // See issue 17577.
                    break;
                }
            }

            return d;
        }

        // Complex circular cotangent
        //
        // DESCRIPTION:
        //
        // If
        //     z = x + iy,
        //
        // then
        //
        //           sin 2x  -  i sinh 2y
        //     w  =  --------------------.
        //            cosh 2y  -  cos 2x
        //
        // On the real axis, the denominator has zeros at even
        // multiples of PI/2.  Near these points it is evaluated
        // by a Taylor series.
        //
        // ACCURACY:
        //
        //                      Relative error:
        // arithmetic   domain     # trials      peak         rms
        //    DEC       -10,+10      3000       6.5e-17     1.6e-17
        //    IEEE      -10,+10     30000       9.2e-16     1.2e-16
        // Also tested by ctan * ccot = 1 + i0.

        // Cot returns the cotangent of x.
        public static System.Numerics.Complex128 Cot(System.Numerics.Complex128 x)
        {
            var d = math.Cosh(2L * imag(x)) - math.Cos(2L * real(x));
            if (math.Abs(d) < 0.25F)
            {
                d = tanSeries(x);
            }
            if (d == 0L)
            {
                return Inf();
            }
            return complex(math.Sin(2L * real(x)) / d, -math.Sinh(2L * imag(x)) / d);
        }
    }
}}
