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

public static S Twice<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    return Scale(s, c);
}

public static (E, E) CopyClearMinMax<S, E>(S dst, S src)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    copy(dst, src);
    var lo = min(dst[0], src[1]);
    var hi = max(dst[0], src[1]);
    clear(src);
    return (lo, hi);
}

internal static void Main() {
    Point p = default!;
    p = new int32[]{1, 2, 3}.slice();
    ScaleAndPrint(p);
    fmt.Println(Twice(new Point(new int32[]{3, 4}.slice()), 2).String());
    var (dst2, src2) = (new Point(new int32[]{0, 0, 0}.slice()), new Point(new int32[]{7, 8, 9}.slice()));
    var (lo, hi) = CopyClearMinMax<Point, int32>(dst2, src2);
    fmt.Println(dst2.String(), src2.String(), lo, hi);
}

} // end main_package
