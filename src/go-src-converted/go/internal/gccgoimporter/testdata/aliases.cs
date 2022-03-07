// package aliases -- go2cs converted at 2022 March 06 23:32:49 UTC
// import "go/internal/gccgoimporter.aliases" ==> using aliases = go.go.@internal.gccgoimporter.aliases_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\aliases.go


namespace go.go.@internal;

public static partial class aliases_package {

public partial struct T0 { // : array<nint>
}
public partial struct T1 { // : slice<byte>
}
public partial struct T2 {
    public nint x;
}
public partial interface T3 {
    T2 m();
}
public delegate  channel<T2> T4(nint,  T0);
public partial struct Ai { // : nint
}
public partial struct A0 { // : T0
}
public partial struct A1 { // : T1
}
public partial struct A2 { // : T2
}
public partial struct A3 { // : T3
}
public partial struct A4 { // : T4
}

public partial struct A10 { // : array<nint>
}
public partial struct A11 { // : slice<byte>
}
public partial struct A12 {
    public nint x;
}
public partial interface A13 {
    A2 m();
}
public delegate  channel<A2> A14(nint,  A0);
public static void m1(this T0 _p0) {
}
public static void m2(this A0 _p0) {
}

// alias receiver types (long type declaration chains)
public partial struct V0 { // : V1
}
public partial struct V1 { // : V2
}
public partial struct V2 { // : V3
}
public partial struct V3 { // : T0
}
public static void n(this V1 _p0) {
}

// cycles
public partial struct C0 {
    public C1 f1;
    public C2 f2;
}

public partial struct C1 { // : ptr<C0>
}
public partial struct C2 { // : C1
}
public partial struct C5 {
    public ptr<C6> f;
}
public partial struct C6 { // : C5
}

} // end aliases_package
