namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sumWalk(ж<node> Ꮡp, nint steps) {
    ref var p = ref Ꮡp.Value;

    nint total = 0;
    for (nint i = 0; i < steps; i++) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.Value;
    }
    return total;
}

internal static ж<node> firstAfter(ж<node> Ꮡp, nint steps) {
    ref var p = ref Ꮡp.Value;

    for (nint i = 0; i < steps; i++) {
        Ꮡp = p.next; p = ref Ꮡp.Value;
    }
    return Ꮡp;
}

internal static nint markSeen(ж<node> Ꮡp, map<ж<node>, bool> seen) {
    ref var p = ref Ꮡp.Value;

    if (seen[Ꮡp]) {
        return p.val;
    }
    seen[Ꮡp] = true;
    return -p.val;
}

internal static nint walkChain(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNil();

    nint count = 0;
    for (; Ꮡp != nil; Ꮡp = p.next) {
        p = ref Ꮡp.DerefOrNil();
        count += p.val;
    }
    return count;
}

internal static void Main() {
    var a = Ꮡ(new node(val: 1));
    var b = Ꮡ(new node(val: 2));
    var c = Ꮡ(new node(val: 3));
    a.Value.next = b;
    b.Value.next = c;
    c.Value.next = a;
    fmt.Println(sumWalk(a, 6));
    fmt.Println((~firstAfter(a, 4)).val);
    var seen = new map<ж<node>, bool>{};
    fmt.Println(markSeen(a, seen), markSeen(a, seen), markSeen(b, seen));
    var x = Ꮡ(new node(val: 100));
    var y = Ꮡ(new node(val: 20));
    x.Value.next = y;
    fmt.Println("chain:", walkChain(x));
}

} // end main_package
