// package main -- go2cs converted at 2020 October 09 06:03:46 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\initorder.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Test of initialization order of package-level vars.
        private static long counter = default;

        private static long next()
        {
            var c = counter;
            counter++;
            return c;
        }

        private static (long, long) next2()
        {
            long x = default;
            long y = default;

            x = next();
            y = next();
            return ;
        }

        private static long makeOrder()
        {
            _ = f;
            _ = b;
            _ = d;
            _ = e;
            return 0L;

        }

        private static void Main() => func((_, panic, __) =>
        { 
            // Initialization constraints:
            // - {f,b,c/d,e} < order  (ref graph traversal)
            // - order < {a}          (lexical order)
            // - b < c/d < e < f      (lexical order)
            // Solution: a b c/d e f
            array<long> abcdef = new array<long>(new long[] { a, b, c, d, e, f });
            if (abcdef != new array<long>(new long[] { 0, 1, 2, 3, 4, 5 }))
            {
                panic(abcdef);
            }

        });

        private static var order = makeOrder();

        private static var a = next();        private static var b = next();


        private static var e = next();        private static var f = next();

        // ------------------------------------------------------------------------



        // ------------------------------------------------------------------------

        private static slice<@string> order2 = default;

        private static long create(long x, @string name)
        {
            order2 = append(order2, name);
            return x;
        }

        public static var C = create(B + 1L, "C");
        public static var A = create(1L, "A");        public static var B = create(2L, "B");

        // Initialization order of package-level value specs.


        // Initialization order of package-level value specs.
        private static void init() => func((_, panic, __) =>
        {
            var x = fmt.Sprint(order2); 
            // Result varies by toolchain.  This is a spec bug.
            if (x != "[B C A]" && x != "[A B C]")
            { // go/types
                panic(x);

            }

            if (C != 3L)
            {
                panic(c);
            }

        });
    }
}
