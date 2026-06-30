namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 lowestSetBit(uint32 x) {
    return (uint32)(x & ((uint32)0 - x));
}

[GoType("dyn")] partial struct main_bitfield {
    internal uint64 cache;
}

internal static void Main() {
    fmt.Println(lowestSetBit(12));
    fmt.Println(lowestSetBit(40));
    uint64 z = 6;
    fmt.Println((uint64)(z & ((uint64)0 - z)));
    uint64 x = 256;
    fmt.Println((x >> (int)(4)) + x);
    fmt.Println((1 << (int)(15)) - 1);
    fmt.Println(x - ((uint64)1 << (int)(4)));
    nuint s = 3;
    uint64 y = 1;
    y <<= (int)(s);
    fmt.Println(y);
    y >>= (int)(s);
    fmt.Println(y);
    var bf = new main_bitfield(cache: 255);
    nuint n = 4;
    bf.cache >>= (int)(n);
    fmt.Println(bf.cache);
    bf.cache <<= (int)(n);
    fmt.Println(bf.cache);
    uint64 hi = (nuint)18446744073709551615UL;
    fmt.Println((uint64)(hi & ~(((uint64)1 << (int)(63)))));
    uint32 u = 4294967295U;
    fmt.Println((uint32)(u & ~(((uint32)1 << (int)(31)))));
}

} // end main_package
