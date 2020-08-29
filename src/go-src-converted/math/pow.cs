// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:57 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\pow.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        private static bool isOddInt(double x)
        {
            var (xi, xf) = Modf(x);
            return xf == 0L && int64(xi) & 1L == 1L;
        }

        // Special cases taken from FreeBSD's /usr/src/lib/msun/src/e_pow.c
        // updated by IEEE Std. 754-2008 "Section 9.2.1 Special values".

        // Pow returns x**y, the base-x exponential of y.
        //
        // Special cases are (in order):
        //    Pow(x, ±0) = 1 for any x
        //    Pow(1, y) = 1 for any y
        //    Pow(x, 1) = x for any x
        //    Pow(NaN, y) = NaN
        //    Pow(x, NaN) = NaN
        //    Pow(±0, y) = ±Inf for y an odd integer < 0
        //    Pow(±0, -Inf) = +Inf
        //    Pow(±0, +Inf) = +0
        //    Pow(±0, y) = +Inf for finite y < 0 and not an odd integer
        //    Pow(±0, y) = ±0 for y an odd integer > 0
        //    Pow(±0, y) = +0 for finite y > 0 and not an odd integer
        //    Pow(-1, ±Inf) = 1
        //    Pow(x, +Inf) = +Inf for |x| > 1
        //    Pow(x, -Inf) = +0 for |x| > 1
        //    Pow(x, +Inf) = +0 for |x| < 1
        //    Pow(x, -Inf) = +Inf for |x| < 1
        //    Pow(+Inf, y) = +Inf for y > 0
        //    Pow(+Inf, y) = +0 for y < 0
        //    Pow(-Inf, y) = Pow(-0, -y)
        //    Pow(x, y) = NaN for finite x < 0 and finite non-integer y
        public static double Pow(double x, double y)
;

        private static double pow(double x, double y)
        {

            if (y == 0L || x == 1L) 
                return 1L;
            else if (y == 1L) 
                return x;
            else if (IsNaN(x) || IsNaN(y)) 
                return NaN();
            else if (x == 0L) 

                if (y < 0L) 
                    if (isOddInt(y))
                    {>>MARKER:FUNCTION_Pow_BLOCK_PREFIX<<
                        return Copysign(Inf(1L), x);
                    }
                    return Inf(1L);
                else if (y > 0L) 
                    if (isOddInt(y))
                    {
                        return x;
                    }
                    return 0L;
                            else if (IsInf(y, 0L)) 

                if (x == -1L) 
                    return 1L;
                else if ((Abs(x) < 1L) == IsInf(y, 1L)) 
                    return 0L;
                else 
                    return Inf(1L);
                            else if (IsInf(x, 0L)) 
                if (IsInf(x, -1L))
                {
                    return Pow(1L / x, -y); // Pow(-0, -y)
                }

                if (y < 0L) 
                    return 0L;
                else if (y > 0L) 
                    return Inf(1L);
                            else if (y == 0.5F) 
                return Sqrt(x);
            else if (y == -0.5F) 
                return 1L / Sqrt(x);
                        var absy = y;
            var flip = false;
            if (absy < 0L)
            {
                absy = -absy;
                flip = true;
            }
            var (yi, yf) = Modf(absy);
            if (yf != 0L && x < 0L)
            {
                return NaN();
            }
            if (yi >= 1L << (int)(63L))
            { 
                // yi is a large even int that will lead to overflow (or underflow to 0)
                // for all x except -1 (x == 1 was handled earlier)

                if (x == -1L) 
                    return 1L;
                else if ((Abs(x) < 1L) == (y > 0L)) 
                    return 0L;
                else 
                    return Inf(1L);
                            } 

            // ans = a1 * 2**ae (= 1 for now).
            float a1 = 1.0F;
            long ae = 0L; 

            // ans *= x**yf
            if (yf != 0L)
            {
                if (yf > 0.5F)
                {
                    yf--;
                    yi++;
                }
                a1 = Exp(yf * Log(x));
            } 

            // ans *= x**yi
            // by multiplying in successive squarings
            // of x according to bits of yi.
            // accumulate powers of two into exp.
            var (x1, xe) = Frexp(x);
            {
                var i = int64(yi);

                while (i != 0L)
                {
                    if (xe < -1L << (int)(12L) || 1L << (int)(12L) < xe)
                    { 
                        // catch xe before it overflows the left shift below
                        // Since i !=0 it has at least one bit still set, so ae will accumulate xe
                        // on at least one more iteration, ae += xe is a lower bound on ae
                        // the lower bound on ae exceeds the size of a float64 exp
                        // so the final call to Ldexp will produce under/overflow (0/Inf)
                        ae += xe;
                        break;
                    i >>= 1L;
                    }
                    if (i & 1L == 1L)
                    {
                        a1 *= x1;
                        ae += xe;
                    }
                    x1 *= x1;
                    xe <<= 1L;
                    if (x1 < .5F)
                    {
                        x1 += x1;
                        xe--;
                    }
                } 

                // ans = a1*2**ae
                // if flip { ans = 1 / ans }
                // but in the opposite order

            } 

            // ans = a1*2**ae
            // if flip { ans = 1 / ans }
            // but in the opposite order
            if (flip)
            {
                a1 = 1L / a1;
                ae = -ae;
            }
            return Ldexp(a1, ae);
        }
    }
}
