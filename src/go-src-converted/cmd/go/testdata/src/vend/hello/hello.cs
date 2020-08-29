// package main -- go2cs converted at 2020 August 29 10:02:04 UTC
// Original source: C:\Go\src\cmd\go\testdata\src\vend\hello\hello.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            fmt.Printf("%s\n", strings.Msg);
        }
    }
}
