namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct @fixed {
    internal nint n;
}

[GoType] partial interface sizer {
    nint size(@string of);
}

[GoType] partial interface @lock {
    bool held();
}

[GoType("num:int16")] partial struct @short;

[GoType("num:uint32")] partial struct dword;

internal static @short toShort(dword d) {
    return ((@short)(int16)(uint32)d);
}

internal static nint size(this @fixed f, @string of) {
    return f.n + len(of);
}

[GoRecv] internal static void grow(this ref @fixed f, nint by) {
    f.n += by;
}

internal static bool held(this @fixed f) {
    return f.n > 3;
}

internal static sizer std = new @fixed(n: 3);

internal static void Main() {
    fmt.Println(std.size("hello"u8));
    ref var f = ref heap<@fixed>(out var Ꮡf);
    f = new @fixed(n: 1);
    fmt.Println(f.size("ab"u8));
    f.grow(4);
    fmt.Println(f.size(""u8));
    sizer p = new fixedжsizer(Ꮡf);
    fmt.Println(p.size("ptr"u8));
    @lock l = f;
    fmt.Println(l.held());
    @lock lp = new fixedжlock(Ꮡf);
    fmt.Println(lp.held());
    dword d = 40000;
    var s = toShort(d);
    fmt.Println((nint)(int16)s, (nint)(uint32)((dword)(uint32)(int16)s));
}

} // end main_package
