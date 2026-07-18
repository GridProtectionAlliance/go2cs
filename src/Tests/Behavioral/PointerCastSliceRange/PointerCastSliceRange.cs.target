namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{10, 20, 30, 40}.array();
    var s = (~Ꮡarr)[..4];
    nint sum = 0;
    foreach (var (i, _) in s) {
        sum += i;
    }
    nint total = 0;
    foreach (var (_, v) in s) {
        total += v;
    }
    foreach (var (i, _) in s) {
        var ps = Ꮡ(s, i);
        ps.Value += 1;
    }
    fmt.Println(sum, total, s[0]);
    ref var ip = ref heap<ж<array<nint>>>(out var Ꮡip);
    ip = Ꮡarr;
    var back = (~(ж<ж<array<int64>>>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡip).Value))).Value.Clone();
    _ = back;
    var pick = @unsafe.Pointer (bool u) => {
        if (u) {
            return (@unsafe.Pointer)(uintptr)0;
        }
        return new @unsafe.Pointer(Ꮡip.ValueSlot);
    };
    _ = (uintptr)pick(true);
    var op = ((opaque)(ж<array<nint>>)(uintptr)(new @unsafe.Pointer(ip)));
    _ = op;
}

[GoType("ж<array<nint>>")] partial class opaque;

} // end main_package
