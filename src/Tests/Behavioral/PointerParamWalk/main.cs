namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sumWalk(ж<node> Ꮡp, nint steps) {
    ref var p = ref Ꮡp.val;

    nint total = 0;
    for (nint i = 0; i < steps; i++) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.val;
    }
    return total;
}

internal static ж<node> firstAfter(ж<node> Ꮡp, nint steps) {
    ref var p = ref Ꮡp.val;

    for (nint i = 0; i < steps; i++) {
        Ꮡp = p.next; p = ref Ꮡp.val;
    }
    return Ꮡp;
}

internal static void Main() {
    var a = Ꮡ(new node(val: 1));
    var b = Ꮡ(new node(val: 2));
    var c = Ꮡ(new node(val: 3));
    a.val.next = b;
    b.val.next = c;
    c.val.next = a;
    fmt.Println(sumWalk(a, 6));
    fmt.Println((~firstAfter(a, 4)).val);
}

} // end main_package
