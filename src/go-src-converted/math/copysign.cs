// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 13 05:41:55 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\copysign.go
namespace go;

public static partial class math_package {

// Copysign returns a value with the magnitude
// of x and the sign of y.
public static double Copysign(double x, double y) {
    const nint sign = 1 << 63;

    return Float64frombits(Float64bits(x) & ~sign | Float64bits(y) & sign);
}

} // end math_package
