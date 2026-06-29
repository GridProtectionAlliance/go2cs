namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

[GoType] partial struct counters {
    internal array<sync.atomic_package.Int32> c = new(3);
    internal array<sync.atomic_package.Uint64> d = new(2);
}

internal static void touch(ж<atomic.Int32> Ꮡp) {
    ref var p = ref Ꮡp.val;

}

internal static void Main() {
    ref var x = ref heap(new counters(), out var Ꮡx);
    touch(Ꮡx.of(counters.Ꮡc).at<sync.atomic_package.Int32>(0));
    touch(Ꮡx.of(counters.Ꮡc).at<sync.atomic_package.Int32>(2));
    fmt.Println(len(x.c), len(x.d));
}

} // end main_package
