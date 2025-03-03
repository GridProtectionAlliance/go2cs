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

[GoType("Operators = Sum, Arithmetic, Comparable, Ordered")]
partial interface Float<T> {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

[GoType("Operators = Sum, Comparable, Ordered")]
partial interface Ordered<T> {
    //  Type constraints: Integer | Float | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

public static T Min<T>(T a, T b)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    if (a < b) {
        return a;
    }
    return b;
}

public static U Convert<T, U>(T value, Func<T, U> converter)
    where T : new()
    where U : new()
{
    return converter(value);
}

internal static void Main() {
    nint minValue = Min<nint>(42, 17);
    fmt.Printf("Min value: %d\n"u8, minValue);
    nint strLength = Convert<@string, nint>("hello"u8, (@string s) => len(s));
    fmt.Printf("String length: %d\n"u8, strLength);
}

} // end main_package
