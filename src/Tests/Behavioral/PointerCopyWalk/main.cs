namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static ж<node> last(ж<node> Ꮡstart) {
    ref var start = ref Ꮡstart.val;

    var r = Ꮡstart;
    while ((~r).next != nil) {
        r = r.val.next;
    }
    return r;
}

internal static nint length(ж<node> Ꮡstart) {
    ref var start = ref Ꮡstart.val;

    nint n = 0;
    for (var r = Ꮡstart; r != nil; r = r.val.next) {
        n++;
    }
    return n;
}

internal static nint sum(ж<node> Ꮡstart) {
    ref var start = ref Ꮡstart.val;

    nint total = 0;
    var p = Ꮡstart;
    while (p != nil) {
        total += p.val.val;
        p = p.val.next;
    }
    return total;
}

internal static void Main() {
    var third = Ꮡ(new node(val: 3));
    var second = Ꮡ(new node(val: 2, next: third));
    var first = Ꮡ(new node(val: 1, next: second));
    fmt.Println((~last(first)).val);
    fmt.Println(length(first));
    fmt.Println(sum(first));
}

} // end main_package
