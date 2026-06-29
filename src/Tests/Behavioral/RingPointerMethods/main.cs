namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Ring {
    internal ж<Ring> next, prev;
    public nint Value;
}

internal static ж<Ring> init(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.val;

    r.next = Ꮡr;
    r.prev = Ꮡr;
    return Ꮡr;
}

public static ж<Ring> Next(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.val;

    if (r.next == nil) {
        return Ꮡr.init();
    }
    return r.next;
}

public static ж<Ring> Prev(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.val;

    if (r.next == nil) {
        return Ꮡr.init();
    }
    return r.prev;
}

public static ж<Ring> Move(this ж<Ring> Ꮡr, nint n) {
    ref var r = ref Ꮡr.val;

    if (r.next == nil) {
        return Ꮡr.init();
    }
    for (; n < 0; n++) {
        Ꮡr = r.prev; r = ref Ꮡr.val;
    }
    for (; n > 0; n--) {
        Ꮡr = r.next; r = ref Ꮡr.val;
    }
    return Ꮡr;
}

public static nint Len(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.val;

    nint n = 0;
    if (r != nil) {
        n = 1;
        for (var p = Ꮡr.Next(); p != Ꮡr; p = p.val.next) {
            n++;
        }
    }
    return n;
}

internal static ж<Ring> makeRing(nint count) {
    if (count <= 0) {
        return default!;
    }
    var r = Ꮡ(new Ring(Value: 0));
    r.init();
    var p = r;
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 1; i < count; i++) {
        var e = Ꮡ(new Ring(Value: i));
        e.val.prev = p;
        e.val.next = p.val.next;
        p.val.next.val.prev = e;
        p.val.next = e;
        p = e;
    }
    return r;
}

internal static void Main() {
    var a = Ꮡ(new Ring(Value: 42));
    a.init();
    fmt.Println((~a.Next()).Value);
    fmt.Println(a.Len());
    fmt.Println((~a.Move(5)).Value);
    var r = makeRing(4);
    fmt.Println(r.Len());
    fmt.Println((~r.Next()).Value);
    fmt.Println((~r.Move(2)).Value);
    fmt.Println((~r.Prev()).Value);
    fmt.Println((~r.Move(-1)).Value);
}

} // end main_package
