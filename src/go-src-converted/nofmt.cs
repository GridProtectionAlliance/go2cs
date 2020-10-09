// package b -- go2cs converted at 2020 October 09 06:04:11 UTC
// import "golang.org/x/tools/go/analysis/passes/printf/testdata/src.b" ==> using b = go.golang.org.x.tools.go.analysis.passes.printf.testdata.src.b_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\printf\testdata\src\nofmt\nofmt.go
using big = go.math.big_package;
using testing = go.testing_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace printf {
namespace testdata
{
    public static partial class b_package
    {
        private static void formatBigInt(ptr<testing.T> _addr_t)
        {
            ref testing.T t = ref _addr_t.val;

            t.Logf("%d\n", big.NewInt(4L));
        }
    }
}}}}}}}}}
