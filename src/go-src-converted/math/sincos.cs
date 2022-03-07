// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:11 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\sincos.go


namespace go;

public static partial class math_package {

    // Coefficients _sin[] and _cos[] are found in pkg/math/sin.go.

    // Sincos returns Sin(x), Cos(x).
    //
    // Special cases are:
    //    Sincos(±0) = ±0, 1
    //    Sincos(±Inf) = NaN, NaN
    //    Sincos(NaN) = NaN, NaN
public static (double, double) Sincos(double x) {
    double sin = default;
    double cos = default;

    const float PI4A = 7.85398125648498535156e-1F; // 0x3fe921fb40000000, Pi/4 split into three parts
    const float PI4B = 3.77489470793079817668e-8F; // 0x3e64442d00000000,
    const float PI4C = 2.69515142907905952645e-15F; // 0x3ce8469898cc5170, 
    // special cases

    if (x == 0) 
        return (x, 1); // return ±0.0, 1.0
    else if (IsNaN(x) || IsInf(x, 0)) 
        return (NaN(), NaN());
    // make argument positive
    var sinSign = false;
    var cosSign = false;
    if (x < 0) {
        x = -x;
        sinSign = true;
    }
    ulong j = default;
    double y = default;    double z = default;

    if (x >= reduceThreshold) {
        j, z = trigReduce(x);
    }
    else
 {
        j = uint64(x * (4 / Pi)); // integer part of x/(Pi/4), as integer for tests on the phase angle
        y = float64(j); // integer part of x/(Pi/4), as float

        if (j & 1 == 1) { // map zeros to origin
            j++;
            y++;

        }
        j &= 7; // octant modulo 2Pi radians (360 degrees)
        z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
    }
    if (j > 3) { // reflect in x axis
        j -= 4;
        (sinSign, cosSign) = (!sinSign, !cosSign);
    }
    if (j > 1) {
        cosSign = !cosSign;
    }
    var zz = z * z;
    cos = 1.0F - 0.5F * zz + zz * zz * ((((((_cos[0] * zz) + _cos[1]) * zz + _cos[2]) * zz + _cos[3]) * zz + _cos[4]) * zz + _cos[5]);
    sin = z + z * zz * ((((((_sin[0] * zz) + _sin[1]) * zz + _sin[2]) * zz + _sin[3]) * zz + _sin[4]) * zz + _sin[5]);
    if (j == 1 || j == 2) {
        (sin, cos) = (cos, sin);
    }
    if (cosSign) {
        cos = -cos;
    }
    if (sinSign) {
        sin = -sin;
    }
    return ;

}

} // end math_package
