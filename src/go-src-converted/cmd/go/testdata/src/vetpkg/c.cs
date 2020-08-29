// +build tagtest

// package p -- go2cs converted at 2020 August 29 10:02:05 UTC
// import "cmd/go/testdata/src.p" ==> using p = go.cmd.go.testdata.src.p_package
// Original source: C:\Go\src\cmd\go\testdata\src\vetpkg\c.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace testdata
{
    public static partial class p_package
    {
        private static void g()
        {
            fmt.Printf("%d", 3L, 4L);
        }
    }
}}}}
