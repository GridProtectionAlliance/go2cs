namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct ring {
    internal nint data;
    internal ж<ring> next;
}

internal static void initSelf(this ж<ring> Ꮡr) {
    ref var r = ref Ꮡr.val;

    r.next = Ꮡr;
}

[GoRecv] internal static void linkTo(this ref ring r, ж<ring> Ꮡother) {
    ref var other = ref Ꮡother.val;

    r.next = Ꮡother;
}

internal static ж<ring> advance(this ж<ring> Ꮡr, nint n) {
    ref var r = ref Ꮡr.val;

    var p = Ꮡr;
    for (nint i = 0; i < n; i++) {
        p = p.val.next;
    }
    return p;
}

internal static void Main() {
    var a = Ꮡ(new ring(data: 1));
    a.initSelf();
    a.val.data = 42;
    fmt.Println((~(~a).next).data);
    fmt.Println((~a.advance(5)).data);
    var b = Ꮡ(new ring(data: 2));
    var c = Ꮡ(new ring(data: 3));
    b.linkTo(c);
    fmt.Println((~(~b).next).data);
    c.linkTo(b);
    fmt.Println((~b.advance(2)).data);
}

} // end main_package
