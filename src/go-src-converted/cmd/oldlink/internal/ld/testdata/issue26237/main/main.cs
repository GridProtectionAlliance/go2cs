// package main -- go2cs converted at 2020 October 08 04:42:06 UTC
// Original source: C:\Go\src\cmd\oldlink\internal\ld\testdata\issue26237\main\main.go
using fmt = go.fmt_package;

using b = go.cmd.oldlink.@internal.ld.testdata.issue26237.b.dir_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long skyx = default;

        private static void Main()
        {
            skyx += b.OOO(skyx);
            if (b.Top(1L) == 99L)
            {
                fmt.Printf("Beware the Jabberwock, my son!\n");
            }

        }
    }
}
