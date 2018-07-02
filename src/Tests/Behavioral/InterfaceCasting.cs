// package main -- go2cs converted at 2018 July 02 12:54:33 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceCasting.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct MyError
        {
            public GoString description;
        }

        public static GoString Error(this MyError err)
        {
            returnfmt.Sprintf("error: %s",err.description)
        }

        // error is an interface - MyError is cast to error interface upon return
        private static error f()
        {
            returnMyError{"foo"}
        }

        private static void Main()
        {
            fmt.Printf("%v\n",f())
        }
    }
}
