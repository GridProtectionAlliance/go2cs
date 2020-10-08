// package runtime -- go2cs converted at 2020 October 08 04:57:34 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/runtime" ==> using runtime = go.golang.org.x.tools.go.ssa.interp.testdata.src.runtime_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\runtime\runtime.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ssa {
namespace interp {
namespace testdata {
namespace src
{
    public static partial class runtime_package
    {
        // An errorString represents a runtime error described by a single string.
        private partial struct errorString // : @string
        {
        }

        private static void RuntimeError(this errorString e)
        {
        }

        private static @string Error(this errorString e)
        {
            return "runtime error: " + string(e);
        }

        public static void Breakpoint()
;

        public partial interface Error : error
        {
            void RuntimeError();
        }

        public static readonly @string GOOS = (@string)"linux";

        public static readonly @string GOARCH = (@string)"amd64";



        public static void GC()
;
    }
}}}}}}}}}
