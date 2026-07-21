namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint digits(nint @base, ж<nint> Ꮡinvalid) {
    ref var invalid = ref Ꮡinvalid.DerefOrNil();

    nint n = 0;
    for (nint i = 0; i < 5; i++) {
        if (i >= @base && invalid == 0) {
            invalid = i;
        }
        n++;
    }
    return n;
}

internal static void Main() {
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 0;
    nint c1 = digits(3, Ꮡx);
    fmt.Println(c1, x);
    nint c2 = digits(10, nil);
    fmt.Println(c2);
}

} // end main_package
