// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:58:25 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\phi.go
// Test to make sure spills of cast-shortened values
// don't end up spilling the pre-shortened size instead
// of the post-shortened size.

using fmt = go.fmt_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // unfoldable true
        private static var true_ = true;

        private static array<int> data1 = new array<int>(26L);
        private static array<long> data2 = new array<long>(26L);

        private static void init()
        {
            for (long i = 0L; i < 26L; i++)
            { 
                // If we spill all 8 bytes of this datum, the 1 in the high-order 4 bytes
                // will overwrite some other variable in the stack frame.
                data2[i] = 0x100000000UL;
            }

        }

        private static int foo()
        {
            int a = default;            int b = default;            int c = default;            int d = default;            int e = default;            int f = default;            int g = default;            int h = default;            int i = default;            int j = default;            int k = default;            int l = default;            int m = default;            int n = default;            int o = default;            int p = default;            int q = default;            int r = default;            int s = default;            int t = default;            int u = default;            int v = default;            int w = default;            int x = default;            int y = default;            int z = default;

            if (true_)
            {
                a = data1[0L];
                b = data1[1L];
                c = data1[2L];
                d = data1[3L];
                e = data1[4L];
                f = data1[5L];
                g = data1[6L];
                h = data1[7L];
                i = data1[8L];
                j = data1[9L];
                k = data1[10L];
                l = data1[11L];
                m = data1[12L];
                n = data1[13L];
                o = data1[14L];
                p = data1[15L];
                q = data1[16L];
                r = data1[17L];
                s = data1[18L];
                t = data1[19L];
                u = data1[20L];
                v = data1[21L];
                w = data1[22L];
                x = data1[23L];
                y = data1[24L];
                z = data1[25L];
            }
            else
            {
                a = int32(data2[0L]);
                b = int32(data2[1L]);
                c = int32(data2[2L]);
                d = int32(data2[3L]);
                e = int32(data2[4L]);
                f = int32(data2[5L]);
                g = int32(data2[6L]);
                h = int32(data2[7L]);
                i = int32(data2[8L]);
                j = int32(data2[9L]);
                k = int32(data2[10L]);
                l = int32(data2[11L]);
                m = int32(data2[12L]);
                n = int32(data2[13L]);
                o = int32(data2[14L]);
                p = int32(data2[15L]);
                q = int32(data2[16L]);
                r = int32(data2[17L]);
                s = int32(data2[18L]);
                t = int32(data2[19L]);
                u = int32(data2[20L]);
                v = int32(data2[21L]);
                w = int32(data2[22L]);
                x = int32(data2[23L]);
                y = int32(data2[24L]);
                z = int32(data2[25L]);
            } 
            // Lots of phis of the form phi(int32,int64) of type int32 happen here.
            // Some will be stack phis. For those stack phis, make sure the spill
            // of the second argument uses the phi's width (4 bytes), not its width
            // (8 bytes).  Otherwise, a random stack slot gets clobbered.
            runtime.Gosched();
            return a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p + q + r + s + t + u + v + w + x + y + z;
        }

        private static void Main() => func((_, panic, __) =>
        {
            var want = int32(0L);
            var got = foo();
            if (got != want)
            {
                fmt.Printf("want %d, got %d\n", want, got);
                panic("bad");
            }
        });
    }
}
