namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class main_package {

[GoType("unsafe_package.Pointer")] partial struct stdFunction;

internal static stdFunction handler;

internal static stdFunction other;

[GoType("sync.atomic_package.Uint32")] partial struct counter;

[GoRecv] internal static uint32 Load(this ref counter c) {
    return (Ꮡ((atomic.Uint32)(c))).Load();
}

[GoRecv] internal static void Store(this ref counter c, uint32 v) {
    (Ꮡ((atomic.Uint32)(c))).Store(v);
}

[GoRecv] internal static uint32 Add(this ref counter c, uint32 d) {
    return (Ꮡ((atomic.Uint32)(c))).Add(d);
}

internal static void Main() {
    handler = other;
    fmt.Println(handler == other);
    counter c = default!;
    c.Store(10);
    _ = c.Add(5);
    fmt.Println("defined-type methods compiled");
}

} // end main_package
