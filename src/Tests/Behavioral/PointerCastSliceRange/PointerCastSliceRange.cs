namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static unsafe void Main() {
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{10, 20, 30, 40}.array();
    var s = new slice<nint>(new ReadOnlySpan<nint>((nint*)(uintptr)(new @unsafe.Pointer(Ꮡarr)), 4));
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
    var ip = Ꮡarr;
    var back = (~(ж<ж<array<nint>>>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡ(ip)).Value))).Value;
    _ = back;
}

} // end main_package
