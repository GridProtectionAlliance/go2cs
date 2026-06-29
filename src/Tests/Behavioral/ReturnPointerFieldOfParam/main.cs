namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal ж<node> next;
    internal nint val;
}

internal static ж<node> advance(ж<node> Ꮡn) {
    ref var n = ref Ꮡn.val;

    return n.next;
}

internal static ж<node> self(ж<node> Ꮡn) {
    ref var n = ref Ꮡn.val;

    return Ꮡn;
}

internal static void Main() {
    var c = Ꮡ(new node(val: 3));
    var b = Ꮡ(new node(next: c, val: 2));
    var a = Ꮡ(new node(next: b, val: 1));
    fmt.Println((~advance(a)).val);
    fmt.Println((~advance(advance(a))).val);
    fmt.Println((~self(a)).val);
    fmt.Println(advance(c) == nil);
}

} // end main_package
