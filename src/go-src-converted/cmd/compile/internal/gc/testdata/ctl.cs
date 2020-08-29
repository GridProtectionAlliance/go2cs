// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test control flow

// package main -- go2cs converted at 2020 August 29 09:58:11 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\ctl.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // nor_ssa calculates NOR(a, b).
        // It is implemented in a way that generates
        // phi control values.
        private static bool nor_ssa(bool a, bool b)
        {
            bool c = default;
            if (a)
            {
                c = true;
            }
            if (b)
            {
                c = true;
            }
            if (c)
            {
                return false;
            }
            return true;
        }

        private static void testPhiControl()
        {
            array<array<bool>> tests = new array<array<bool>>(new array<bool>[] { {false,false,true}, {true,false,false}, {false,true,false}, {true,true,false} });
            foreach (var (_, test) in tests)
            {
                var a = test[0L];
                var b = test[1L];
                var got = nor_ssa(a, b);
                var want = test[2L];
                if (want != got)
                {
                    print("nor(", a, ", ", b, ")=", want, " got ", got, "\n");
                    failed = true;
                }
            }
        }

        private static bool emptyRange_ssa(slice<byte> b)
        {
            foreach (var (_, x) in b)
            {
                _ = x;
            }
            return true;
        }

        private static void testEmptyRange()
        {
            if (!emptyRange_ssa(new slice<byte>(new byte[] {  })))
            {
                println("emptyRange_ssa([]byte{})=false, want true");
                failed = true;
            }
        }

        private static long switch_ssa(long a)
        {
            long ret = 0L;
            switch (a)
            {
                case 5L: 
                    ret += 5L;
                    break;
                case 4L: 
                    ret += 4L;
                    break;
                case 3L: 
                    ret += 3L;
                    break;
                case 2L: 
                    ret += 2L;
                    break;
                case 1L: 
                    ret += 1L;
                    break;
            }
            return ret;

        }

        private static long fallthrough_ssa(long a)
        {
            long ret = 0L;

            if (a == 5L)
            {
                ret++;
                fallthrough = true;
            }
            if (fallthrough || a == 4L)
            {
                ret++;
                fallthrough = true;
            }
            if (fallthrough || a == 3L)
            {
                ret++;
                fallthrough = true;
            }
            if (fallthrough || a == 2L)
            {
                ret++;
                fallthrough = true;
            }
            if (fallthrough || a == 1L)
            {
                ret++;
                goto __switch_break0;
            }

            __switch_break0:;
            return ret;

        }

        private static void testFallthrough()
        {
            for (long i = 0L; i < 6L; i++)
            {
                {
                    var got = fallthrough_ssa(i);

                    if (got != i)
                    {
                        println("fallthrough_ssa(i) =", got, "wanted", i);
                        failed = true;
                    }

                }
            }

        }

        private static void testSwitch()
        {
            for (long i = 0L; i < 6L; i++)
            {
                {
                    var got = switch_ssa(i);

                    if (got != i)
                    {
                        println("switch_ssa(i) =", got, "wanted", i);
                        failed = true;
                    }

                }
            }

        }

        private partial struct junk
        {
            public long step;
        }

        // flagOverwrite_ssa is intended to reproduce an issue seen where a XOR
        // was scheduled between a compare and branch, clearing flags.
        //go:noinline
        private static long flagOverwrite_ssa(ref junk s, long c)
        {
            if ('0' <= c && c <= '9')
            {
                s.step = 0L;
                return 1L;
            }
            if (c == 'e' || c == 'E')
            {
                s.step = 0L;
                return 2L;
            }
            s.step = 0L;
            return 3L;
        }

        private static void testFlagOverwrite()
        {
            junk j = new junk();
            {
                var got = flagOverwrite_ssa(ref j, ' ');

                if (got != 3L)
                {
                    println("flagOverwrite_ssa =", got, "wanted 3");
                    failed = true;
                }

            }
        }

        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            testPhiControl();
            testEmptyRange();

            testSwitch();
            testFallthrough();

            testFlagOverwrite();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
