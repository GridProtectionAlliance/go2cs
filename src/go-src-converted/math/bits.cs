// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\bits.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        private static readonly ulong uvnan = (ulong)0x7FF8000000000001UL;
        private static readonly ulong uvinf = (ulong)0x7FF0000000000000UL;
        private static readonly ulong uvneginf = (ulong)0xFFF0000000000000UL;
        private static readonly ulong uvone = (ulong)0x3FF0000000000000UL;
        private static readonly ulong mask = (ulong)0x7FFUL;
        private static readonly long shift = (long)64L - 11L - 1L;
        private static readonly long bias = (long)1023L;
        private static readonly long signMask = (long)1L << (int)(63L);
        private static readonly long fracMask = (long)1L << (int)(shift) - 1L;


        // Inf returns positive infinity if sign >= 0, negative infinity if sign < 0.
        public static double Inf(long sign)
        {
            ulong v = default;
            if (sign >= 0L)
            {
                v = uvinf;
            }
            else
            {
                v = uvneginf;
            }

            return Float64frombits(v);

        }

        // NaN returns an IEEE 754 ``not-a-number'' value.
        public static double NaN()
        {
            return Float64frombits(uvnan);
        }

        // IsNaN reports whether f is an IEEE 754 ``not-a-number'' value.
        public static bool IsNaN(double f)
        {
            bool @is = default;
 
            // IEEE 754 says that only NaNs satisfy f != f.
            // To avoid the floating-point hardware, could use:
            //    x := Float64bits(f);
            //    return uint32(x>>shift)&mask == mask && x != uvinf && x != uvneginf
            return f != f;

        }

        // IsInf reports whether f is an infinity, according to sign.
        // If sign > 0, IsInf reports whether f is positive infinity.
        // If sign < 0, IsInf reports whether f is negative infinity.
        // If sign == 0, IsInf reports whether f is either infinity.
        public static bool IsInf(double f, long sign)
        { 
            // Test for infinity by comparing against maximum float.
            // To avoid the floating-point hardware, could use:
            //    x := Float64bits(f);
            //    return sign >= 0 && x == uvinf || sign <= 0 && x == uvneginf;
            return sign >= 0L && f > MaxFloat64 || sign <= 0L && f < -MaxFloat64;

        }

        // normalize returns a normal number y and exponent exp
        // satisfying x == y × 2**exp. It assumes x is finite and non-zero.
        private static (double, long) normalize(double x)
        {
            double y = default;
            long exp = default;

            const float SmallestNormal = (float)2.2250738585072014e-308F; // 2**-1022
 // 2**-1022
            if (Abs(x) < SmallestNormal)
            {
                return (x * (1L << (int)(52L)), -52L);
            }

            return (x, 0L);

        }
    }
}
