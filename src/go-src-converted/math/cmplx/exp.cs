// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2022 March 06 22:31:13 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Program Files\Go\src\math\cmplx\exp.go
using math = go.math_package;

namespace go.math;

public static partial class cmplx_package {

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

    // Complex exponential function
    //
    // DESCRIPTION:
    //
    // Returns the complex exponential of the complex argument z.
    //
    // If
    //     z = x + iy,
    //     r = exp(x),
    // then
    //     w = r cos y + i r sin y.
    //
    // ACCURACY:
    //
    //                      Relative error:
    // arithmetic   domain     # trials      peak         rms
    //    DEC       -10,+10      8700       3.7e-17     1.1e-17
    //    IEEE      -10,+10     30000       3.0e-16     8.7e-17

    // Exp returns e**x, the base-e exponential of x.
public static System.Numerics.Complex128 Exp(System.Numerics.Complex128 x) {
    {
        var re = real(x);
        var im = imag(x);


        if (math.IsInf(re, 0)) 

            if (re > 0 && im == 0) 
                return x;
            else if (math.IsInf(im, 0) || math.IsNaN(im)) 
                if (re < 0) {
                    return complex(0, math.Copysign(0, im));
                }
                else
 {
                    return complex(math.Inf(1.0F), math.NaN());
                }
                    else if (math.IsNaN(re)) 
            if (im == 0) {
                return complex(math.NaN(), im);
            }

    }
    var r = math.Exp(real(x));
    var (s, c) = math.Sincos(imag(x));
    return complex(r * c, r * s);

}

} // end cmplx_package
