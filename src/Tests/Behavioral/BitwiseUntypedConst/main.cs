namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint64 copysign(uint64 f, uint64 sign) {
    const uint64 signBit = /* 1 << 63 */ 9223372036854775808;
    return (uint64)((uint64)(f & ~signBit) | (uint64)(sign & signBit));
}

internal static void Main() {
    fmt.Println(copysign(0xFF, 0x8000000000000000UL));
    fmt.Println(copysign(0x8000000000000042UL, 0));
}

} // end main_package
