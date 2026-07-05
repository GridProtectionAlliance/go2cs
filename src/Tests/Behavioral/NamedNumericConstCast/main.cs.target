namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nuint")] partial struct Class;

[GoType("num:uint64")] partial struct Big;

internal static readonly Class allClass = /* ^Class(0) */ unchecked((Class)18446744073709551615);
internal static readonly Big allBig = /* ^Big(0) */ unchecked((Big)18446744073709551615);
internal static readonly Class small = /* Class(5) */ 5;

internal static void Main() {
    fmt.Println((uint64)(nuint)allClass);
    fmt.Println((uint64)allBig);
    fmt.Println((nuint)small);
}

} // end main_package
