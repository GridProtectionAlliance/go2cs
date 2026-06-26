namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint sumWithLenLocal(slice<nint> buf) {
    if (len(buf) == 0) {
        return 0;
    }
    nint lenΔ1 = len(buf);
    nint total = 0;
    for (nint i = 0; i < lenΔ1; i++) {
        total += buf[i];
    }
    return total + lenΔ1;
}

internal static nint capPlusOne(slice<nint> s) {
    nint capΔ1 = cap(s);
    return capΔ1 + 1;
}

internal static void Main() {
    fmt.Println(sumWithLenLocal(new nint[]{10, 20, 30}.slice()));
    fmt.Println(sumWithLenLocal(default!));
    fmt.Println(capPlusOne(new slice<nint>(2, 5)));
}

} // end main_package
