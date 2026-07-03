namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal ж<node> next;
}

[GoRecv] internal static void link(this ref node n, ж<node> Ꮡm) {
    ref var m = ref Ꮡm.val;

    n.next = Ꮡm;
}

internal static array<nint> process(nint n) {
    array<nint> a = new(5);
    for (nint iΔ1 = 0; iΔ1 < n; iΔ1++) {
        a[iΔ1] = iΔ1 * 10;
    }
    if (n >= 3) {
        for (nint iΔ2 = 0; iΔ2 < 2; iΔ2++) {
            (a[iΔ2], a[iΔ2 + 1]) = (a[iΔ2 + 1], a[iΔ2]);
        }
    }
    ref var nodes = ref heap(new array<node>(5), out var Ꮡnodes);
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i < n - 1; i++) {
        nodes[i].link(Ꮡnodes.at<node>(i + 1));
    }
    return a;
}

internal static nint mapShadow(nint ns) {
    var m = new map<nint, nint>{};
    m[ns] = ns * 100;
    {
        nint nsΔ1 = 3;
        m[nsΔ1] = nsΔ1 * 1000;
    }
    return m[ns] * 10 + len(m);
}

internal static nint caseSiblings(nint kind) {
    nint total = 0;
    switch (kind) {
    case 1: {
        ref var xs = ref heap(new array<node>(3), out var Ꮡxs);
        ref var i = ref heap<nint>(out var Ꮡi);
        for (i = 0; i < 2; i++) {
            xs[i].link(Ꮡxs.at<node>(i + 1));
        }
        ref var ys = ref heap(new array<node>(3), out var Ꮡys);
        ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1);
        for (iΔ1 = 0; iΔ1 < 2; iΔ1++) {
            ys[iΔ1].link(Ꮡys.at<node>(iΔ1 + 1));
        }
        for (var p = Ꮡxs.at<node>(0); p != nil; p = p.val.next) {
            total++;
        }
        for (var p = Ꮡys.at<node>(0); p != nil; p = p.val.next) {
            total += 10;
        }
        break;
    }}

    return total;
}

internal static void Main() {
    fmt.Println(process(5));
    fmt.Println(mapShadow(2));
    fmt.Println(parenIndex());
    fmt.Println(caseSiblings(1));
}

} // end main_package
