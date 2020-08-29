// package p2 -- go2cs converted at 2020 August 29 10:02:03 UTC
// import "cmd/go/testdata/src/coverdep2/p2" ==> using p2 = go.cmd.go.testdata.src.coverdep2.p2_package
// Original source: C:\Go\src\cmd\go\testdata\src\coverdep2\p2\p2.go
using p1 = go.coverdep2.p1_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace testdata {
namespace src {
namespace coverdep2
{
    public static partial class p2_package
    {
        public static void F()
        {
            p1.F();
        }
    }
}}}}}}
