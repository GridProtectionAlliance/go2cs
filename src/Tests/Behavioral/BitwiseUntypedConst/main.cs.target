namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint64 copysign(uint64 f, uint64 sign) {
    UntypedInt signBit = /* 1 << 63 */ 9223372036854775808;
    return (uint64)((uint64)(f & ~(uint64)signBit) | (uint64)(sign & (uint64)signBit));
}

internal static void Main() {
    fmt.Println(copysign(255, (nuint)9223372036854775808UL));
    fmt.Println(copysign((nuint)9223372036854775874UL, 0));
}

} // end main_package
