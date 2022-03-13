// package a -- go2cs converted at 2022 March 13 06:35:38 UTC
// import "cmd/link/internal/ld/testdata/issue25459/a" ==> using a = go.cmd.link.@internal.ld.testdata.issue25459.a_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue25459\a\a.go
namespace go.cmd.link.@internal.ld.testdata.issue25459;

using System;
public static partial class a_package {

public static readonly var Always = true;



public static nint Count = default;

public delegate nint FuncReturningInt();

public static FuncReturningInt PointerToConstIf = default;

public static nint ConstIf() {
    if (Always) {
        return 1;
    }
    array<nint> imdead = new array<nint>(4);
    imdead[Count] = 1;
    return imdead[0];
}

public static nint CallConstIf() {
    Count += 3;
    return ConstIf();
}

public static void Another() => func((defer, _, _) => {
    defer(() => {
        PointerToConstIf = ConstIf;

        Count += 1;
    }());
});

} // end a_package
