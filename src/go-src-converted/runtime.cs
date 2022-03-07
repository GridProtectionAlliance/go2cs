// package runtime -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/runtime" ==> using runtime = go.golang.org.x.tools.go.ssa.interp.testdata.src.runtime_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\runtime\runtime.go


namespace go.golang.org.x.tools.go.ssa.interp.testdata.src;

public static partial class runtime_package {

    // An errorString represents a runtime error described by a single string.
private partial struct errorString { // : @string
}

private static void RuntimeError(this errorString e) {
}

private static @string Error(this errorString e) {
    return "runtime error: " + string(e);
}

public static void Breakpoint();

public partial interface Error {
    void RuntimeError();
}

public static readonly @string GOOS = "linux";

public static readonly @string GOARCH = "amd64";



public static void GC();

} // end runtime_package
