// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:59 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\unsafe.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class math_package
    {
        // Float32bits returns the IEEE 754 binary representation of f.
        public static uint Float32bits(float f)
        {
            return @unsafe.Pointer(ref f).Value;
        }

        // Float32frombits returns the floating point number corresponding
        // to the IEEE 754 binary representation b.
        public static float Float32frombits(uint b)
        {
            return @unsafe.Pointer(ref b).Value;
        }

        // Float64bits returns the IEEE 754 binary representation of f.
        public static ulong Float64bits(double f)
        {
            return @unsafe.Pointer(ref f).Value;
        }

        // Float64frombits returns the floating point number corresponding
        // the IEEE 754 binary representation b.
        public static double Float64frombits(ulong b)
        {
            return @unsafe.Pointer(ref b).Value;
        }
    }
}
