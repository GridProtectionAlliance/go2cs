// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package big -- go2cs converted at 2020 October 08 03:25:53 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\sqrt.go
using math = go.math_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static var threeOnce = default;

        private static ptr<Float> three()
        {
            threeOnce.Do(() =>
            {
                threeOnce.v = NewFloat(3.0F);
            });
            return _addr_threeOnce.v!;

        }

        // Sqrt sets z to the rounded square root of x, and returns it.
        //
        // If z's precision is 0, it is changed to x's precision before the
        // operation. Rounding is performed according to z's precision and
        // rounding mode, but z's accuracy is not computed. Specifically, the
        // result of z.Acc() is undefined.
        //
        // The function panics if z < 0. The value of z is undefined in that
        // case.
        private static ptr<Float> Sqrt(this ptr<Float> _addr_z, ptr<Float> _addr_x) => func((_, panic, __) =>
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;

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
                return _addr_z!;

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
                    z.exp++;
                    break;
                case -1L: 
                    z.exp--;
                    break;
            } 
            // 0.25 <= z < 2.0

            // Solving 1/x² - z = 0 avoids Quo calls and is faster, especially
            // for high precisions.
            z.sqrtInverse(z); 

            // re-attach halved exponent
            return _addr_z.SetMantExp(z, b / 2L)!;

        });

        // Compute √x (to z.prec precision) by solving
        //   1/t² - x = 0
        // for t (using Newton's method), and then inverting.
        private static void sqrtInverse(this ptr<Float> _addr_z, ptr<Float> _addr_x)
        {
            ref Float z = ref _addr_z.val;
            ref Float x = ref _addr_x.val;
 
            // let
            //   f(t) = 1/t² - x
            // then
            //   g(t) = f(t)/f'(t) = -½t(1 - xt²)
            // and the next guess is given by
            //   t2 = t - g(t) = ½t(3 - xt²)
            var u = newFloat(z.prec);
            var v = newFloat(z.prec);
            var three = three();
            Func<ptr<Float>, ptr<Float>> ng = t =>
            {
                u.prec = t.prec;
                v.prec = t.prec;
                u.Mul(t, t); // u = t²
                u.Mul(x, u); //   = xt²
                v.Sub(three, u); // v = 3 - xt²
                u.Mul(t, v); // u = t(3 - xt²)
                u.exp--; //   = ½t(3 - xt²)
                return t.Set(u);

            }
;

            var (xf, _) = x.Float64();
            var sqi = newFloat(z.prec);
            sqi.SetFloat64(1L / math.Sqrt(xf));
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

        // newFloat returns a new *Float with space for twice the given
        // precision.
        private static ptr<Float> newFloat(uint prec2)
        {
            ptr<Float> z = @new<Float>(); 
            // nat.make ensures the slice length is > 0
            z.mant = z.mant.make(int(prec2 / _W) * 2L);
            return _addr_z!;

        }
    }
}}
