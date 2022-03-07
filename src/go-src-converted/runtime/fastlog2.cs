// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:40 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\fastlog2.go


namespace go;

public static partial class runtime_package {

    // fastlog2 implements a fast approximation to the base 2 log of a
    // float64. This is used to compute a geometric distribution for heap
    // sampling, without introducing dependencies into package math. This
    // uses a very rough approximation using the float64 exponent and the
    // first 25 bits of the mantissa. The top 5 bits of the mantissa are
    // used to load limits from a table of constants and the rest are used
    // to scale linearly between them.
private static double fastlog2(double x) {
    const nint fastlogScaleBits = 20;

    const float fastlogScaleRatio = 1.0F / (1 << (int)(fastlogScaleBits));



    var xBits = float64bits(x); 
    // Extract the exponent from the IEEE float64, and index a constant
    // table with the first 10 bits from the mantissa.
    var xExp = int64((xBits >> 52) & 0x7FF) - 1023;
    var xManIndex = (xBits >> (int)((52 - fastlogNumBits))) % (1 << (int)(fastlogNumBits));
    var xManScale = (xBits >> (int)((52 - fastlogNumBits - fastlogScaleBits))) % (1 << (int)(fastlogScaleBits));

    var low = fastlog2Table[xManIndex];
    var high = fastlog2Table[xManIndex + 1];
    return float64(xExp) + low + (high - low) * float64(xManScale) * fastlogScaleRatio;

}

} // end runtime_package
