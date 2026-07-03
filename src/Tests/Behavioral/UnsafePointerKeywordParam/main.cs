namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static bool sameAs(@unsafe.Pointer old, @unsafe.Pointer @new) {
    return @new.Value == old.Value;
}

internal static bool notNil(@unsafe.Pointer @new) {
    return @new != nil;
}

internal static void Main() {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    @unsafe.Pointer pa = new @unsafe.Pointer(Ꮡa);
    @unsafe.Pointer pb = new @unsafe.Pointer(Ꮡb);
    fmt.Println(sameAs(pa, pa));
    fmt.Println(sameAs(pa, pb));
    fmt.Println(notNil(pa));
    fmt.Println(notNil(nil));
}

} // end main_package
