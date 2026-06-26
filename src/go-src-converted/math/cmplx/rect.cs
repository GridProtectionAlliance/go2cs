// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;

partial class cmplx_package {

// Rect returns the complex number x with polar coordinates r, θ.
public static complex128 Rect(float64 r, float64 θ) {
    var (s, c) = math.Sincos(θ);
    return complex(r * c, r * s);
}

} // end cmplx_package
