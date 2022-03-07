// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue25459\main\main.go
using a = go.cmd.link.@internal.ld.testdata.issue25459.a_package;

namespace go;

public static partial class main_package {

public static nint Glob = default;

private static void Main() {
    a.Another();
    Glob += a.ConstIf() + a.CallConstIf();
}

} // end main_package
