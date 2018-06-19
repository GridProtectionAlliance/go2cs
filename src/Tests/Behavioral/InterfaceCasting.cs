// package main -- go2cs converted at 2018 June 19 13:39:31 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceCasting.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct MyError
        {
            public string description;
        }

        public static string Error(this MyError err) => func((defer, panic, recover) =>
        {
            returnfmt.Sprintf("error: %s",err.description)
        });

        // error is an interface - MyError is cast to error interface upon return
        private static error f() => func((defer, panic, recover) =>
        {
            returnMyError{"foo"}
        });

        private static void Main() => func((defer, panic, recover) =>
        {
            fmt.Printf("%v\n",f())
        });
    }
}
