namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

public static readonly UntypedFloat MaxFloat32 = /* 0x1p127 * (1 + (1 - 0x1p-23)) */ 3.4028234663852886e+38;
public static readonly UntypedFloat SmallestNonzeroFloat32 = /* 0x1p-126 * 0x1p-23 */ 1.401298464324817e-45;
public static readonly UntypedFloat MaxFloat64 = /* 0x1p1023 * (1 + (1 - 0x1p-52)) */ 1.7976931348623157e+308;
public static readonly UntypedFloat SmallestNonzeroFloat64 = /* 0x1p-1022 * 0x1p-52 */ 5e-324;

public static readonly UntypedFloat Ln10 = 2.30258509299404568401799145468436420760110148862877297603332790;
public static readonly UntypedFloat Log10E = /* 1 / Ln10 */ 0.4342944819032518;
public static readonly UntypedFloat Pi = 3.14159265358979323846264338327950288419716939937510582097494459;
internal static readonly UntypedFloat twoPi = /* 2 * Pi */ 6.283185307179586;
internal static readonly UntypedFloat halfPi = /* Pi / 2 */ 1.5707963267948966;
internal static readonly UntypedFloat third = /* 1.0 / 3.0 */ 0.3333333333333333;

internal static bool isInf(float64 f, nint sign) {
    return sign >= 0 && f > MaxFloat64 || sign <= 0 && f < -MaxFloat64;
}

internal static void bits64(@string label, uint64 got, uint64 want) {
    fmt.Println(label, got, got == want);
}

internal static void bits32(@string label, uint32 got, uint32 want) {
    fmt.Println(label, got, got == want);
}

internal static void Main() {
    fmt.Println("-- float64 constant expressions (IEEE 754 bits, want-match) --");
    bits64("MaxFloat64            "u8, Δmath.Float64bits(MaxFloat64), 0x7fefffffffffffffUL);
    bits64("SmallestNonzeroFloat64"u8, Δmath.Float64bits(SmallestNonzeroFloat64), 0x1);
    bits64("Ln10                  "u8, Δmath.Float64bits(Ln10), 0x40026bb1bbb55516UL);
    bits64("Log10E                "u8, Δmath.Float64bits(Log10E), 0x3fdbcb7b1526e50eUL);
    bits64("Pi                    "u8, Δmath.Float64bits(Pi), 0x400921fb54442d18UL);
    bits64("twoPi                 "u8, Δmath.Float64bits(twoPi), 0x401921fb54442d18UL);
    bits64("halfPi                "u8, Δmath.Float64bits(halfPi), 0x3ff921fb54442d18UL);
    bits64("third                 "u8, Δmath.Float64bits(third), 0x3fd5555555555555UL);
    fmt.Println("-- float32 constant expressions (IEEE 754 bits, want-match) --");
    bits32("MaxFloat32            "u8, Δmath.Float32bits(MaxFloat32), 0x7f7fffff);
    bits32("SmallestNonzeroFloat32"u8, Δmath.Float32bits(SmallestNonzeroFloat32), 0x1);
    fmt.Println("-- exact-value identities --");
    fmt.Println("MaxFloat64 == literal:", (float64)MaxFloat64 == 1.7976931348623157e+308D);
    fmt.Println("SmallestNonzeroFloat64 == literal:", (float64)SmallestNonzeroFloat64 == 5e-324D);
    fmt.Println("MaxFloat32 == literal:", (float32)MaxFloat32 == 3.4028235e+38F);
    fmt.Println("Log10E == literal:", (float64)Log10E == 0.4342944819032518D);
    fmt.Println("-- IsInf boundary --");
    fmt.Println("isInf(MaxFloat64):", isInf(1.7976931348623157e+308D, 1));
    fmt.Println("isInf(-MaxFloat64):", isInf(-1.7976931348623157e+308D, -1));
    fmt.Println("isInf(truncated 1.79769e+308):", isInf(1.79769e+308D, 1));
    fmt.Println("truncated < MaxFloat64:", 1.79769e+308D < (float64)MaxFloat64);
}

} // end main_package
