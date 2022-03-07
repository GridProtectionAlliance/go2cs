// package utf8 -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/unicode/utf8" ==> using utf8 = go.golang.org.x.tools.go.ssa.interp.testdata.src.unicode.utf8_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\unicode\utf8\utf8.go


namespace go.golang.org.x.tools.go.ssa.interp.testdata.src.unicode;

public static partial class utf8_package {

public static (int, nint) DecodeRuneInString(@string _p0);

public static (int, nint) DecodeRune(slice<byte> b) {
    int _p0 = default;
    nint _p0 = default;

    return DecodeRuneInString(string(b));
}

public static readonly char RuneError = '\uFFFD';


} // end utf8_package
