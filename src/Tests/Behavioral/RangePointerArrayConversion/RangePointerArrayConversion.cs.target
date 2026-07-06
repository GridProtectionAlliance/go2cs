namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var a = ref heap<array<nint>>(out var Ꮡa);
    a = new nint[]{10, 20, 30}.array();
    @unsafe.Pointer p = new @unsafe.Pointer(Ꮡa);
    nint sum = 0;
    foreach (var (i, x) in ((ж<array<nint>>)(uintptr)(p)).Value) {
        sum += i + x;
    }
    fmt.Println(sum);
}

} // end main_package
