// Tests of call chaining f(g()) when g has multiple return values (MRVs).
// See https://code.google.com/p/go/issues/detail?id=4573.

// package main -- go2cs converted at 2020 October 08 04:57:33 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\mrvchain.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void assert(long actual, long expected) => func((_, panic, __) =>
        {
            if (actual != expected)
            {
                panic(actual);
            }
        });

        private static (long, long) g()
        {
            long _p0 = default;
            long _p0 = default;

            return (5L, 7L);
        }

        private static (double, double) g2()
        {
            double _p0 = default;
            double _p0 = default;

            return (5L, 7L);
        }

        private static void f1v(long x, params long[] v)
        {
            v = v.Clone();

            assert(x, 5L);
            assert(v[0L], 7L);
        }

        private static void f2(long x, long y)
        {
            assert(x, 5L);
            assert(y, 7L);
        }

        private static void f2v(long x, long y, params long[] v)
        {
            v = v.Clone();

            assert(x, 5L);
            assert(y, 7L);
            assert(len(v), 0L);
        }

        private static (double, double) complexArgs()
        {
            double _p0 = default;
            double _p0 = default;

            return (5L, 7L);
        }

        private static (slice<@string>, @string) appendArgs()
        {
            slice<@string> _p0 = default;
            @string _p0 = default;

            return (new slice<@string>(new @string[] { "foo" }), "bar");
        }

        private static (object, bool) h()
        {
            object i = default;
            bool ok = default;

            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, @string>{1:"hi"};
            i, ok = m[1L]; // string->interface{} conversion within multi-valued expression
            return ;

        }

        private static (object, bool) h2()
        {
            object i = default;
            bool ok = default;

            var ch = make_channel<@string>(1L);
            ch.Send("hi");
            i, ok = ch.Receive(); // string->interface{} conversion within multi-valued expression
            return ;

        }

        private static void Main() => func((_, panic, __) =>
        {
            f1v(g());
            f2(g());
            f2v(g());
            {
                var c = complex(complexArgs());

                if (c != 5L + 7iUL)
                {
                    panic(c);
                }

            }

            {
                var s = append(appendArgs());

                if (len(s) != 2L || s[0L] != "foo" || s[1L] != "bar")
                {
                    panic(s);
                }

            }

            var (i, ok) = h();
            if (!ok || i._<@string>() != "hi")
            {
                panic(i);
            }

            i, ok = h2();
            if (!ok || i._<@string>() != "hi")
            {
                panic(i);
            }

        });
    }
}
