namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:uint8")] partial struct Tag;

internal static readonly UntypedInt classConstructed = 0x20;

internal static readonly UntypedInt classContext = 0x80;

public static Tag Constructed(this Tag t) {
    return (Tag)(t | (uint8)classConstructed);
}

public static Tag Context(this Tag t) {
    return (Tag)(t | (uint8)classContext);
}

[GoType("num:uint64")] partial struct word;

internal static readonly UntypedInt tagBits = 19;

internal static word low(this word w) {
    return (word)(w & (uint64)(((1 << (int)(tagBits)) - 1)));
}

internal static void Main() {
    Tag t = 0x10;
    fmt.Println((uint8)t.Constructed());
    fmt.Println((uint8)t.Context());
    word w = 0xABCDEF012345UL;
    fmt.Println((uint64)w.low());
    fmt.Println((uint8)mask((Tag)(~((Tag)((Tag)0)))));
    fmt.Println((uint64)maskFor(4), (uint64)maskFor(19));
    word wv = 0xF0;
    fmt.Println((uint64)((word)((word)(wv & 0x30) | ((word)0x01))), (uint64)((word)(wv ^ ((word)0xFF))));
}

internal static Tag mask(Tag t) {
    return (Tag)(t & 0xFF);
}

internal static word maskFor(nuint s) {
    var m = (((word)1) << (int)(s)) - 1;
    return m;
}

} // end main_package
