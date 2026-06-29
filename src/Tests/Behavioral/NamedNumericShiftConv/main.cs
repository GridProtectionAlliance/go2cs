namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nuint")] partial struct arenaIdx;

[GoType("num:uint8")] partial struct tag;

[GoType("num:uint64")] partial struct big;

internal static readonly UntypedInt bits = 6;

internal static void Main() {
    arenaIdx a = ((arenaIdx)((arenaIdx)((nuint)1 << (int)(bits))));
    tag t = ((tag)((tag)(uint8)(1 << (int)(3))));
    big b = ((big)((big)((uint64)1 << (int)(40))));
    fmt.Println(((nuint)a), ((uint8)t), ((uint64)b));
}

} // end main_package
