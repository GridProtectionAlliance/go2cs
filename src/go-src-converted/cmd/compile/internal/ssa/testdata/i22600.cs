// package main -- go2cs converted at 2020 October 09 05:39:57 UTC
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
            growstack(); // Use stack early to prevent growth during test, which confuses gdb
            test();

        }

        private static @string snk = default;

        //go:noinline
        private static void growstack()
        {
            snk = fmt.Sprintf("%#v,%#v,%#v", 1L, true, "cat");
        }
    }
}
