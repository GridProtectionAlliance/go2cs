namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

[GoType("num:uintptr")] partial struct utp;

internal static uintptr readViaParam(ж<uintptr> Ꮡp) {
    ref var p = ref Ꮡp.val;

    var q = (ж<uintptr>)(uintptr)(@unsafe.Pointer.FromRef(ref p));
    return q.val;
}

[GoRecv] internal static uintptr read(this ref utp r) {
    var q = (ж<uintptr>)(uintptr)(@unsafe.Pointer.FromRef(ref r));
    return q.val;
}

[GoRecv] internal static uintptr tricky(this ref utp r) {
    ref var y = ref heap(new uintptr(), out var Ꮡy);
    y = 111;
    {
        var rΔ1 = Ꮡy;
        var q = (ж<uintptr>)(uintptr)(@unsafe.Pointer.FromRef(ref (rΔ1).val));
        return q.val;
    }
}

[GoType] partial struct holder {
    internal uintptr v;
}

internal static @unsafe.Pointer pick(bool cond, @unsafe.Pointer a, @unsafe.Pointer zero) {
    if (cond) {
        return a;
    }
    return zero;
}

internal static (uint32, @unsafe.Pointer) advance(@unsafe.Pointer fd, uint32 n) {
    return (n + 1, fd);
}

internal static ж<uintptr> same(ж<uintptr> Ꮡp) {
    ref var p = ref Ꮡp.val;

    return Ꮡp;
}

internal static void Main() {
    ref var x = ref heap(new uintptr(), out var Ꮡx);
    x = 42;
    fmt.Println(readViaParam(Ꮡx));
    utp t = 77;
    fmt.Println(t.read());
    fmt.Println(t.tricky());
    var h = new holder(v: 9);
    var q = (ж<uintptr>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡ(h).of(holder.Ꮡv)).val));
    fmt.Println(q.val);
    ref var a = ref heap(new uintptr(), out var Ꮡa);
    a = 11;
    ref var z = ref heap(new uintptr(), out var Ꮡz);
    z = 22;
    @unsafe.Pointer pa = @unsafe.Pointer.FromRef(ref (Ꮡa).val);
    @unsafe.Pointer pz = @unsafe.Pointer.FromRef(ref (Ꮡz).val);
    fmt.Println(~(ж<uintptr>)(uintptr)((uintptr)pick(true, pa, pz)), ~(ж<uintptr>)(uintptr)((uintptr)pick(false, pa, pz)));
    var (n2, fd2) = advance(pa, 5);
    fmt.Println(n2, ~(ж<uintptr>)(uintptr)(fd2));
    var w = same(Ꮡa);
    w.val = 99;
    fmt.Println(a);
}

} // end main_package
