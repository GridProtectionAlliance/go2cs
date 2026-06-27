namespace go;

using fmt = fmt_package;

partial class main_package {

public static readonly uintptr MaxU = /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);

internal static void Main() {
    uintptr zero = 0;
    uintptr one = 1;
    fmt.Println(MaxU > zero);
    fmt.Println(MaxU + one == zero);
    fmt.Println(MaxU - one < MaxU);
}

} // end main_package
