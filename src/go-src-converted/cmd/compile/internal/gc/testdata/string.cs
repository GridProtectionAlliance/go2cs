// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// string_ssa.go tests string operations.
// package main -- go2cs converted at 2020 August 29 09:58:28 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\string.go

using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static @string testStringSlice1_ssa(@string a, long i, long j)
        {
            return a[i..];
        }

        //go:noinline
        private static @string testStringSlice2_ssa(@string a, long i, long j)
        {
            return a[..j];
        }

        //go:noinline
        private static @string testStringSlice12_ssa(@string a, long i, long j)
        {
            return a[i..j];
        }

        private static void testStringSlice()
        {
            foreach (var (i, t) in tests)
            {
                {
                    var got = t.fn(t.s, t.low, t.high);

                    if (t.want != got)
                    {
                        println("#", i, " ", t.s, "[", t.low, ":", t.high, "] = ", got, " want ", t.want);
                        failed = true;
                    }

                }
            }
        }

        private partial struct prefix
        {
            public @string prefix;
        }

        private static void slice_ssa(this ref prefix p)
        {
            p.prefix = p.prefix[..3L];
        }

        //go:noinline
        private static void testStructSlice()
        {
            prefix p = ref new prefix("prefix");
            p.slice_ssa();
            if ("pre" != p.prefix)
            {
                println("wrong field slice: wanted %s got %s", "pre", p.prefix);
                failed = true;
            }
        }

        private static void testStringSlicePanic() => func((defer, _, __) =>
        {
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null)
                    {
                        println("panicked as expected");
                    }

                }
            }());

            @string str = "foobar";
            println("got ", testStringSlice12_ssa(str, 3L, 9L));
            println("expected to panic, but didn't");
            failed = true;
        });

        private static readonly @string _Accuracy_name = "BelowExactAbove";



        private static array<byte> _Accuracy_index = new array<byte>(new byte[] { 0, 5, 10, 15 });

        //go:noinline
        private static @string testSmallIndexType_ssa(long i)
        {
            return _Accuracy_name[_Accuracy_index[i].._Accuracy_index[i + 1L]];
        }

        private static void testSmallIndexType()
        {
            foreach (var (i, t) in tests)
            {
                {
                    var got = testSmallIndexType_ssa(t.i);

                    if (got != t.want)
                    {
                        println("#", i, "got ", got, ", wanted", t.want);
                        failed = true;
                    }

                }
            }
        }

        //go:noinline
        private static byte testInt64Index_ssa(@string s, long i)
        {
            return s[i];
        }

        //go:noinline
        private static @string testInt64Slice_ssa(@string s, long i, long j)
        {
            return s[i..j];
        }

        private static void testInt64Index()
        {
            @string str = "BelowExactAbove";
            foreach (var (i, t) in tests)
            {
                {
                    var got__prev1 = got;

                    var got = testInt64Index_ssa(str, t.i);

                    if (got != t.b)
                    {
                        println("#", i, "got ", got, ", wanted", t.b);
                        failed = true;
                    }

                    got = got__prev1;

                }
                {
                    var got__prev1 = got;

                    got = testInt64Slice_ssa(str, t.i, t.j);

                    if (got != t.s)
                    {
                        println("#", i, "got ", got, ", wanted", t.s);
                        failed = true;
                    }

                    got = got__prev1;

                }
            }
        }

        private static void testInt64IndexPanic() => func((defer, _, __) =>
        {
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null)
                    {
                        println("panicked as expected");
                    }

                }
            }());

            @string str = "foobar";
            println("got ", testInt64Index_ssa(str, 1L << (int)(32L) + 1L));
            println("expected to panic, but didn't");
            failed = true;
        });

        private static void testInt64SlicePanic() => func((defer, _, __) =>
        {
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null)
                    {
                        println("panicked as expected");
                    }

                }
            }());

            @string str = "foobar";
            println("got ", testInt64Slice_ssa(str, 1L << (int)(32L), 1L << (int)(32L) + 1L));
            println("expected to panic, but didn't");
            failed = true;
        });

        //go:noinline
        private static byte testStringElem_ssa(@string s, long i)
        {
            return s[i];
        }

        private static void testStringElem()
        {
            foreach (var (_, t) in tests)
            {
                {
                    var got = testStringElem_ssa(t.s, t.i);

                    if (got != t.n)
                    {
                        print("testStringElem \"", t.s, "\"[", t.i, "]=", got, ", wanted ", t.n, "\n");
                        failed = true;
                    }

                }
            }
        }

        //go:noinline
        private static byte testStringElemConst_ssa(long i)
        {
            @string s = "foobar";
            return s[i];
        }

        private static void testStringElemConst()
        {
            {
                var got = testStringElemConst_ssa(3L);

                if (got != 98L)
                {
                    println("testStringElemConst=", got, ", wanted 98");
                    failed = true;
                }

            }
        }

        private static void Main() => func((_, panic, __) =>
        {
            testStringSlice();
            testStringSlicePanic();
            testStructSlice();
            testSmallIndexType();
            testStringElem();
            testStringElemConst();
            testInt64Index();
            testInt64IndexPanic();
            testInt64SlicePanic();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
