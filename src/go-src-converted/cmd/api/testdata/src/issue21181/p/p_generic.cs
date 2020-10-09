// +build !amd64

// package p -- go2cs converted at 2020 October 09 05:20:07 UTC
// import "cmd/api/testdata/src/issue21181/p" ==> using p = go.cmd.api.testdata.src.issue21181.p_package
// Original source: C:\Go\src\cmd\api\testdata\src\issue21181\p\p_generic.go
using indirect = go.indirect_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace api {
namespace testdata {
namespace src {
namespace issue21181
{
    public static partial class p_package
    {
        private static algo @in = new slice<algo>(new algo[] { {indirect.F} });
    }
}}}}}}
