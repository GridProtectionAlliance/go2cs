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
}

internal static float64 localPrecision(float64 x) {
    UntypedFloat c = 5.42857142857142815906e-01;
    UntypedFloat d = -7.05306122448979611050e-01;
    UntypedFloat smallestNormal = 2.22507385850720138309e-308;
    if (x < smallestNormal) {
        return d;
    }
    return (float64)c + x * (float64)d;
}

} // end main_package
