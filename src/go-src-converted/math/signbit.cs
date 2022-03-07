// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:10 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\signbit.go


namespace go;

public static partial class math_package {

    // Signbit reports whether x is negative or negative zero.
public static bool Signbit(double x) {
    return Float64bits(x) & (1 << 63) != 0;
}

} // end math_package
