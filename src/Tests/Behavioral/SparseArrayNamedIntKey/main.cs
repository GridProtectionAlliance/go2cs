namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nint")] partial struct rank;

internal static readonly rank rankLow = /* iota */ 0;
internal static readonly rank rankMid = 1;
internal static readonly rank rankHigh = 2;

[GoType("num:uint8")] partial struct code;

internal static readonly code codeA = /* iota */ 0;
internal static readonly code codeB = 1;

internal static slice<@string> rankNames = new runtime.SparseArray<@string>{
    [(int)rankLow] = "low"u8,
    [(int)rankMid] = "mid"u8,
    [(int)rankHigh] = "high"u8
}.slice();

internal static slice<@string> codeNames = new runtime.SparseArray<@string>{
    [codeA] = "a"u8,
    [codeB] = "b"u8
}.slice();

internal static void Main() {
    fmt.Println(rankNames[rankLow], rankNames[rankMid], rankNames[rankHigh]);
    fmt.Println(codeNames[codeA], codeNames[codeB]);
    fmt.Println(len(rankNames));
}

} // end main_package
