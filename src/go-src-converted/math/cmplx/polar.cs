// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

partial class cmplx_package {

// Polar returns the absolute value r and phase θ of x,
// such that x = r * e**θi.
// The phase is in the range [-Pi, Pi].
public static (float64 r, float64 θ) Polar(complex128 x) {
    float64 r = default!;
    float64 θ = default!;

    return (Abs(x), Phase(x));
}

} // end cmplx_package
