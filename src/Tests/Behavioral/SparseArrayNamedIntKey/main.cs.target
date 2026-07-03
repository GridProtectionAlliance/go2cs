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

internal static slice<@string> rankNames = new golib.SparseArray<@string>{
    [(int)rankLow] = "low"u8,
    [(int)rankMid] = "mid"u8,
    [(int)rankHigh] = "high"u8
}.slice();

internal static slice<@string> codeNames = new golib.SparseArray<@string>{
    [codeA] = "a"u8,
    [codeB] = "b"u8
}.slice();

[GoType("num:uintptr")] partial struct errno;

internal static readonly errno errBase = /* 1 << 10 */ 1024;
internal static readonly errno eBig = /* errBase + 1 */ 1025;
internal static readonly errno eAcces = /* errBase + 2 */ 1026;

internal static slice<@string> errNames = new golib.SparseArray<@string>{
    [1] = "big"u8,
    [2] = "acces"u8
}.slice();

internal static void Main() {
    fmt.Println(rankNames[rankLow], rankNames[rankMid], rankNames[rankHigh]);
    fmt.Println(codeNames[codeA], codeNames[codeB]);
    fmt.Println(len(rankNames));
    fmt.Println(errNames[eBig - errBase], errNames[eAcces - errBase]);
    fmt.Println(asciiSpace[(rune)'\t'], asciiSpace[(rune)'\n'], asciiSpace[(rune)' '], asciiSpace[(rune)'A'], len(asciiSpace));
}

internal static array<uint8> asciiSpace = new array<uint8>(256){[(rune)'\t'] = 1, [(rune)'\n'] = 1, [(rune)'\v'] = 1, [(rune)'\f'] = 1, [(rune)'\r'] = 1, [(rune)' '] = 1};

} // end main_package
