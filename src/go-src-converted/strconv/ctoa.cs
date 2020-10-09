// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2020 October 09 05:06:29 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\ctoa.go

using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        // FormatComplex converts the complex number c to a string of the
        // form (a+bi) where a and b are the real and imaginary parts,
        // formatted according to the format fmt and precision prec.
        //
        // The format fmt and precision prec have the same meaning as in FormatFloat.
        // It rounds the result assuming that the original was obtained from a complex
        // value of bitSize bits, which must be 64 for complex64 and 128 for complex128.
        public static @string FormatComplex(System.Numerics.Complex128 c, byte fmt, long prec, long bitSize) => func((_, panic, __) =>
        {
            if (bitSize != 64L && bitSize != 128L)
            {
                panic("invalid bitSize");
            }
            bitSize >>= 1L; // complex64 uses float32 internally

            // Check if imaginary part has a sign. If not, add one.
            var im = FormatFloat(imag(c), fmt, prec, bitSize);
            if (im[0L] != '+' && im[0L] != '-')
            {
                im = "+" + im;
            }
            return "(" + FormatFloat(real(c), fmt, prec, bitSize) + im + "i)";

        });
    }
}
