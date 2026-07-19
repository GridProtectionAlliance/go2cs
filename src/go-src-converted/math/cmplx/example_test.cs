// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using math = math_package;
using cmplx = go.math.cmplx_package;
using go.math;

partial class cmplx_test_package {

public static void ExampleAbs() {
    fmt.Printf("%.1f"u8, cmplx.Abs(3D + 4D.i()));
}

// Output: 5.0

// ExampleExp computes Euler's identity.
public static void ExampleExp() {
    fmt.Printf("%.1f"u8, cmplx.Exp(1D.i() * math.Pi) + 1);
}

// Output: (0.0+0.0i)
public static void ExamplePolar() {
    var (r, theta) = cmplx.Polar(2D.i());
    fmt.Printf("r: %.1f, θ: %.1f*π"u8, r, theta / (float64)math.Pi);
}

// Output: r: 2.0, θ: 0.5*π

} // end cmplx_test_package
