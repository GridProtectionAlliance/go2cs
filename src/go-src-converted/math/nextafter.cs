// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:42:03 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\nextafter.go
namespace go;

public static partial class math_package {

// Nextafter32 returns the next representable float32 value after x towards y.
//
// Special cases are:
//    Nextafter32(x, x)   = x
//    Nextafter32(NaN, y) = NaN
//    Nextafter32(x, NaN) = NaN
public static float Nextafter32(float x, float y) {
    float r = default;


    if (IsNaN(float64(x)) || IsNaN(float64(y))) // special case
        r = float32(NaN());
    else if (x == y) 
        r = x;
    else if (x == 0) 
        r = float32(Copysign(float64(Float32frombits(1)), float64(y)));
    else if ((y > x) == (x > 0)) 
        r = Float32frombits(Float32bits(x) + 1);
    else 
        r = Float32frombits(Float32bits(x) - 1);
        return ;
}

// Nextafter returns the next representable float64 value after x towards y.
//
// Special cases are:
//    Nextafter(x, x)   = x
//    Nextafter(NaN, y) = NaN
//    Nextafter(x, NaN) = NaN
public static double Nextafter(double x, double y) {
    double r = default;


    if (IsNaN(x) || IsNaN(y)) // special case
        r = NaN();
    else if (x == y) 
        r = x;
    else if (x == 0) 
        r = Copysign(Float64frombits(1), y);
    else if ((y > x) == (x > 0)) 
        r = Float64frombits(Float64bits(x) + 1);
    else 
        r = Float64frombits(Float64bits(x) - 1);
        return ;
}

} // end math_package
