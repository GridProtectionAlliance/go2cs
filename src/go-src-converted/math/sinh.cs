// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:58 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\sinh.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Floating-point hyperbolic sine and cosine.

            The exponential func is called for arguments
            greater in magnitude than 0.5.

            A series is used for arguments smaller in magnitude than 0.5.

            Cosh(x) is computed from the exponential func for
            all arguments.
        */

        // Sinh returns the hyperbolic sine of x.
        //
        // Special cases are:
        //    Sinh(±0) = ±0
        //    Sinh(±Inf) = ±Inf
        //    Sinh(NaN) = NaN
        public static double Sinh(double x)
;

        private static double sinh(double x)
        { 
            // The coefficients are #2029 from Hart & Cheney. (20.36D)
            const float P0 = -0.6307673640497716991184787251e+6F;
            const float P1 = -0.8991272022039509355398013511e+5F;
            const float P2 = -0.2894211355989563807284660366e+4F;
            const float P3 = -0.2630563213397497062819489e+2F;
            const float Q0 = -0.6307673640497716991212077277e+6F;
            const float Q1 = 0.1521517378790019070696485176e+5F;
            const float Q2 = -0.173678953558233699533450911e+3F;

            var sign = false;
            if (x < 0L)
            {>>MARKER:FUNCTION_Sinh_BLOCK_PREFIX<<
                x = -x;
                sign = true;
            }
            double temp = default;

            if (true == x > 21L) 
                temp = Exp(x) / 2L;
            else if (true == x > 0.5F) 
                temp = (Exp(x) - Exp(-x)) / 2L;
            else 
                var sq = x * x;
                temp = (((P3 * sq + P2) * sq + P1) * sq + P0) * x;
                temp = temp / (((sq + Q2) * sq + Q1) * sq + Q0);
                        if (sign)
            {
                temp = -temp;
            }
            return temp;
        }

        // Cosh returns the hyperbolic cosine of x.
        //
        // Special cases are:
        //    Cosh(±0) = 1
        //    Cosh(±Inf) = +Inf
        //    Cosh(NaN) = NaN
        public static double Cosh(double x)
;

        private static double cosh(double x)
        {
            if (x < 0L)
            {>>MARKER:FUNCTION_Cosh_BLOCK_PREFIX<<
                x = -x;
            }
            if (x > 21L)
            {
                return Exp(x) / 2L;
            }
            return (Exp(x) + Exp(-x)) / 2L;
        }
    }
}
