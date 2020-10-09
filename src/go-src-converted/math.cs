// package math -- go2cs converted at 2020 October 09 06:03:48 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/math" ==> using math = go.golang.org.x.tools.go.ssa.interp.testdata.src.math_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\math\math.go

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
    public static partial class math_package
    {
        public static double NaN()
;

        public static double Inf(long _p0)
;

        public static bool IsNaN(double _p0)
;

        public static ulong Float64bits(double _p0)
;

        public static bool Signbit(double x)
        {
            return Float64bits(x) & (1L << (int)(63L)) != 0L;
        }
    }
}}}}}}}}}
