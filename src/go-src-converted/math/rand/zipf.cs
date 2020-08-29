// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// W.Hormann, G.Derflinger:
// "Rejection-Inversion to Generate Variates
// from Monotone Discrete Distributions"
// http://eeyore.wu-wien.ac.at/papers/96-04-04.wh-der.ps.gz

// package rand -- go2cs converted at 2020 August 29 08:25:55 UTC
// import "math/rand" ==> using rand = go.math.rand_package
// Original source: C:\Go\src\math\rand\zipf.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class rand_package
    {
        // A Zipf generates Zipf distributed variates.
        public partial struct Zipf
        {
            public ptr<Rand> r;
            public double imax;
            public double v;
            public double q;
            public double s;
            public double oneminusQ;
            public double oneminusQinv;
            public double hxm;
            public double hx0minusHxm;
        }

        private static double h(this ref Zipf z, double x)
        {
            return math.Exp(z.oneminusQ * math.Log(z.v + x)) * z.oneminusQinv;
        }

        private static double hinv(this ref Zipf z, double x)
        {
            return math.Exp(z.oneminusQinv * math.Log(z.oneminusQ * x)) - z.v;
        }

        // NewZipf returns a Zipf variate generator.
        // The generator generates values k ∈ [0, imax]
        // such that P(k) is proportional to (v + k) ** (-s).
        // Requirements: s > 1 and v >= 1.
        public static ref Zipf NewZipf(ref Rand r, double s, double v, ulong imax)
        {
            ptr<Zipf> z = @new<Zipf>();
            if (s <= 1.0F || v < 1L)
            {
                return null;
            }
            z.r = r;
            z.imax = float64(imax);
            z.v = v;
            z.q = s;
            z.oneminusQ = 1.0F - z.q;
            z.oneminusQinv = 1.0F / z.oneminusQ;
            z.hxm = z.h(z.imax + 0.5F);
            z.hx0minusHxm = z.h(0.5F) - math.Exp(math.Log(z.v) * (-z.q)) - z.hxm;
            z.s = 1L - z.hinv(z.h(1.5F) - math.Exp(-z.q * math.Log(z.v + 1.0F)));
            return z;
        }

        // Uint64 returns a value drawn from the Zipf distribution described
        // by the Zipf object.
        private static ulong Uint64(this ref Zipf _z) => func(_z, (ref Zipf z, Defer _, Panic panic, Recover __) =>
        {
            if (z == null)
            {
                panic("rand: nil Zipf");
            }
            float k = 0.0F;

            while (true)
            {
                var r = z.r.Float64(); // r on [0,1]
                var ur = z.hxm + r * z.hx0minusHxm;
                var x = z.hinv(ur);
                k = math.Floor(x + 0.5F);
                if (k - x <= z.s)
                {
                    break;
                }
                if (ur >= z.h(k + 0.5F) - math.Exp(-math.Log(k + z.v) * z.q))
                {
                    break;
                }
            }

            return uint64(k);
        });
    }
}}
