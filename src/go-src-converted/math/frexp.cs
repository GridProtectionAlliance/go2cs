// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Frexp breaks f into a normalized fraction
// and an integral power of two.
// It returns frac and exp satisfying f == frac × 2**exp,
// with the absolute value of frac in the interval [½, 1).
//
// Special cases are:
//
//	Frexp(±0) = ±0, 0
//	Frexp(±Inf) = ±Inf, 0
//	Frexp(NaN) = NaN, 0
public static (float64 frac, nint exp) Frexp(float64 f) {
    float64 frac = default!;
    nint exp = default!;

    if (haveArchFrexp) {
        return archFrexp(f);
    }
    return frexp(f);
}

internal static (float64 frac, nint exp) frexp(float64 f) {
    float64 frac = default!;
    nint exp = default!;

    // special cases
    switch (ᐧ) {
    case {} when f is 0: {
        return (f, 0);
    }
    case {} when IsInf(f, // correctly return -0
 0) || IsNaN(f): {
        return (f, 0);
    }}

    (f, exp) = normalize(f);
    var x = Float64bits(f);
    exp += ((nint)((uint64)((x >> (int)(shift)) & mask))) - bias + 1;
    x &= ~(uint64)(mask << (int)(shift));
    x |= (uint64)((-1 + bias) << (int)(shift));
    frac = Float64frombits(x);
    return (frac, exp);
}

} // end math_package
