// package p1 -- go2cs converted at 2020 August 29 10:02:05 UTC
// import "cmd/go/testdata/src/vetfail/p1" ==> using p1 = go.cmd.go.testdata.src.vetfail.p1_package
// Original source: C:\Go\src\cmd\go\testdata\src\vetfail\p1\p1.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace testdata {
namespace src {
namespace vetfail
{
    public static partial class p1_package
    {
        public static void F()
        {
            fmt.Printf("%d", "hello"); // causes vet error
        }
    }
}}}}}}
