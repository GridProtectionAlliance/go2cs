// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2020 August 29 08:45:00 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Go\src\math\cmplx\conj.go

using static go.builtin;

namespace go {
namespace math
{
    public static partial class cmplx_package
    {
        // Conj returns the complex conjugate of x.
        public static System.Numerics.Complex128 Conj(System.Numerics.Complex128 x)
        {
            return complex(real(x), -imag(x));
        }
    }
}}
