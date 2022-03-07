// package errors -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/errors" ==> using errors = go.golang.org.x.tools.go.ssa.interp.testdata.src.errors_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\errors\errors.go


namespace go.golang.org.x.tools.go.ssa.interp.testdata.src;

public static partial class errors_package {

public static error New(@string text) {
    return error.As(new errorString(text))!;
}

private partial struct errorString {
    public @string s;
}

private static @string Error(this errorString e) {
    return e.s;
}

} // end errors_package
