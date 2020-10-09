// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2020 October 09 05:07:48 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Go\src\math\cmplx\rect.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class cmplx_package
    {
        // Rect returns the complex number x with polar coordinates r, θ.
        public static System.Numerics.Complex128 Rect(double r, double θ)
        {
            var (s, c) = math.Sincos(θ);
            return complex(r * c, r * s);
        }
    }
}}
