namespace go;

partial class constraints_package {

[GoType] partial struct Frog {
    public @string Name;
    public @string Color;
}

[GoType] partial interface ConstraintTest1<ΔT> {
    //  Type constraints: string | []int | map[string]int | chan string | *int | [2]int | Frog
    // Derived operators: none
    @string Upper();
}

[GoType("operators = Comparable")]
partial interface ConstraintTest2<ΔT> {
    //  Type constraints: string | chan string | *int | [2]int | Frog
    // Derived operators: ==, !=
    @string Lower();
}

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

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface PromotedTest1<ΔT> {
    //  Type constraints: Signed
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType] partial interface PromotedTest2<ΔT> :
    ConstraintTest1<ΔT>
{
    //  Type constraints: ConstraintTest1
    // Derived operators: none
}

[GoType("operators = Comparable")]
partial interface PromotedTest3<ΔT> :
    ConstraintTest2<ΔT>
{
    //  Type constraints: ConstraintTest2
    // Derived operators: ==, !=
}

[GoType("operators = Sum, Arithmetic, Comparable, Ordered")]
partial interface Float<ΔT> {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

[GoType("operators = Sum, Arithmetic, Comparable")]
partial interface Complex<ΔT> {
    //  Type constraints: ~complex64 | ~complex128
    // Derived operators: +, -, *, /, ==, !=
}

[GoType("operators = Sum, Comparable, Ordered")]
partial interface Ordered<ΔT> {
    //  Type constraints: Integer | Float | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

} // end constraints_package
