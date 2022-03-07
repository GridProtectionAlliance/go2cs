// Test of examples with divergent packages.

// Package buf ...
// package buf -- go2cs converted at 2022 March 06 23:34:16 UTC
// import "golang.org/x/tools/go/analysis/passes/tests/testdata/src.buf" ==> using buf = go.golang.org.x.tools.go.analysis.passes.tests.testdata.src.buf_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\tests\testdata\src\divergent\buf.go


namespace go.golang.org.x.tools.go.analysis.passes.tests.testdata;

public static partial class buf_package {

    // Buf is a ...
public partial struct Buf { // : slice<byte>
}

// Append ...
private static void Append(this ptr<Buf> _addr__p0, slice<byte> _p0) {
    ref Buf _p0 = ref _addr__p0.val;

}

public static void Reset(this Buf _p0) {
}

public static nint Len(this Buf _p0) {
    return 0;
}

// DefaultBuf is a ...
public static Buf DefaultBuf = default;

} // end buf_package
