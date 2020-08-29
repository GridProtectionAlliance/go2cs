// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests phi implementation

// package main -- go2cs converted at 2020 August 29 09:58:26 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\regalloc.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long phiOverwrite_ssa()
        {
            long n = default;
            for (long i = 0L; i < 10L; i++)
            {
                if (i == 6L)
                {
                    break;
                }
                n = i;
            }
            return n;
        }

        private static void phiOverwrite()
        {
            long want = 5L;
            var got = phiOverwrite_ssa();
            if (got != want)
            {
                println("phiOverwrite_ssa()=", want, ", got", got);
                failed = true;
            }
        }

        private static long phiOverwriteBig_ssa()
        {
            long a = default;            long b = default;            long c = default;            long d = default;            long e = default;            long f = default;            long g = default;            long h = default;            long i = default;            long j = default;            long k = default;            long l = default;            long m = default;            long n = default;            long o = default;            long p = default;            long q = default;            long r = default;            long s = default;            long t = default;            long u = default;            long v = default;            long w = default;            long x = default;            long y = default;            long z = default;

            a = 1L;
            for (long idx = 0L; idx < 26L; idx++)
            {
                a = b;
                b = c;
                c = d;
                d = e;
                e = f;
                f = g;
                g = h;
                h = i;
                i = j;
                j = k;
                k = l;
                l = m;
                m = n;
                n = o;
                o = p;
                p = q;
                q = r;
                r = s;
                s = t;
                t = u;
                u = v;
                v = w;
                w = x;
                x = y;
                y = z;
                z = a;
            }

            return a * 1L + b * 2L + c * 3L + d * 4L + e * 5L + f * 6L + g * 7L + h * 8L + i * 9L + j * 10L + k * 11L + l * 12L + m * 13L + n * 14L + o * 15L + p * 16L + q * 17L + r * 18L + s * 19L + t * 20L + u * 21L + v * 22L + w * 23L + x * 24L + y * 25L + z * 26L;
        }

        private static void phiOverwriteBig()
        {
            long want = 1L;
            var got = phiOverwriteBig_ssa();
            if (got != want)
            {
                println("phiOverwriteBig_ssa()=", want, ", got", got);
                failed = true;
            }
        }

        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            phiOverwrite();
            phiOverwriteBig();
            if (failed)
            {
                panic("failed");
            }
        });
    }
}
