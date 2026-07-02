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

internal static @string signame(nint sig) {
    if (sig == 9) {
        return "SIGKILL"u8;
    }
    return "SIG?"u8;
}

internal static @string describeSignal(nint sig) {
    @string signameΔ1 = signame(sig);
    if (signameΔ1 != ""u8) {
        return "["u8 + signameΔ1 + "]"u8;
    }
    return "none"u8;
}

internal static void Main() {
    fmt.Println(sumWithLenLocal(new nint[]{10, 20, 30}.slice()));
    fmt.Println(sumWithLenLocal(default!));
    fmt.Println(capPlusOne(new slice<nint>(2, 5)));
    fmt.Println(describeSignal(9));
    fmt.Println(describeSignal(1));
}

} // end main_package
