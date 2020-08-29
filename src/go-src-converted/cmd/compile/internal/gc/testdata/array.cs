// package main -- go2cs converted at 2020 August 29 09:57:35 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\array.go

using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static (long, long) testSliceLenCap12_ssa(array<long> a, long i, long j)
        {
            a = a.Clone();

            var b = a[i..j];
            return (len(b), cap(b));
        }

        //go:noinline
        private static (long, long) testSliceLenCap1_ssa(array<long> a, long i, long j)
        {
            a = a.Clone();

            var b = a[i..];
            return (len(b), cap(b));
        }

        //go:noinline
        private static (long, long) testSliceLenCap2_ssa(array<long> a, long i, long j)
        {
            a = a.Clone();

            var b = a[..j];
            return (len(b), cap(b));
        }

        private static void testSliceLenCap()
        {
            array<long> a = new array<long>(new long[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            foreach (var (i, t) in tests)
            {
                {
                    var (l, c) = t.fn(a, t.i, t.j);

                    if (l != t.l && c != t.c)
                    {
                        println("#", i, " len(a[", t.i, ":", t.j, "]), cap(a[", t.i, ":", t.j, "]) =", l, c, ", want", t.l, t.c);
                        failed = true;
                    }

                }
            }
        }

        //go:noinline
        private static long testSliceGetElement_ssa(array<long> a, long i, long j, long p)
        {
            a = a.Clone();

            return a[i..j][p];
        }

        private static void testSliceGetElement()
        {
            array<long> a = new array<long>(new long[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 });
            foreach (var (i, t) in tests)
            {
                {
                    var got = testSliceGetElement_ssa(a, t.i, t.j, t.p);

                    if (got != t.want)
                    {
                        println("#", i, " a[", t.i, ":", t.j, "][", t.p, "] = ", got, " wanted ", t.want);
                        failed = true;
                    }

                }
            }
        }

        //go:noinline
        private static void testSliceSetElement_ssa(ref array<long> a, long i, long j, long p, long x)
        {
            (a.Value)[i..j][p] = x;
        }

        private static void testSliceSetElement()
        {
            array<long> a = new array<long>(new long[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 });
            foreach (var (i, t) in tests)
            {
                testSliceSetElement_ssa(ref a, t.i, t.j, t.p, t.want);
                {
                    var got = a[t.i + t.p];

                    if (got != t.want)
                    {
                        println("#", i, " a[", t.i, ":", t.j, "][", t.p, "] = ", got, " wanted ", t.want);
                        failed = true;
                    }

                }
            }
        }

        private static void testSlicePanic1() => func((defer, _, __) =>
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

            array<long> a = new array<long>(new long[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 });
            testSliceLenCap12_ssa(a, 3L, 12L);
            println("expected to panic, but didn't");
            failed = true;
        });

        private static void testSlicePanic2() => func((defer, _, __) =>
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

            array<long> a = new array<long>(new long[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 });
            testSliceGetElement_ssa(a, 3L, 7L, 4L);
            println("expected to panic, but didn't");
            failed = true;
        });

        private static void Main() => func((_, panic, __) =>
        {
            testSliceLenCap();
            testSliceGetElement();
            testSliceSetElement();
            testSlicePanic1();
            testSlicePanic2();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
