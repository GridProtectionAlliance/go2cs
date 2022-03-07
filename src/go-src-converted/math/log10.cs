// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:09 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\log10.go


namespace go;

public static partial class math_package {

    // Log10 returns the decimal logarithm of x.
    // The special cases are the same as for Log.
public static double Log10(double x) {
    if (haveArchLog10) {
        return archLog10(x);
    }
    return log10(x);

}

private static double log10(double x) {
    return Log(x) * (1 / Ln10);
}

// Log2 returns the binary logarithm of x.
// The special cases are the same as for Log.
public static double Log2(double x) {
    if (haveArchLog2) {
        return archLog2(x);
    }
    return log2(x);

}

private static double log2(double x) {
    var (frac, exp) = Frexp(x); 
    // Make sure exact powers of two give an exact answer.
    // Don't depend on Log(0.5)*(1/Ln2)+exp being exactly exp-1.
    if (frac == 0.5F) {
        return float64(exp - 1);
    }
    return Log(frac) * (1 / Ln2) + float64(exp);

}

} // end math_package
