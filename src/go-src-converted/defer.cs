// package main -- go2cs converted at 2020 October 09 06:03:45 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\defer.go
// Tests of defer.  (Deferred recover() belongs is recover.go.)

using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static (long, long) deferMutatesResults(bool noArgReturn) => func((defer, panic, _) =>
        {
            long a = default;
            long b = default;

            defer(() =>
            {
                if (a != 1L || b != 2L)
                {
                    panic(fmt.Sprint(a, b));
                }
                a = 3L;
                b = 4L;

            }());
            if (noArgReturn)
            {
                a = 1L;
                b = 2L;
                return ;

            }
            return (1L, 2L);

        });

        private static void init() => func((_, panic, __) =>
        {
            var (a, b) = deferMutatesResults(true);
            if (a != 3L || b != 4L)
            {
                panic(fmt.Sprint(a, b));
            }

            a, b = deferMutatesResults(false);
            if (a != 3L || b != 4L)
            {
                panic(fmt.Sprint(a, b));
            }

        });

        // We concatenate init blocks to make a single function, but we must
        // run defers at the end of each block, not the combined function.
        private static long deferCount = 0L;

        private static void init() => func((_, panic, __) =>
        {
            deferCount = 1L;
            defer(() =>
            {
                deferCount++;
            }()); 
            // defer runs HERE
        });

        private static void init() => func((_, panic, __) =>
        { 
            // Strictly speaking the spec says deferCount may be 0 or 2
            // since the relative order of init blocks is unspecified.
            if (deferCount != 2L)
            {
                panic(deferCount); // defer call has not run!
            }

        });

        private static void Main()
        {
        }
    }
}
