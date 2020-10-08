// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:19:18 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\complex.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // inf2one returns a signed 1 if f is an infinity and a signed 0 otherwise.
        // The sign of the result is the sign of f.
        private static double inf2one(double f)
        {
            float g = 0.0F;
            if (isInf(f))
            {
                g = 1.0F;
            }
            return copysign(g, f);

        }

        private static System.Numerics.Complex128 complex128div(System.Numerics.Complex128 n, System.Numerics.Complex128 m)
        {
            double e = default;            double f = default; // complex(e, f) = n/m

            // Algorithm for robust complex division as described in
            // Robert L. Smith: Algorithm 116: Complex division. Commun. ACM 5(8): 435 (1962).
 // complex(e, f) = n/m

            // Algorithm for robust complex division as described in
            // Robert L. Smith: Algorithm 116: Complex division. Commun. ACM 5(8): 435 (1962).
            if (abs(real(m)) >= abs(imag(m)))
            {
                var ratio = imag(m) / real(m);
                var denom = real(m) + ratio * imag(m);
                e = (real(n) + imag(n) * ratio) / denom;
                f = (imag(n) - real(n) * ratio) / denom;
            }
            else
            {
                ratio = real(m) / imag(m);
                denom = imag(m) + ratio * real(m);
                e = (real(n) * ratio + imag(n)) / denom;
                f = (imag(n) * ratio - real(n)) / denom;
            }

            if (isNaN(e) && isNaN(f))
            { 
                // Correct final result to infinities and zeros if applicable.
                // Matches C99: ISO/IEC 9899:1999 - G.5.1  Multiplicative operators.

                var a = real(n);
                var b = imag(n);
                var c = real(m);
                var d = imag(m);


                if (m == 0L && (!isNaN(a) || !isNaN(b))) 
                    e = copysign(inf, c) * a;
                    f = copysign(inf, c) * b;
                else if ((isInf(a) || isInf(b)) && isFinite(c) && isFinite(d)) 
                    a = inf2one(a);
                    b = inf2one(b);
                    e = inf * (a * c + b * d);
                    f = inf * (b * c - a * d);
                else if ((isInf(c) || isInf(d)) && isFinite(a) && isFinite(b)) 
                    c = inf2one(c);
                    d = inf2one(d);
                    e = 0L * (a * c + b * d);
                    f = 0L * (b * c - a * d);
                
            }

            return complex(e, f);

        }
    }
}
