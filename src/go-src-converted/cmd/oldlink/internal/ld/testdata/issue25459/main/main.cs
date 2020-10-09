// package main -- go2cs converted at 2020 October 09 05:52:42 UTC
// Original source: C:\Go\src\cmd\oldlink\internal\ld\testdata\issue25459\main\main.go
using a = go.cmd.oldlink.@internal.ld.testdata.issue25459.a_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public static long Glob = default;

        private static void Main()
        {
            a.Another();
            Glob += a.ConstIf() + a.CallConstIf();
        }
    }
}
