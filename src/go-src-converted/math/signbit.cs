// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:57 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\signbit.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Signbit returns true if x is negative or negative zero.
        public static bool Signbit(double x)
        {
            return Float64bits(x) & (1L << (int)(63L)) != 0L;
        }
    }
}
