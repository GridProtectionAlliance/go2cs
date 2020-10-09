// package main -- go2cs converted at 2020 October 09 06:03:48 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\static.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Static tests of SSA builder (via the sanity checker).
        // Dynamic semantics are not exercised.
        private static void init()
        { 
            // Regression test for issue 6806.
            var ch = make_channel<long>();
            _ = n;
            private partial struct mybool // : bool
            {
            }
            long x = default;
            mybool y = default;
            _ = x;
            _ = y;

        }

        private static long a = default;

        // Regression test for issue 7840 (covered by SSA sanity checker).
        private static bool bug7840()
        { 
            // This creates a single-predecessor block with a φ-node.
            return false && a == 0L && a == 0L;

        }

        // A blocking select (sans "default:") cannot fall through.
        // Regression test for issue 7022.
        private static long bug7022()
        {
            channel<long> c1 = default;            channel<long> c2 = default;

            return 123L;
            return 456L;
        }

        // Parens should not prevent intrinsic treatment of built-ins.
        // (Regression test for a crash.)
        private static void init()
        {
            _ = (new)(int);
            _ = (make)(typeof(slice<long>), 0L);
        }

        private static void Main()
        {
        }
    }
}
