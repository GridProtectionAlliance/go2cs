namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    var b = new byte[]{}.slice();
    for (nint ch = 32; ch < 80; ch++) {
        b = append(b, ((@string)((rune)ch)).ꓸꓸꓸ);
    }
    @string str = @unsafe.String(Ꮡ(b, 0), len(b));
    fmt.Println(str);
    var strptr = @unsafe.StringData(str);
    fmt.Println(@unsafe.String(strptr, len(str)));
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{1, 2, 3, 4}.array();
    var arrptr = Ꮡarr.at<nint>(0);
    var nextPtr = ((@unsafe.Pointer)(((uintptr)new @unsafe.Pointer(arrptr)) + @unsafe.Sizeof(arr[0])));
    fmt.Println("Value of the next element:", ~(ж<nint>)(uintptr)(nextPtr));
}

} // end main_package
