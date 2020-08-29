// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package big -- go2cs converted at 2020 August 29 08:29:33 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\sqrt.go
using math = go.math_package;
using static go.builtin;
using System;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static var half = NewFloat(0.5F);        private static var two = NewFloat(2.0F);        private static var three = NewFloat(3.0F);

        // Sqrt sets z to the rounded square root of x, and returns it.
        //
        // If z's precision is 0, it is changed to x's precision before the
        // operation. Rounding is performed according to z's precision and
        // rounding mode.
        //
        // The function panics if z < 0. The value of z is undefined in that
        // case.
        private static ref Float Sqrt(this ref Float _z, ref Float _x) => func(_z, _x, (ref Float z, ref Float x, Defer _, Panic panic, Recover __) =>
        {
            if (debugFloat)
            {
                x.validate();
            }
            if (z.prec == 0L)
            {
                z.prec = x.prec;
            }
            if (x.Sign() == -1L)
            { 
                // following IEEE754-2008 (section 7.2)
                panic(new ErrNaN("square root of negative operand"));
            } 

            // handle ±0 and +∞
            if (x.form != finite)
            {
                z.acc = Exact;
                z.form = x.form;
                z.neg = x.neg; // IEEE754-2008 requires √±0 = ±0
                return z;
            } 

            // MantExp sets the argument's precision to the receiver's, and
            // when z.prec > x.prec this will lower z.prec. Restore it after
            // the MantExp call.
            var prec = z.prec;
            var b = x.MantExp(z);
            z.prec = prec; 

            // Compute √(z·2**b) as
            //   √( z)·2**(½b)     if b is even
            //   √(2z)·2**(⌊½b⌋)   if b > 0 is odd
            //   √(½z)·2**(⌈½b⌉)   if b < 0 is odd
            switch (b % 2L)
            {
                case 0L: 
                    break;
                case 1L: 
                    z.Mul(two, z);
                    break;
                case -1L: 
                    z.Mul(half, z);
                    break;
            } 
            // 0.25 <= z < 2.0

            // Solving x² - z = 0 directly requires a Quo call, but it's
            // faster for small precisions.
            //
            // Solving 1/x² - z = 0 avoids the Quo call and is much faster for
            // high precisions.
            //
            // 128bit precision is an empirically chosen threshold.
            if (z.prec <= 128L)
            {
                z.sqrtDirect(z);
            }
            else
            {
                z.sqrtInverse(z);
            } 

            // re-attach halved exponent
            return z.SetMantExp(z, b / 2L);
        });

        // Compute √x (up to prec 128) by solving
        //   t² - x = 0
        // for t, starting with a 53 bits precision guess from math.Sqrt and
        // then using at most two iterations of Newton's method.
        private static void sqrtDirect(this ref Float _z, ref Float _x) => func(_z, _x, (ref Float z, ref Float x, Defer _, Panic panic, Recover __) =>
        { 
            // let
            //   f(t) = t² - x
            // then
            //   g(t) = f(t)/f'(t) = ½(t² - x)/t
            // and the next guess is given by
            //   t2 = t - g(t) = ½(t² + x)/t
            ptr<Float> u = @new<Float>();
            Func<ref Float, ref Float> ng = t =>
            {
                u.prec = t.prec;
                u.Mul(t, t); // u = t²
                u.Add(u, x); //   = t² + x
                u.Mul(half, u); //   = ½(t² + x)
                return t.Quo(u, t); //   = ½(t² + x)/t
            }
;

            var (xf, _) = x.Float64();
            var sq = NewFloat(math.Sqrt(xf));


            if (z.prec > 128L)
            {
                panic("sqrtDirect: only for z.prec <= 128");
                goto __switch_break0;
            }
            if (z.prec > 64L)
            {
                sq.prec *= 2L;
                sq = ng(sq);
            }
            // default: 
                sq.prec *= 2L;
                sq = ng(sq);

            __switch_break0:;

            z.Set(sq);
        });

        // Compute √x (to z.prec precision) by solving
        //   1/t² - x = 0
        // for t (using Newton's method), and then inverting.
        private static void sqrtInverse(this ref Float z, ref Float x)
        { 
            // let
            //   f(t) = 1/t² - x
            // then
            //   g(t) = f(t)/f'(t) = -½t(1 - xt²)
            // and the next guess is given by
            //   t2 = t - g(t) = ½t(3 - xt²)
            ptr<Float> u = @new<Float>();
            Func<ref Float, ref Float> ng = t =>
            {
                u.prec = t.prec;
                u.Mul(t, t); // u = t²
                u.Mul(x, u); //   = xt²
                u.Sub(three, u); //   = 3 - xt²
                u.Mul(t, u); //   = t(3 - xt²)
                return t.Mul(half, u); //   = ½t(3 - xt²)
            }
;

            var (xf, _) = x.Float64();
            var sqi = NewFloat(1L / math.Sqrt(xf));
            {
                var prec = z.prec + 32L;

                while (sqi.prec < prec)
                {
                    sqi.prec *= 2L;
                    sqi = ng(sqi);
                } 
                // sqi = 1/√x

                // x/√x = √x

            } 
            // sqi = 1/√x

            // x/√x = √x
            z.Mul(x, sqi);
        }
    }
}}
