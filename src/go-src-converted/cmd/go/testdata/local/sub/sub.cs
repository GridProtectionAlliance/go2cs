// package sub -- go2cs converted at 2020 August 29 10:02:02 UTC
// import "cmd/go/testdata/local/sub" ==> using sub = go.cmd.go.testdata.local.sub_package
// Original source: C:\Go\src\cmd\go\testdata\local\sub\sub.go
using fmt = go.fmt_package;

using subsub = go...sub_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace testdata {
namespace local
{
    public static partial class sub_package
    {
        public static void Hello()
        {
            fmt.Println("sub.Hello");
            subsub.Hello();
        }
    }
}}}}}
