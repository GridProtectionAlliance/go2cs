namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sumList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNil();

    nint total = 0;
    while (Ꮡp != nil) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNil();
    }
    return total;
}

internal static void doubleList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNil();

    while (Ꮡp != nil) {
        p.val *= 2;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNil();
    }
}

internal static ж<node> build(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.slice();

    ж<node> head = default!;
    for (nint i = len(vals) - 1; i >= 0; i--) {
        head = Ꮡ(new node(val: vals[i], next: head));
    }
    return head;
}

internal static (ж<node>, nint) advance(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.val;

    return (p.next, p.val);
}

internal static nint sumEveryOther(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNil();

    nint total = 0;
    nint skipped = 0;
    while (Ꮡp != nil) {
        total += p.val;
        (Ꮡp, skipped) = advance(Ꮡp); p = ref Ꮡp.DerefOrNil();
        _ = skipped;
        if (Ꮡp != nil) {
            (Ꮡp, skipped) = advance(Ꮡp); p = ref Ꮡp.DerefOrNil();
            total += skipped * 0;
        }
    }
    return total;
}

internal static void bumpFirstViaTuple(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNil();

    nint skipped = default!;
    (Ꮡp, skipped) = advance(Ꮡp); p = ref Ꮡp.DerefOrNil();
    _ = skipped;
    if (Ꮡp != nil) {
        p.val += 100;
    }
}

internal static void Main() {
    var list = build(1, 2, 3, 4);
    fmt.Println(sumList(list));
    doubleList(list);
    fmt.Println(sumList(list));
    fmt.Println(sumList(nil));
    fmt.Println(sumEveryOther(list));
    fmt.Println(sumEveryOther(nil));
    bumpFirstViaTuple(list);
    fmt.Println(sumList(list));
}

} // end main_package
