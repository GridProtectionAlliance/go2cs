namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt replacementChar = /* '�' */ 65533;

internal static readonly UntypedFloat scale = 2.5;

internal static readonly UntypedFloat cbrtC = 5.42857142857142815906e-01;

internal static readonly UntypedFloat cbrtD = -7.05306122448979611050e-01;

internal static readonly UntypedFloat folded = /* 19.0 / 35.0 */ 0.5428571428571428;

internal static void Main() {
    rune codepoint = replacementChar;
    @string s = ((@string)codepoint);
    fmt.Println(len(s), codepoint);
    float64 factor = scale;
    fmt.Println(factor * 2);
    fmt.Println(cbrtC);
    fmt.Println(cbrtD);
    fmt.Println(folded);
    fmt.Println(localPrecision(2.0D));
    tightenGuards();
}

internal static void tightenGuards() => func((defer, recover) => {
    UntypedFloat mixed = 0.25;
    float64 f64 = mixed;
    float32 f32 = mixed;
    fmt.Println(f64, f32);
    UntypedFloat feeder = 3.5;
    const float64 derived = /* feeder * 2 */ 7;
    fmt.Println(derived);
    const int64 big = /* 1 << 62 */ 4611686018427387904;
    int64 n = 1;
    fmt.Println(n + big);
    UntypedInt sh = 3;
    uint64 v2 = 1;
    fmt.Println((v2 << (int)(sh)));
    const nint shifted = 3;
    nuint k = 2;
    fmt.Println((shifted << (int)(k)));
    const uint16 localMarker = 0xFFFD;
    slice<uint16> u16 = default!;
    u16 = append(u16, localMarker);
    fmt.Println(u16[0]);
    const byte cb = 200;
    nuint sh1 = 1;
    byte b = 1;
    fmt.Println(b + (byte)(cb << (int)(sh1)));
    const int16 c16 = 30000;
    int16 i16 = 1;
    fmt.Println(i16 + (int16)(c16 << (int)(sh1)));
    const uint16 cu16 = 60000;
    uint16 w16 = 1;
    fmt.Println(w16 + (uint16)(cu16 << (int)(sh1)));
    const nint localDefer = 42;
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), localDefer, defer);
});

internal static float64 localPrecision(float64 x) {
    const float64 c = 5.42857142857142815906e-01;
    const float64 d = -7.05306122448979611050e-01;
    const float64 smallestNormal = 2.22507385850720138309e-308;
    if (x < smallestNormal) {
        return d;
    }
    return c + x * d;
}

} // end main_package
