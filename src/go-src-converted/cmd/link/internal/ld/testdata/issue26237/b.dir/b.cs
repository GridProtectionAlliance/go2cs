// package b -- go2cs converted at 2022 March 06 23:22:34 UTC
// import "cmd/link/internal/ld/testdata/issue26237.b" ==> using b = go.cmd.link.@internal.ld.testdata.issue26237.b_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue26237\b.dir\b.go

using System;


namespace go.cmd.link.@internal.ld.testdata;

public static partial class b_package {

private static nint q = default;

public static nint Top(nint x) {
    q += 1;
    if (q != x) {
        return 3;
    }
    return 4;

}

public static nint OOO(nint x) => func((defer, _, _) => {
    defer(() => {
        q += x & 7;
    }());
    return Top(x + 1);

});

} // end b_package
