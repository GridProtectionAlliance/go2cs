namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nint")] partial struct Code;

public static readonly Code A = /* iota */ 0;
internal static readonly Code _ᴛ1ʗ = 1;
public static readonly Code B = 2;
internal static readonly Code _ᴛ2ʗ = 3;
public static readonly Code C = 4;

internal static void _() {
    if (A + B + C < 0) {
        throw panic("unreachable");
    }
}

internal static void Main() {
    fmt.Println(A, B, C);
}

} // end main_package
