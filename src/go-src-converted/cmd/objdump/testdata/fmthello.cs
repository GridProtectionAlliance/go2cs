// package main -- go2cs converted at 2020 August 29 10:04:45 UTC
// Original source: C:\Go\src\cmd\objdump\testdata\fmthello.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            Println("hello, world");
        }

        //go:noinline
        public static void Println(@string s)
        {
            fmt.Println(s);
        }
    }
}
