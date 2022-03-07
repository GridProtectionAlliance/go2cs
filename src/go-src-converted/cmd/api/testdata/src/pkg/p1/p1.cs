// package p1 -- go2cs converted at 2022 March 06 22:43:01 UTC
// import "cmd/api/testdata/src/pkg/p1" ==> using p1 = go.cmd.api.testdata.src.pkg.p1_package
// Original source: C:\Program Files\Go\src\cmd\api\testdata\src\pkg\p1\p1.go
using ptwo = go.p2_package;
using System;


namespace go.cmd.api.testdata.src.pkg;

public static partial class p1_package {

public static readonly var ConstChase2 = constChase; // forward declaration to unexported ident
private static readonly var constChase = AIsLowerA; // forward declaration to exported ident

public static readonly nint A = 1;
private static readonly nint a = 11;
public static readonly long A64 = 1;

public static readonly var AIsLowerA = a; // previously declared

public static readonly var ConversionConst = MyInt(5);


// Variables from function calls.
public static var V = ptwo.F();public static var VError = BarE();public static var V1 = Bar1(1, 2, 3);public static var V2 = ptwo.G();

// Variables with conversions:
public static var StrConv = string("foo");public static slice<byte> ByteConv = (slice<byte>)"foo";

public static var ChecksumError = ptwo.NewError("gzip checksum error");

public static readonly nint B0 = 2;

public static readonly @string StrConst = "foo";

public static readonly float FloatConst = 1.5F;



private partial struct myInt { // : nint
}

public partial struct MyInt { // : nint
}

public partial struct Time {
}

public partial struct S {
    public ptr<nint> Public;
    public ptr<nint> @private;
    public Time PublicTime;
}

public partial struct URL {
}

public partial struct EmbedURLPtr {
    public ref ptr<URL> ptr<URL> => ref ptr<URL>_ptr;
}

public partial struct S2 {
    public ref S S => ref S_val;
    public bool Extra;
}

public static long X0 = default;

public static nint Y = default;public static I X = default!;

public partial interface Namer {
    @string Name();
}

public partial interface I {
    long Set(@string name, long balance);
    long Get(@string _p0);
    long GetNamed(@string _p0);
    long @private();
}

public partial interface Public {
    void X();
    void Y();
}

public partial interface Private {
    void X();
    void y();
}

public partial interface Error {
    bool Temporary();
}

private static void privateTypeMethod(this myInt _p0) {
}
private static void CapitalMethodUnexportedType(this myInt _p0) {
}

private static void SMethod(this ptr<S2> _addr_s, sbyte x, short y, long z) {
    ref S2 s = ref _addr_s.val;

}

private partial struct s {
}

private static void method(this s _p0)
private static void Method(this s _p0)

public static void StructValueMethod(this S _p0)
public static void StructValueMethodNamedRecv(this S ignored)

private static void unexported(this ptr<S2> _addr_s, sbyte x, short y, long z) {
    ref S2 s = ref _addr_s.val;

}

public static void Bar(sbyte x, short y, long z) {
}
public static ulong Bar1(sbyte x, short y, long z) {
}
public static (byte, ulong) Bar2(sbyte x, short y, long z) {
    byte _p0 = default;
    ulong _p0 = default;

}
public static Error BarE() {
}

private static void unexported(sbyte x, short y, long z) {
}

public static nint TakesFunc(Func<nint, nint> f);

public partial struct Codec {
    public Func<nint, nint, nint> Func;
}

public partial struct SI {
    public nint I;
}

public static SI SIVal = new SI();
public static ptr<SI> SIPtr = addr(new SI());
public static ptr<SI> SIPtr2;

public partial struct T {
    public ref common common => ref common_val;
}

public partial struct B {
    public ref common common => ref common_val;
}

private partial struct common {
    public nint i;
}

public partial struct TPtrUnexported {
    public ref ptr<common> ptr<common> => ref ptr<common>_ptr;
}

public partial struct TPtrExported {
    public ref ptr<Embedded> ptr<Embedded> => ref ptr<Embedded>_ptr;
}

public delegate  error) FuncType(nint,  nint,  @string,  (ptr<B>);

public partial struct Embedded {
}

public static (ptr<B>, error) PlainFunc(nint x, nint y, @string s);

private static void OnEmbedded(this ptr<Embedded> _addr__p0) {
    ref Embedded _p0 = ref _addr__p0.val;

}

private static void JustOnT(this ptr<T> _addr__p0) {
    ref T _p0 = ref _addr__p0.val;

}
private static void JustOnB(this ptr<B> _addr__p0) {
    ref B _p0 = ref _addr__p0.val;

}
private static void OnBothTandBPtr(this ptr<common> _addr__p0) {
    ref common _p0 = ref _addr__p0.val;

}
private static void OnBothTandBVal(this common _p0) {
}

public partial struct EmbedSelector {
    public ref Time Time => ref Time_val;
}

private static readonly @string foo = "foo";
private static readonly @string foo2 = "foo2";
private static readonly var truth = foo == "foo" || foo2 == "foo2";


private static void ellipsis(params @string _p0) {
}

public static Time Now() {
    Time now = default;
    return now;
}

private static ptr<S> x = addr(new S(Public:nil,private:nil,PublicTime:Now(),));

private static nint parenExpr = (1 + 5);

private static Action funcLit = () => {>>MARKER:FUNCTION_PlainFunc_BLOCK_PREFIX<<
};

private static map<@string, nint> m = default;

private static channel<nint> chanVar = default;

private static nint ifaceVar = 5;

private static nint assertVar = ifaceVar._<nint>();

private static var indexVar = m["foo"];

public static byte Byte = default;
public static Func<byte, int> ByteFunc = default;

public partial struct ByteStruct {
    public byte B;
    public int R;
}

} // end p1_package
