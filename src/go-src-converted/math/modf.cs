// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:45 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\modf.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Modf returns integer and fractional floating-point numbers
        // that sum to f. Both values have the same sign as f.
        //
        // Special cases are:
        //    Modf(±Inf) = ±Inf, NaN
        //    Modf(NaN) = NaN, NaN
        public static (double, double) Modf(double f)
;

        private static (double, double) modf(double f)
        {
            double @int = default;
            double frac = default;

            if (f < 1L)
            {>>MARKER:FUNCTION_Modf_BLOCK_PREFIX<<

                if (f < 0L) 
                    int, frac = Modf(-f);
                    return (-int, -frac);
                else if (f == 0L) 
                    return (f, f); // Return -0, -0 when f == -0
                                return (0L, f);

            }

            var x = Float64bits(f);
            var e = uint(x >> (int)(shift)) & mask - bias; 

            // Keep the top 12+e bits, the integer part; clear the rest.
            if (e < 64L - 12L)
            {
                x &= 1L << (int)((64L - 12L - e)) - 1L;
            }

            int = Float64frombits(x);
            frac = f - int;
            return ;

        }
    }
}
