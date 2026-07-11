/*
package main

import (
    "fmt"
    "math/cmplx"
)

var (
    ToBe   bool       = false
    MaxInt uint64     = 1<<64 - 1 // <-- This is trickier than it looks
    z      complex128 = cmplx.Sqrt(-5 + 12i)
)

func main() {
    fmt.Printf("Type: %T Value: %v\n", ToBe, ToBe)
    fmt.Printf("Type: %T Value: %v\n", MaxInt, MaxInt)
    fmt.Printf("Type: %T Value: %v\n", z, z)
}
*/
#region source
using fmt = go.fmt_package;
using cmplx = go.math.cmplx_package;
using uint64 = System.UInt64;
using complex128 = System.Numerics.Complex;
using static go.builtin;

static class main_package
{
    static bool ToBe = false;

    // Go literal expressions are arbitrary-precision numbers until assigned to a variable,
    // C# conversion process should auto-convert using result from BigInteger, e.g.:
    // (uint64)((BigInteger.Parse("1", NumberStyles.Float) << 64) - BigInteger.Parse("1", NumberStyles.Float))
    static uint64 MaxInt = /* 1 << 64 - 1 */ 18446744073709551615UL;

    static complex128 z = cmplx.Sqrt(-5 + i(12));

    static void Main() {
        fmt.Printf("Type: {0} Value: {1}\n", GetGoTypeName(ToBe.GetType()), ToBe);
        fmt.Printf("Type: {0} Value: {1}\n", GetGoTypeName(MaxInt.GetType()), MaxInt);
        fmt.Printf("Type: {0} Value: ({1}+{2}i)\n", GetGoTypeName(z.GetType()), z.Real, z.Imaginary);
    }
}
#endregion
// BIG TAKEAWAY: all constants are arbitrary-precision numbers until assigned to a variable.
// Compiler should "preparse" all constants as BigInteger for compile-time maths then apply
// result at variable assignment

// For example, Go has no issue taking a compile-time 1e1000 / 1e999 to get a value of 10:
//   BigInteger v = BigInteger.Parse("1e1000",  NumberStyles.Float);
//   v /= BigInteger.Parse("1e999", NumberStyles.Float);
//   fmt.Println("{0}", v); <- prints 10
