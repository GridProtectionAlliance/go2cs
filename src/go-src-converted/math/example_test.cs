// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δmath = go.math_package;
using go;

partial class math_test_package {

public static void ExampleAcos() {
    fmt.Printf("%.2f"u8, Δmath.Acos(1));
}

// Output: 0.00
public static void ExampleAcosh() {
    fmt.Printf("%.2f"u8, Δmath.Acosh(1));
}

// Output: 0.00
public static void ExampleAsin() {
    fmt.Printf("%.2f"u8, Δmath.Asin(0));
}

// Output: 0.00
public static void ExampleAsinh() {
    fmt.Printf("%.2f"u8, Δmath.Asinh(0));
}

// Output: 0.00
public static void ExampleAtan() {
    fmt.Printf("%.2f"u8, Δmath.Atan(0));
}

// Output: 0.00
public static void ExampleAtan2() {
    fmt.Printf("%.2f"u8, Δmath.Atan2(0, 0));
}

// Output: 0.00
public static void ExampleAtanh() {
    fmt.Printf("%.2f"u8, Δmath.Atanh(0));
}

// Output: 0.00
public static void ExampleCopysign() {
    fmt.Printf("%.2f"u8, Δmath.Copysign(3.2D, -1D));
}

// Output: -3.20
public static void ExampleCos() {
    fmt.Printf("%.2f"u8, Δmath.Cos(Δmath.Pi / 2D));
}

// Output: 0.00
public static void ExampleCosh() {
    fmt.Printf("%.2f"u8, Δmath.Cosh(0));
}

// Output: 1.00
public static void ExampleSin() {
    fmt.Printf("%.2f"u8, Δmath.Sin(Δmath.Pi));
}

// Output: 0.00
public static void ExampleSincos() {
    var (sin, cos) = Δmath.Sincos(0);
    fmt.Printf("%.2f, %.2f"u8, sin, cos);
}

// Output: 0.00, 1.00
public static void ExampleSinh() {
    fmt.Printf("%.2f"u8, Δmath.Sinh(0));
}

// Output: 0.00
public static void ExampleTan() {
    fmt.Printf("%.2f"u8, Δmath.Tan(0));
}

// Output: 0.00
public static void ExampleTanh() {
    fmt.Printf("%.2f"u8, Δmath.Tanh(0));
}

// Output: 0.00
public static void ExampleSqrt() {
    UntypedInt a = 3;
    UntypedInt b = 4;
    var c = Δmath.Sqrt(a * a + b * b);
    fmt.Printf("%.1f"u8, c);
}

// Output: 5.0
public static void ExampleCeil() {
    var c = Δmath.Ceil(1.49D);
    fmt.Printf("%.1f"u8, c);
}

// Output: 2.0
public static void ExampleFloor() {
    var c = Δmath.Floor(1.51D);
    fmt.Printf("%.1f"u8, c);
}

// Output: 1.0
public static void ExamplePow() {
    var c = Δmath.Pow(2, 3);
    fmt.Printf("%.1f"u8, c);
}

// Output: 8.0
public static void ExamplePow10() {
    var c = Δmath.Pow10(2);
    fmt.Printf("%.1f"u8, c);
}

// Output: 100.0
public static void ExampleRound() {
    var p = Δmath.Round(10.5D);
    fmt.Printf("%.1f\n"u8, p);
    var n = Δmath.Round(-10.5D);
    fmt.Printf("%.1f\n"u8, n);
}

// Output:
// 11.0
// -11.0
public static void ExampleRoundToEven() {
    var u = Δmath.RoundToEven(11.5D);
    fmt.Printf("%.1f\n"u8, u);
    var d = Δmath.RoundToEven(12.5D);
    fmt.Printf("%.1f\n"u8, d);
}

// Output:
// 12.0
// 12.0
public static void ExampleLog() {
    var x = Δmath.Log(1);
    fmt.Printf("%.1f\n"u8, x);
    var y = Δmath.Log(2.7183D);
    fmt.Printf("%.1f\n"u8, y);
}

// Output:
// 0.0
// 1.0
public static void ExampleLog2() {
    fmt.Printf("%.1f"u8, Δmath.Log2(256));
}

// Output: 8.0
public static void ExampleLog10() {
    fmt.Printf("%.1f"u8, Δmath.Log10(100));
}

// Output: 2.0
public static void ExampleRemainder() {
    fmt.Printf("%.1f"u8, Δmath.Remainder(100, 30));
}

// Output: 10.0
public static void ExampleMod() {
    var c = Δmath.Mod(7, 4);
    fmt.Printf("%.1f"u8, c);
}

// Output: 3.0
public static void ExampleAbs() {
    var x = Δmath.Abs(-2D);
    fmt.Printf("%.1f\n"u8, x);
    var y = Δmath.Abs(2);
    fmt.Printf("%.1f\n"u8, y);
}

// Output:
// 2.0
// 2.0
public static void ExampleDim() {
    fmt.Printf("%.2f\n"u8, Δmath.Dim(4, -2D));
    fmt.Printf("%.2f\n"u8, Δmath.Dim(-4D, 2));
}

// Output:
// 6.00
// 0.00
public static void ExampleExp() {
    fmt.Printf("%.2f\n"u8, Δmath.Exp(1));
    fmt.Printf("%.2f\n"u8, Δmath.Exp(2));
    fmt.Printf("%.2f\n"u8, Δmath.Exp(-1D));
}

// Output:
// 2.72
// 7.39
// 0.37
public static void ExampleExp2() {
    fmt.Printf("%.2f\n"u8, Δmath.Exp2(1));
    fmt.Printf("%.2f\n"u8, Δmath.Exp2(-3D));
}

// Output:
// 2.00
// 0.12
public static void ExampleExpm1() {
    fmt.Printf("%.6f\n"u8, Δmath.Expm1(0.01D));
    fmt.Printf("%.6f\n"u8, Δmath.Expm1(-1D));
}

// Output:
// 0.010050
// -0.632121
public static void ExampleTrunc() {
    fmt.Printf("%.2f\n"u8, Δmath.Trunc(Δmath.Pi));
    fmt.Printf("%.2f\n"u8, Δmath.Trunc(-1.2345D));
}

// Output:
// 3.00
// -1.00
public static void ExampleCbrt() {
    fmt.Printf("%.2f\n"u8, Δmath.Cbrt(8));
    fmt.Printf("%.2f\n"u8, Δmath.Cbrt(27));
}

// Output:
// 2.00
// 3.00
public static void ExampleModf() {
    var (@int, frac) = Δmath.Modf(3.14D);
    fmt.Printf("%.2f, %.2f\n"u8, @int, frac);
    (@int, frac) = Δmath.Modf(-2.71D);
    fmt.Printf("%.2f, %.2f\n"u8, @int, frac);
}

// Output:
// 3.00, 0.14
// -2.00, -0.71

} // end math_test_package
