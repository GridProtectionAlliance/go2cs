// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:02 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\atan2.go


namespace go;

public static partial class math_package {

    // Atan2 returns the arc tangent of y/x, using
    // the signs of the two to determine the quadrant
    // of the return value.
    //
    // Special cases are (in order):
    //    Atan2(y, NaN) = NaN
    //    Atan2(NaN, x) = NaN
    //    Atan2(+0, x>=0) = +0
    //    Atan2(-0, x>=0) = -0
    //    Atan2(+0, x<=-0) = +Pi
    //    Atan2(-0, x<=-0) = -Pi
    //    Atan2(y>0, 0) = +Pi/2
    //    Atan2(y<0, 0) = -Pi/2
    //    Atan2(+Inf, +Inf) = +Pi/4
    //    Atan2(-Inf, +Inf) = -Pi/4
    //    Atan2(+Inf, -Inf) = 3Pi/4
    //    Atan2(-Inf, -Inf) = -3Pi/4
    //    Atan2(y, +Inf) = 0
    //    Atan2(y>0, -Inf) = +Pi
    //    Atan2(y<0, -Inf) = -Pi
    //    Atan2(+Inf, x) = +Pi/2
    //    Atan2(-Inf, x) = -Pi/2
public static double Atan2(double y, double x) {
    if (haveArchAtan2) {
        return archAtan2(y, x);
    }
    return atan2(y, x);

}

private static double atan2(double y, double x) { 
    // special cases

    if (IsNaN(y) || IsNaN(x)) 
        return NaN();
    else if (y == 0) 
        if (x >= 0 && !Signbit(x)) {
            return Copysign(0, y);
        }
        return Copysign(Pi, y);
    else if (x == 0) 
        return Copysign(Pi / 2, y);
    else if (IsInf(x, 0)) 
        if (IsInf(x, 1)) {

            if (IsInf(y, 0)) 
                return Copysign(Pi / 4, y);
            else 
                return Copysign(0, y);
            
        }

        if (IsInf(y, 0)) 
            return Copysign(3 * Pi / 4, y);
        else 
            return Copysign(Pi, y);
            else if (IsInf(y, 0)) 
        return Copysign(Pi / 2, y);
    // Call atan and determine the quadrant.
    var q = Atan(y / x);
    if (x < 0) {
        if (q <= 0) {
            return q + Pi;
        }
        return q - Pi;

    }
    return q;

}

} // end math_package
