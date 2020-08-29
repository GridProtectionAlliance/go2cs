// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cmplx provides basic constants and mathematical functions for
// complex numbers.
// package cmplx -- go2cs converted at 2020 August 29 08:44:59 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Go\src\math\cmplx\abs.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class cmplx_package
    {
        // Abs returns the absolute value (also called the modulus) of x.
        public static double Abs(System.Numerics.Complex128 x)
        {
            return math.Hypot(real(x), imag(x));
        }
    }
}}
