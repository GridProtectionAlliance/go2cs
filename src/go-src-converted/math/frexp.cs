// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:41 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\frexp.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Frexp breaks f into a normalized fraction
        // and an integral power of two.
        // It returns frac and exp satisfying f == frac × 2**exp,
        // with the absolute value of frac in the interval [½, 1).
        //
        // Special cases are:
        //    Frexp(±0) = ±0, 0
        //    Frexp(±Inf) = ±Inf, 0
        //    Frexp(NaN) = NaN, 0
        public static (double, long) Frexp(double f)
;

        private static (double, long) frexp(double f)
        {
            double frac = default;
            long exp = default;
 
            // special cases

            if (f == 0L) 
                return (f, 0L); // correctly return -0
            else if (IsInf(f, 0L) || IsNaN(f)) 
                return (f, 0L);
                        f, exp = normalize(f);
            var x = Float64bits(f);
            exp += int((x >> (int)(shift)) & mask) - bias + 1L;
            x &= mask << (int)(shift);
            x |= (-1L + bias) << (int)(shift);
            frac = Float64frombits(x);
            return ;

        }
    }
}
