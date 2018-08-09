// package main -- go2cs converted at 2018 August 09 01:21:19 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceCasting.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct MyError
        {
            public @string description;
        }

        public static @string Error(this MyError err)
        {
            return fmt.Sprintf("error: %s", err.description);
        }

        // error is an interface - MyError is cast to error interface upon return
        private static error f()
        {
            return MyError{"foo"};
        }

        private static void Main()
        {
            fmt.Printf("%v\n", f()); // error: foo
        }
    }
}
