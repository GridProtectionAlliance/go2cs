// package main -- go2cs converted at 2018 August 06 14:33:08 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\ImportOptions.go
using fmt = go.fmt_package;
using _math_ = go.math_package;
using _file_ = go.path.file_package;
using static go.math.rand_package;
using os = go.os_package;
using @implicit = go.text.tabwriter_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main() => func((defer, _, _) =>
        {
            fmt.Println(Int());
            var w = @implicit.NewWriter(os.Stdout, 1, 1, 1, ' ', 0);
            defer(w.Flush());
        });
    }
}
