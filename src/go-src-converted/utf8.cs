// package utf8 -- go2cs converted at 2020 October 08 04:57:34 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/unicode/utf8" ==> using utf8 = go.golang.org.x.tools.go.ssa.interp.testdata.src.unicode.utf8_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\unicode\utf8\utf8.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ssa {
namespace interp {
namespace testdata {
namespace src {
namespace unicode
{
    public static partial class utf8_package
    {
        public static (int, long) DecodeRuneInString(@string _p0)
;

        public static (int, long) DecodeRune(slice<byte> b)
        {
            int _p0 = default;
            long _p0 = default;

            return DecodeRuneInString(string(b));
        }

        public static readonly char RuneError = (char)'\uFFFD';

    }
}}}}}}}}}}
