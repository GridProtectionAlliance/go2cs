// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests load/store ordering

// package main -- go2cs converted at 2020 August 29 09:58:22 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\loadstore.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        // testLoadStoreOrder tests for reordering of stores/loads.
        private static void testLoadStoreOrder()
        {
            var z = uint32(1000L);
            if (testLoadStoreOrder_ssa(ref z, 100L) == 0L)
            {
                println("testLoadStoreOrder failed");
                failed = true;
            }
        }

        //go:noinline
        private static long testLoadStoreOrder_ssa(ref uint z, ulong prec)
        {
            var old = z.Value; // load
            z.Value = uint32(prec); // store
            if (z < old.Value)
            { // load
                return 1L;
            }
            return 0L;
        }

        private static void testStoreSize()
        {
            array<ushort> a = new array<ushort>(new ushort[] { 11, 22, 33, 44 });
            testStoreSize_ssa(ref a[0L], ref a[2L], 77L);
            array<ushort> want = new array<ushort>(new ushort[] { 77, 22, 33, 44 });
            if (a != want)
            {
                fmt.Println("testStoreSize failed.  want =", want, ", got =", a);
                failed = true;
            }
        }

        //go:noinline
        private static void testStoreSize_ssa(ref ushort p, ref ushort q, uint v)
        { 
            // Test to make sure that (Store ptr (Trunc32to16 val) mem)
            // does not end up as a 32-bit store. It must stay a 16 bit store
            // even when Trunc32to16 is rewritten to be a nop.
            // To ensure that we get rewrite the Trunc32to16 before
            // we rewrite the Store, we force the truncate into an
            // earlier basic block by using it on both branches.
            var w = uint16(v);
            if (p != null)
            {
                p.Value = w;
            }
            else
            {
                q.Value = w;
            }
        }

        private static var failed = false;

        //go:noinline
        private static long testExtStore_ssa(ref byte p, bool b)
        {
            var x = p.Value;
            p.Value = 7L;
            if (b)
            {
                return int(x);
            }
            return 0L;
        }

        private static void testExtStore()
        {
            const long start = 8L;

            byte b = start;
            {
                var got = testExtStore_ssa(ref b, true);

                if (got != start)
                {
                    fmt.Println("testExtStore failed.  want =", start, ", got =", got);
                    failed = true;
                }

            }
        }

        private static long b = default;

        // testDeadStorePanic_ssa ensures that we don't optimize away stores
        // that could be read by after recover().  Modeled after fixedbugs/issue1304.
        //go:noinline
        private static long testDeadStorePanic_ssa(long a) => func((defer, _, recover) =>
        {
            defer(() =>
            {
                recover();
                r = a;
            }());
            a = 2L; // store
            var b = a - a; // optimized to zero
            long c = 4L;
            a = c / b; // store, but panics
            a = 3L; // store
            r = a;
            return;
        });

        private static void testDeadStorePanic()
        {
            {
                long want = 2L;
                var got = testDeadStorePanic_ssa(1L);

                if (want != got)
                {
                    fmt.Println("testDeadStorePanic failed.  want =", want, ", got =", got);
                    failed = true;
                }

            }
        }

        //go:noinline
        private static int loadHitStore8(sbyte x, ref sbyte p)
        {
            x *= x; // try to trash high bits (arch-dependent)
            p.Value = x; // store
            return int32(p.Value); // load and cast
        }

        //go:noinline
        private static uint loadHitStoreU8(byte x, ref byte p)
        {
            x *= x; // try to trash high bits (arch-dependent)
            p.Value = x; // store
            return uint32(p.Value); // load and cast
        }

        //go:noinline
        private static int loadHitStore16(short x, ref short p)
        {
            x *= x; // try to trash high bits (arch-dependent)
            p.Value = x; // store
            return int32(p.Value); // load and cast
        }

        //go:noinline
        private static uint loadHitStoreU16(ushort x, ref ushort p)
        {
            x *= x; // try to trash high bits (arch-dependent)
            p.Value = x; // store
            return uint32(p.Value); // load and cast
        }

        //go:noinline
        private static long loadHitStore32(int x, ref int p)
        {
            x *= x; // try to trash high bits (arch-dependent)
            p.Value = x; // store
            return int64(p.Value); // load and cast
        }

        //go:noinline
        private static ulong loadHitStoreU32(uint x, ref uint p)
        {
            x *= x; // try to trash high bits (arch-dependent)
            p.Value = x; // store
            return uint64(p.Value); // load and cast
        }

        private static void testLoadHitStore()
        { 
            // Test that sign/zero extensions are kept when a load-hit-store
            // is replaced by a register-register move.
            {
                sbyte @in = (1L << (int)(6L)) + 1L;
                sbyte p = default;
                var got = loadHitStore8(in, ref p);
                var want = int32(in * in);
                if (got != want)
                {
                    fmt.Println("testLoadHitStore (int8) failed. want =", want, ", got =", got);
                    failed = true;
                }
            }
            {
                @in = (1L << (int)(6L)) + 1L;
                p = default;
                got = loadHitStoreU8(in, ref p);
                want = uint32(in * in);
                if (got != want)
                {
                    fmt.Println("testLoadHitStore (uint8) failed. want =", want, ", got =", got);
                    failed = true;
                }
            }
            {
                @in = (1L << (int)(10L)) + 1L;
                p = default;
                got = loadHitStore16(in, ref p);
                want = int32(in * in);
                if (got != want)
                {
                    fmt.Println("testLoadHitStore (int16) failed. want =", want, ", got =", got);
                    failed = true;
                }
            }
            {
                @in = (1L << (int)(10L)) + 1L;
                p = default;
                got = loadHitStoreU16(in, ref p);
                want = uint32(in * in);
                if (got != want)
                {
                    fmt.Println("testLoadHitStore (uint16) failed. want =", want, ", got =", got);
                    failed = true;
                }
            }
            {
                @in = (1L << (int)(30L)) + 1L;
                p = default;
                got = loadHitStore32(in, ref p);
                want = int64(in * in);
                if (got != want)
                {
                    fmt.Println("testLoadHitStore (int32) failed. want =", want, ", got =", got);
                    failed = true;
                }
            }
            {
                @in = (1L << (int)(30L)) + 1L;
                p = default;
                got = loadHitStoreU32(in, ref p);
                want = uint64(in * in);
                if (got != want)
                {
                    fmt.Println("testLoadHitStore (uint32) failed. want =", want, ", got =", got);
                    failed = true;
                }
            }
        }

        private static void Main() => func((_, panic, __) =>
        {
            testLoadStoreOrder();
            testStoreSize();
            testExtStore();
            testDeadStorePanic();
            testLoadHitStore();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
