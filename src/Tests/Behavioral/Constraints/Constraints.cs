using System.Numerics;

namespace go;

partial class constraints_package {

    // In the case of generic constraints, restrictions in C# work somewhat differently than in Go. In C# a constraint
    // can be a class, an interface and a few special cases. In Go, all constraints are interfaces and can be restricted
    // to types, i.e., structs and heap-allocated types alike. Since at the point of the C# code conversion Go will have
    // already parsed and validated the code, we can assume that all the type-based constraints will have been satisfied.
    // Also, any defined method-set constraints will be handled as normal for existing interface conversion handling.
    // The remaining step to be handled is to determine the set of operators that all types in the constraint type-set
    // have in common, which is the set of operators that the C# code will need to account for. There are three sets of
    // operators to be considered: arithmetic, comparison and equality. The arithmetic operators apply only to numeric
    // types, the comparison operators apply to types that can be ordered and the equality operators apply to types that
    // can be compared for equality. See "Operators" section in the Go specification: https://go.dev/ref/spec#Operators

    /*
        Arithmetic operators:

        +    sum                    integers, floats, complex values, strings
        -    difference             integers, floats, complex values
        *    product                integers, floats, complex values
        /    quotient               integers, floats, complex values
        %    remainder              integers

        &    bitwise AND            integers
        |    bitwise OR             integers
        ^    bitwise XOR            integers
        &^   bit clear (AND NOT)    integers

        <<   left shift             integer << integer >= 0
        >>   right shift            integer >> integer >= 0

        Comparison operators:

        ==    equal                 comparable types
        !=    not equal             comparable types
        <     less                  ordered types
        <=    less or equal         ordered types
        >     greater               ordered types
        >=    greater or equal      ordered types

        comparable types:           bools, integers, floats, complex values, strings, pointers, channels, interfaces, structs, arrays
        ordered types:              integers, floats, strings
     */

[GoType] partial interface Signed<T> : ISignedNumber<T> where T : ISignedNumber<T>
    //~@int | ~int8 | ~int16 | ~int32 | ~int64
{
}

[GoType] partial interface Unsigned<T> : IUnsignedNumber<T> where T : IUnsignedNumber<T>
    //~@uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
{
}

[GoType] partial interface Integer<T> : INumber<T> where T : INumber<T>
    //Signed | Unsigned
{
}

[GoType] partial interface Float<T> : IFloatingPointIeee754<T> where T : IFloatingPointIeee754<T>
    //~float32 | ~float64
{
}

[GoType] partial interface Complex<T> : INumberBase<T> where T : INumberBase<T>
    //~complex64 | ~complex128
{
}

[GoType] partial interface Ordered<T> where T : comparable<T>
    //Integer | Float | ~@string
{
}

} // end constraints_package
