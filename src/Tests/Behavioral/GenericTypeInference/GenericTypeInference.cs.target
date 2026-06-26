namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Signed<ΔT> {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Unsigned<ΔT> {
    //  Type constraints: ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Integer<ΔT> {
    //  Type constraints: Signed | Unsigned
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("[]int32")] partial struct Point;

public static @string String(this Point p) {
    return fmt.Sprintf("%d"u8, p);
}

public static S Scale<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var r = make<S>(len(s));
    foreach (var (i, v) in s) {
        r[i] = v * c;
    }
    return r;
}

public static void ScaleAndPrint(Point p) {
    var r = Scale(p, 2);
    fmt.Println(r.String());
}

internal static void Main() {
    Point p = default!;
    p = new int32[]{1, 2, 3}.slice();
    ScaleAndPrint(p);
}

} // end main_package
