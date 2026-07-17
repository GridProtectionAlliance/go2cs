namespace go;

public static partial class math_package
{
    // Integer limit values (form matches full-conversion output; Go math declares these
    // as untyped constants).
    public static readonly UntypedInt MaxInt8 = /* 1<<7 - 1 */ 127; // 127

    // Inf returns positive infinity if sign >= 0, negative infinity if sign < 0.
    public static double Inf(nint sign)
    {
        return sign >= 0 ? double.PositiveInfinity : double.NegativeInfinity;
    }

    // NaN returns an IEEE 754 "not-a-number" value.
    public static double NaN()
    {
        return double.NaN;
    }
}
