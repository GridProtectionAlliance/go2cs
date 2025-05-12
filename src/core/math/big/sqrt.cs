// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;
using sync = sync_package;

partial class big_package {


[GoType("dyn")] partial struct threeOnceᴛ1 {
    public partial ref sync_package.Once Once { get; }
    internal ж<Float> v;
}
internal static threeOnceᴛ1 threeOnce;

internal static ж<Float> three() {
    threeOnce.Do(
    var threeOnceʗ2 = threeOnce;
    () => {
        threeOnceʗ2.v = NewFloat(3.0F);
    });
    return threeOnce.v;
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
[GoRecv("capture")] public static ж<Float> Sqrt(this ref Float z, ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.val;

    if (debugFloat) {
        x.validate();
    }
    if (z.prec == 0) {
        z.prec = x.prec;
    }
    if (x.Sign() == -1) {
        // following IEEE754-2008 (section 7.2)
        throw panic(new ErrNaN("square root of negative operand"));
    }
    // handle ±0 and +∞
    if (x.form != finite) {
        z.acc = Exact;
        z.form = x.form;
        z.neg = x.neg;
        // IEEE754-2008 requires √±0 = ±0
        return SqrtꓸᏑz;
    }
    // MantExp sets the argument's precision to the receiver's, and
    // when z.prec > x.prec this will lower z.prec. Restore it after
    // the MantExp call.
    var prec = z.prec;
    nint b = x.MantExp(z);
    z.prec = prec;
    // Compute √(z·2**b) as
    //   √( z)·2**(½b)     if b is even
    //   √(2z)·2**(⌊½b⌋)   if b > 0 is odd
    //   √(½z)·2**(⌈½b⌉)   if b < 0 is odd
    var exprᴛ1 = b % 2;
    if (exprᴛ1 is 0) {
    }
    else if (exprᴛ1 is 1) {
        z.exp++;
    }
    else if (exprᴛ1 == -1) {
        z.exp--;
    }

    // nothing to do
    // 0.25 <= z < 2.0
    // Solving 1/x² - z = 0 avoids Quo calls and is faster, especially
    // for high precisions.
    z.sqrtInverse(z);
    // re-attach halved exponent
    return z.SetMantExp(z, b / 2);
}

// Compute √x (to z.prec precision) by solving
//
//	1/t² - x = 0
//
// for t (using Newton's method), and then inverting.
[GoRecv] public static void sqrtInverse(this ref Float z, ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.val;

    // let
    //   f(t) = 1/t² - x
    // then
    //   g(t) = f(t)/f'(t) = -½t(1 - xt²)
    // and the next guess is given by
    //   t2 = t - g(t) = ½t(3 - xt²)
    var u = newFloat(z.prec);
    var v = newFloat(z.prec);
    var three = three();
    var ng = 
    var threeʗ1 = three;
    var uʗ1 = u;
    var vʗ1 = v;
    (ж<Float> t) => {
        uʗ1.val.prec = t.val.prec;
        vʗ1.val.prec = t.val.prec;
        uʗ1.Mul(t, t);
        // u = t²
        uʗ1.Mul(Ꮡx, uʗ1);
        //   = xt²
        vʗ1.Sub(threeʗ1, uʗ1);
        // v = 3 - xt²
        uʗ1.Mul(t, vʗ1);
        // u = t(3 - xt²)
        (~uʗ1).exp--;
        //   = ½t(3 - xt²)
        return t.Set(uʗ1);
    };
    var (xf, Δ_) = x.Float64();
    var sqi = newFloat(z.prec);
    sqi.SetFloat64(1 / math.Sqrt(xf));
    for (var prec = z.prec + 32; (~sqi).prec < prec; ) {
        sqi.val.prec *= 2;
        sqi = ng(sqi);
    }
    // sqi = 1/√x
    // x/√x = √x
    z.Mul(Ꮡx, sqi);
}

// newFloat returns a new *Float with space for twice the given
// precision.
internal static ж<Float> newFloat(uint32 prec2) {
    var z = @new<Float>();
    // nat.make ensures the slice length is > 0
    z.val.mant = (~z).mant.make(((nint)(prec2 / _W)) * 2);
    return z;
}

} // end big_package
