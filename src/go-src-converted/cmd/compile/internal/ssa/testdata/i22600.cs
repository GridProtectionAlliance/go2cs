// package main -- go2cs converted at 2020 August 29 09:24:39 UTC
// Original source: C:\Go\src\cmd\compile\internal\ssa\testdata\i22600.go
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void test()
        {
            var (pwd, err) = os.Getwd();
            if (err != null)
            {
                fmt.Println(err);
                os.Exit(1L);
            }
            fmt.Println(pwd);
        }

        private static void Main()
        {
            test();
        }
    }
}
