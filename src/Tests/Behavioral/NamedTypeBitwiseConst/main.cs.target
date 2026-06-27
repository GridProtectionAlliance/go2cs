namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:uint8")] partial struct Tag;

internal static readonly UntypedInt classConstructed = /* 0x20 */ 32;

internal static readonly UntypedInt classContext = /* 0x80 */ 128;

public static Tag Constructed(this Tag t) {
    return (Tag)(t | (uint8)classConstructed);
}

public static Tag Context(this Tag t) {
    return (Tag)(t | (uint8)classContext);
}

internal static void Main() {
    Tag t = 16;
    fmt.Println(((uint8)t.Constructed()));
    fmt.Println(((uint8)t.Context()));
}

} // end main_package
