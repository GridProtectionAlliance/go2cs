// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cmplx -- go2cs converted at 2020 August 29 08:45:00 UTC
// import "math/cmplx" ==> using cmplx = go.math.cmplx_package
// Original source: C:\Go\src\math\cmplx\isnan.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class cmplx_package
    {
        // IsNaN returns true if either real(x) or imag(x) is NaN
        // and neither is an infinity.
        public static bool IsNaN(System.Numerics.Complex128 x)
        {

            if (math.IsInf(real(x), 0L) || math.IsInf(imag(x), 0L)) 
                return false;
            else if (math.IsNaN(real(x)) || math.IsNaN(imag(x))) 
                return true;
                        return false;
        }

        // NaN returns a complex ``not-a-number'' value.
        public static System.Numerics.Complex128 NaN()
        {
            var nan = math.NaN();
            return complex(nan, nan);
        }
    }
}}
