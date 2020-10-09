// package strings -- go2cs converted at 2020 October 09 06:03:48 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/strings" ==> using strings = go.golang.org.x.tools.go.ssa.interp.testdata.src.strings_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\strings\strings.go

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
    public static partial class strings_package
    {
        public static @string Replace(@string s, @string old, @string @new, long n)
;

        public static long Index(@string haystack, @string needle)
;

        public static bool Contains(@string haystack, @string needle)
        {
            return Index(haystack, needle) >= 0L;
        }
    }
}}}}}}}}}
