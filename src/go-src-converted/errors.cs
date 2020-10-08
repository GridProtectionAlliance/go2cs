// package errors -- go2cs converted at 2020 October 08 04:57:34 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/errors" ==> using errors = go.golang.org.x.tools.go.ssa.interp.testdata.src.errors_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\errors\errors.go

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
    public static partial class errors_package
    {
        public static error New(@string text)
        {
            return error.As(new errorString(text))!;
        }

        private partial struct errorString
        {
            public @string s;
        }

        private static @string Error(this errorString e)
        {
            return e.s;
        }
    }
}}}}}}}}}
