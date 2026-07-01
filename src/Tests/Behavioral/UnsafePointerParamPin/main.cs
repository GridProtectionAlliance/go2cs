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
}

} // end main_package
