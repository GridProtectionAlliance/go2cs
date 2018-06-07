// package main -- go2cs converted at 2018 June 07 01:54:01 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ImportOptions.go

using fmt = go.fmt_package;
using _math_ = go.math_package;
using static go.math.rand_package;
using os = go.os_package;
using @implicit = go.text.tabwriter_package;

using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            fmt.Println(Int())w:=implicit.NewWriter(os.Stdout,1,1,1,' ',0)deferw.Flush()
        });
    }
}
