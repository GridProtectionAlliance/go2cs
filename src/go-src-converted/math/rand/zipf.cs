// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// W.Hormann, G.Derflinger:
// "Rejection-Inversion to Generate Variates
// from Monotone Discrete Distributions"
// http://eeyore.wu-wien.ac.at/papers/96-04-04.wh-der.ps.gz
namespace go.math;

using math = math_package;

partial class rand_package {

// A Zipf generates Zipf distributed variates.
[GoType] partial struct Zipf {
    internal ж<Rand> r;
    internal float64 imax;
    internal float64 v;
    internal float64 q;
    internal float64 s;
    internal float64 oneminusQ;
    internal float64 oneminusQinv;
    internal float64 hxm;
    internal float64 hx0minusHxm;
}

[GoRecv] internal static float64 h(this ref Zipf z, float64 x) {
    return math.Exp(z.oneminusQ * math.Log(z.v + x)) * z.oneminusQinv;
}

[GoRecv] internal static float64 hinv(this ref Zipf z, float64 x) {
    return math.Exp(z.oneminusQinv * math.Log(z.oneminusQ * x)) - z.v;
}

// NewZipf returns a [Zipf] variate generator.
// The generator generates values k ∈ [0, imax]
// such that P(k) is proportional to (v + k) ** (-s).
// Requirements: s > 1 and v >= 1.
public static ж<Zipf> NewZipf(ж<Rand> Ꮡr, float64 s, float64 v, uint64 imax) {
    ref var r = ref Ꮡr.val;

    var z = @new<Zipf>();
    if (s <= 1.0F || v < 1) {
        return default!;
    }
    z.val.r = r;
    z.val.imax = ((float64)imax);
    z.val.v = v;
    z.val.q = s;
    z.val.oneminusQ = 1.0F - (~z).q;
    z.val.oneminusQinv = 1.0F / (~z).oneminusQ;
    z.val.hxm = z.h((~z).imax + 0.5F);
    z.val.hx0minusHxm = z.h(0.5F) - math.Exp(math.Log((~z).v) * (-(~z).q)) - (~z).hxm;
    z.val.s = 1 - z.hinv(z.h(1.5F) - math.Exp(-(~z).q * math.Log((~z).v + 1.0F)));
    return z;
}

// Uint64 returns a value drawn from the [Zipf] distribution described
// by the [Zipf] object.
[GoRecv] public static uint64 Uint64(this ref Zipf z) {
    if (z == nil) {
        throw panic("rand: nil Zipf");
    }
    var k = 0.0F;
    while (ᐧ) {
        var r = z.r.Float64();
        // r on [0,1]
        var ur = z.hxm + r * z.hx0minusHxm;
        var x = z.hinv(ur);
        k = math.Floor(x + 0.5F);
        if (k - x <= z.s) {
            break;
        }
        if (ur >= z.h(k + 0.5F) - math.Exp(-math.Log(k + z.v) * z.q)) {
            break;
        }
    }
    return ((uint64)k);
}

} // end rand_package
