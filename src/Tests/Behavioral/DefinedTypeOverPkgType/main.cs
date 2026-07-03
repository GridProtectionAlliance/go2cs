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

internal static uint32 Load(this ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    return (Ꮡ((atomic.Uint32)(c))).Load();
}

internal static void Store(this ж<counter> Ꮡc, uint32 v) {
    ref var c = ref Ꮡc.Value;

    (Ꮡ((atomic.Uint32)(c))).Store(v);
}

internal static uint32 Add(this ж<counter> Ꮡc, uint32 d) {
    ref var c = ref Ꮡc.Value;

    return (Ꮡ((atomic.Uint32)(c))).Add(d);
}

internal static void Main() {
    handler = other;
    fmt.Println(handler == other);
    ref var c = ref heap(new counter(), out var Ꮡc);
    Ꮡc.Store(10);
    _ = Ꮡc.Add(5);
    fmt.Println("defined-type methods compiled");
}

} // end main_package
