namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer noescape(@unsafe.Pointer p) {
    var x = (uintptr)p;
    return (@unsafe.Pointer)((uintptr)(x ^ 0));
}

[GoType] partial struct builder {
    internal ж<builder> addr;
    internal slice<byte> buf;
}

internal static void copyCheck(this ж<builder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNil();

    if (b.addr == nil){
        b.addr = Ꮡb;
    } else 
    if (b.addr != Ꮡb) {
        throw panic("builder: illegal use of non-zero builder copied by value");
    }
}

internal static void write(this ж<builder> Ꮡb, @string s) {
    ref var b = ref Ꮡb.Value;

    Ꮡb.copyCheck();
    b.buf = append(b.buf, s.ꓸꓸꓸ);
}

[GoRecv] internal static @string String(this ref builder b) {
    return ((@string)b.buf);
}

internal static void Main() {
    ref var b = ref heap(new builder(), out var Ꮡb);
    Ꮡb.write("hello"u8);
    Ꮡb.write(", "u8);
    Ꮡb.write("world"u8);
    fmt.Println(b.String());
    ((Action)(() => func((defer, recover) => {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println("caught:", r);
                }
            }
        });
        ref var src = ref heap(new builder(), out var Ꮡsrc);
        Ꮡsrc.write("seed"u8);
        ref var cp = ref heap<builder>(out var Ꮡcp);
        cp = src;
        Ꮡcp.write("more"u8);
        fmt.Println("unreachable");
    })))();
}

} // end main_package
