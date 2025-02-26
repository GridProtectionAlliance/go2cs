namespace go;

partial class constraints_package {

[GoType] partial struct Frog {
    public @string Name;
    public @string Color;
}

[GoType] partial interface ConstraintTest1<T> {
    //  Type constraints: string | []int | map[string]int | chan string | *int | [2]int | Frog
    // Derived operators: none
    @string Upper();
}

[GoType("Operators = Comparable")]
partial interface ConstraintTest2<T> {
    //  Type constraints: string | chan string | *int | [2]int | Frog
    // Derived operators: ==, !=
    @string Lower();
}

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

[GoType("Operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface PromotedTest1<T> {
    //  Type constraints: Signed
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType] partial interface PromotedTest2<T> :
    ConstraintTest1<T>
{
    //  Type constraints: ConstraintTest1
    // Derived operators: none
}

[GoType("Operators = Comparable")]
partial interface PromotedTest3<T> :
    ConstraintTest2<T>
{
    //  Type constraints: ConstraintTest2
    // Derived operators: ==, !=
}

[GoType("Operators = Sum, Arithmetic, Comparable, Ordered")]
partial interface Float<T> {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

[GoType("Operators = Sum, Arithmetic, Comparable")]
partial interface Complex<T> {
    //  Type constraints: ~complex64 | ~complex128
    // Derived operators: +, -, *, /, ==, !=
}

[GoType("Operators = Sum, Comparable, Ordered")]
partial interface Ordered<T> {
    //  Type constraints: Integer | Float | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

} // end constraints_package
