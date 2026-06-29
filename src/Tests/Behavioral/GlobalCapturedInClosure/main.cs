namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct heap {
    internal nint count;
}

[GoRecv] internal static nint alloc(this ref heap h) {
    h.count++;
    return h.count;
}

internal static ж<heap> Ꮡmheap = new(default(heap));
internal static ref heap mheap => ref Ꮡmheap.val;

internal static void keep(ж<heap> Ꮡh) {
    ref var h = ref Ꮡh.val;

    _ = Ꮡh;
}

internal static void run(Action f) {
    f();
}

internal static void Main() {
    keep(Ꮡmheap);
    nint got = default!;
    run(() => {
        got = mheap.alloc();
        var p = Ꮡmheap.of(heap.Ꮡcount);
        p.val += 10;
    });
    fmt.Println(got, mheap.count);
}

} // end main_package
