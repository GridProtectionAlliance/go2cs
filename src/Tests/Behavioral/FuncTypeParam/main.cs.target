namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static nint apply(ж<atomic.Int32> Ꮡv, Func<ж<atomic.Int32>, nint> f) {
    ref var v = ref Ꮡv.val;

    return f(Ꮡv);
}

internal static void consume(ж<atomic.Int32> Ꮡv, Action<ж<atomic.Int32>> sink) {
    ref var v = ref Ꮡv.val;

    sink(Ꮡv);
}

internal static void Main() {
    ref var v = ref heap(new atomic.Int32(), out var Ꮡv);
    Ꮡv.Store(10);
    nint n = apply(Ꮡv, (ж<atomic.Int32> x) => ((nint)x.Load()) + 5);
    fmt.Println(n);
    consume(Ꮡv, (ж<atomic.Int32> x) => {
        x.Store(99);
    });
    fmt.Println(Ꮡv.Load());
}

} // end main_package
