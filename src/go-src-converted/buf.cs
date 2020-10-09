// Test of examples with divergent packages.

// Package buf ...
// package buf -- go2cs converted at 2020 October 09 06:04:18 UTC
// import "golang.org/x/tools/go/analysis/passes/tests/testdata/src.buf" ==> using buf = go.golang.org.x.tools.go.analysis.passes.tests.testdata.src.buf_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\tests\testdata\src\divergent\buf.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace tests {
namespace testdata
{
    public static partial class buf_package
    {
        // Buf is a ...
        public partial struct Buf // : slice<byte>
        {
        }

        // Append ...
        private static void Append(this ptr<Buf> _addr__p0, slice<byte> _p0)
        {
            ref Buf _p0 = ref _addr__p0.val;

        }

        public static void Reset(this Buf _p0)
        {
        }

        public static long Len(this Buf _p0)
        {
            return 0L;
        }

        // DefaultBuf is a ...
        public static Buf DefaultBuf = default;
    }
}}}}}}}}}
