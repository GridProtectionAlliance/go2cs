// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:48 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\floor.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Floor returns the greatest integer value less than or equal to x.
        //
        // Special cases are:
        //    Floor(±0) = ±0
        //    Floor(±Inf) = ±Inf
        //    Floor(NaN) = NaN
        public static double Floor(double x)
;

        private static double floor(double x)
        {
            if (x == 0L || IsNaN(x) || IsInf(x, 0L))
            {>>MARKER:FUNCTION_Floor_BLOCK_PREFIX<<
                return x;
            }
            if (x < 0L)
            {
                var (d, fract) = Modf(-x);
                if (fract != 0.0F)
                {
                    d = d + 1L;
                }
                return -d;
            }
            var (d, _) = Modf(x);
            return d;
        }

        // Ceil returns the least integer value greater than or equal to x.
        //
        // Special cases are:
        //    Ceil(±0) = ±0
        //    Ceil(±Inf) = ±Inf
        //    Ceil(NaN) = NaN
        public static double Ceil(double x)
;

        private static double ceil(double x)
        {
            return -Floor(-x);
        }

        // Trunc returns the integer value of x.
        //
        // Special cases are:
        //    Trunc(±0) = ±0
        //    Trunc(±Inf) = ±Inf
        //    Trunc(NaN) = NaN
        public static double Trunc(double x)
;

        private static double trunc(double x)
        {
            if (x == 0L || IsNaN(x) || IsInf(x, 0L))
            {>>MARKER:FUNCTION_Trunc_BLOCK_PREFIX<<
                return x;
            }
            var (d, _) = Modf(x);
            return d;
        }

        // Round returns the nearest integer, rounding half away from zero.
        //
        // Special cases are:
        //    Round(±0) = ±0
        //    Round(±Inf) = ±Inf
        //    Round(NaN) = NaN
        public static double Round(double x)
        { 
            // Round is a faster implementation of:
            //
            // func Round(x float64) float64 {
            //   t := Trunc(x)
            //   if Abs(x-t) >= 0.5 {
            //     return t + Copysign(1, x)
            //   }
            //   return t
            // }
            var bits = Float64bits(x);
            var e = uint(bits >> (int)(shift)) & mask;
            if (e < bias)
            {>>MARKER:FUNCTION_Ceil_BLOCK_PREFIX<< 
                // Round abs(x) < 1 including denormals.
                bits &= signMask; // +-0
                if (e == bias - 1L)
                {
                    bits |= uvone; // +-1
                }
            }
            else if (e < bias + shift)
            { 
                // Round any abs(x) >= 1 containing a fractional component [0,1).
                //
                // Numbers with larger exponents are returned unchanged since they
                // must be either an integer, infinity, or NaN.
                const long half = 1L << (int)((shift - 1L));

                e -= bias;
                bits += half >> (int)(e);
                bits &= fracMask >> (int)(e);
            }
            return Float64frombits(bits);
        }

        // RoundToEven returns the nearest integer, rounding ties to even.
        //
        // Special cases are:
        //    RoundToEven(±0) = ±0
        //    RoundToEven(±Inf) = ±Inf
        //    RoundToEven(NaN) = NaN
        public static double RoundToEven(double x)
        { 
            // RoundToEven is a faster implementation of:
            //
            // func RoundToEven(x float64) float64 {
            //   t := math.Trunc(x)
            //   odd := math.Remainder(t, 2) != 0
            //   if d := math.Abs(x - t); d > 0.5 || (d == 0.5 && odd) {
            //     return t + math.Copysign(1, x)
            //   }
            //   return t
            // }
            var bits = Float64bits(x);
            var e = uint(bits >> (int)(shift)) & mask;
            if (e >= bias)
            { 
                // Round abs(x) >= 1.
                // - Large numbers without fractional components, infinity, and NaN are unchanged.
                // - Add 0.499.. or 0.5 before truncating depending on whether the truncated
                //   number is even or odd (respectively).
                const long halfMinusULP = (1L << (int)((shift - 1L))) - 1L;

                e -= bias;
                bits += (halfMinusULP + (bits >> (int)((shift - e))) & 1L) >> (int)(e);
                bits &= fracMask >> (int)(e);
            }
            else if (e == bias - 1L && bits & fracMask != 0L)
            { 
                // Round 0.5 < abs(x) < 1.
                bits = bits & signMask | uvone; // +-1
            }
            else
            { 
                // Round abs(x) <= 0.5 including denormals.
                bits &= signMask; // +-0
            }
            return Float64frombits(bits);
        }
    }
}
