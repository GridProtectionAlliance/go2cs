// package math -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/math" ==> using math = go.golang.org.x.tools.go.ssa.interp.testdata.src.math_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\math\math.go


namespace go.golang.org.x.tools.go.ssa.interp.testdata.src;

public static partial class math_package {

public static double NaN();

public static double Inf(nint _p0);

public static bool IsNaN(double _p0);

public static ulong Float64bits(double _p0);

public static bool Signbit(double x) {
    return Float64bits(x) & (1 << 63) != 0;
}

} // end math_package
