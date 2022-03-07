// package strings -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/strings" ==> using strings = go.golang.org.x.tools.go.ssa.interp.testdata.src.strings_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\strings\strings.go


namespace go.golang.org.x.tools.go.ssa.interp.testdata.src;

public static partial class strings_package {

public static @string Replace(@string s, @string old, @string @new, nint n);

public static nint Index(@string haystack, @string needle);

public static bool Contains(@string haystack, @string needle) {
    return Index(haystack, needle) >= 0;
}

} // end strings_package
