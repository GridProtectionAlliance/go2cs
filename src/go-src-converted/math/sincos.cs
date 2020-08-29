// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !386

// package math -- go2cs converted at 2020 August 29 08:44:58 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\sincos.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Coefficients _sin[] and _cos[] are found in pkg/math/sin.go.

        // Sincos returns Sin(x), Cos(x).
        //
        // Special cases are:
        //    Sincos(±0) = ±0, 1
        //    Sincos(±Inf) = NaN, NaN
        //    Sincos(NaN) = NaN, NaN
        public static (double, double) Sincos(double x)
        {
            const float PI4A = 7.85398125648498535156E-1F; // 0x3fe921fb40000000, Pi/4 split into three parts
            const float PI4B = 3.77489470793079817668E-8F; // 0x3e64442d00000000,
            const float PI4C = 2.69515142907905952645E-15F; // 0x3ce8469898cc5170,
            const float M4PI = 1.273239544735162542821171882678754627704620361328125F; // 4/pi 
            // special cases

            if (x == 0L) 
                return (x, 1L); // return ±0.0, 1.0
            else if (IsNaN(x) || IsInf(x, 0L)) 
                return (NaN(), NaN());
            // make argument positive
            var sinSign = false;
            var cosSign = false;
            if (x < 0L)
            {
                x = -x;
                sinSign = true;
            }
            var j = int64(x * M4PI); // integer part of x/(Pi/4), as integer for tests on the phase angle
            var y = float64(j); // integer part of x/(Pi/4), as float

            if (j & 1L == 1L)
            { // map zeros to origin
                j++;
                y++;
            }
            j &= 7L; // octant modulo 2Pi radians (360 degrees)
            if (j > 3L)
            { // reflect in x axis
                j -= 4L;
                sinSign = !sinSign;
                cosSign = !cosSign;
            }
            if (j > 1L)
            {
                cosSign = !cosSign;
            }
            var z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
            var zz = z * z;
            cos = 1.0F - 0.5F * zz + zz * zz * ((((((_cos[0L] * zz) + _cos[1L]) * zz + _cos[2L]) * zz + _cos[3L]) * zz + _cos[4L]) * zz + _cos[5L]);
            sin = z + z * zz * ((((((_sin[0L] * zz) + _sin[1L]) * zz + _sin[2L]) * zz + _sin[3L]) * zz + _sin[4L]) * zz + _sin[5L]);
            if (j == 1L || j == 2L)
            {
                sin = cos;
                cos = sin;
            }
            if (cosSign)
            {
                cos = -cos;
            }
            if (sinSign)
            {
                sin = -sin;
            }
            return;
        }
    }
}
