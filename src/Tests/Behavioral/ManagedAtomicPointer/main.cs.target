namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class main_package {

[GoType] partial struct proc {
    internal nint addr;
}

[GoType] partial struct lazyProc {
    internal ж<proc> p;
}

internal static bool find(this ж<lazyProc> Ꮡl) {
    if (atomic.LoadPointer(Ꮡl.of(lazyProc.Ꮡp)) == nil) {
        atomic.StorePointer(Ꮡl.of(lazyProc.Ꮡp), Ꮡ(new proc(addr: 42)));
        return true;
    }
    return false;
}

internal static void Main() {
    ref var l = ref heap(new lazyProc(), out var Ꮡl);
    fmt.Println(Ꮡl.find());
    fmt.Println(Ꮡl.find());
    fmt.Println((~l.p).addr);
}

} // end main_package
