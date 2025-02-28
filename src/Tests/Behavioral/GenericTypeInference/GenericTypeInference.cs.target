namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("Operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Signed<T> {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("Operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Unsigned<T> {
    //  Type constraints: ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("Operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Integer<T> {
    //  Type constraints: Signed | Unsigned
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("[]int32")]
partial struct Point {
    // TODO: Add this constructor to slice template
    public Point(nint size)
    {
        m_value = new int32[size];
    }
}

public static @string String(this Point p) {
    return fmt.Sprintf("%d"u8, p);
}

public static S Scale<S, E>(S s, E c)
    // TODO: Add slice constraint handling
    where S : /*~[]E*/ ISlice<E>, new()
    // TODO: Lift base type constraints when there are no interface methods
    where E : /*Integer*/ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var r = make<S>(len(s));

    // TODO: Fix this range loop
    /* for i, v := range s {
	r[i] = v * c
} */
    return r;
}

public static void ScaleAndPrint(Point p) {
    var r = Scale(p, 2);
    fmt.Println(r.String());
}

private static void Main() {
    Point p = default!;
    p = new int32[]{1, 2, 3}.slice();
    ScaleAndPrint(p);
}

} // end main_package
