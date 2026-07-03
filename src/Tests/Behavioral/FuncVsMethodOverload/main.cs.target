namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer add(@unsafe.Pointer p, uintptr x) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

[GoType] partial struct nih {
    internal uint32 a, b;
}

internal static ж<nih> add(this ж<nih> Ꮡp, uintptr bytes) {
    ref var p = ref Ꮡp.Value;

    if (bytes == 0) {
        return Ꮡp;
    }
    return (ж<nih>)(uintptr)((@unsafe.Pointer)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref p) + bytes));
}

internal static uint32 step(ж<uint32> Ꮡv) {
    ref var v = ref Ꮡv.Value;

    var q = (ж<uint32>)(uintptr)(add(new @unsafe.Pointer(Ꮡv), @unsafe.Sizeof((uint32)0)));
    return q.Value;
}

[GoType] partial struct holder {
    internal uint32 x, y;
}

[GoRecv] internal static uint32 second(this ref holder h) {
    return ~(ж<uint32>)(uintptr)(add((uintptr)@unsafe.Pointer.FromRef(ref h), @unsafe.Sizeof((uint32)0)));
}

internal static void Main() {
    ref var vals = ref heap<array<uint32>>(out var Ꮡvals);
    vals = new uint32[]{10, 20, 30, 40}.array();
    fmt.Println(step(Ꮡvals.at<uint32>(0)));
    @unsafe.Pointer p = new @unsafe.Pointer(Ꮡvals.at<uint32>(1));
    fmt.Println(~(ж<uint32>)(uintptr)(add(p, 8)));
    ref var n = ref heap<nih>(out var Ꮡn);
    n = new nih(a: 7, b: 9);
    var m = Ꮡn.add(0);
    fmt.Println((~m).a, (~m).b);
    var pb = (ж<uint32>)(uintptr)(new @unsafe.Pointer(Ꮡn.add(4)));
    fmt.Println(pb.Value);
    var hh = new holder(x: 3, y: 5);
    fmt.Println(hh.second());
}

} // end main_package
