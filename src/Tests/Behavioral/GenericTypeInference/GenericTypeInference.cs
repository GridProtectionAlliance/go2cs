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

[GoType("[]int32")] partial struct Point {}

public static @string String(this Point p) {
    return fmt.Sprintf("%d"u8, p);
}

public static S Scale(S s, E c) {
    var r = make<S>(len(s));
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
