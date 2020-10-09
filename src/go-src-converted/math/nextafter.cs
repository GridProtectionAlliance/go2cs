// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 05:07:45 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\nextafter.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Nextafter32 returns the next representable float32 value after x towards y.
        //
        // Special cases are:
        //    Nextafter32(x, x)   = x
        //    Nextafter32(NaN, y) = NaN
        //    Nextafter32(x, NaN) = NaN
        public static float Nextafter32(float x, float y)
        {
            float r = default;


            if (IsNaN(float64(x)) || IsNaN(float64(y))) // special case
                r = float32(NaN());
            else if (x == y) 
                r = x;
            else if (x == 0L) 
                r = float32(Copysign(float64(Float32frombits(1L)), float64(y)));
            else if ((y > x) == (x > 0L)) 
                r = Float32frombits(Float32bits(x) + 1L);
            else 
                r = Float32frombits(Float32bits(x) - 1L);
                        return ;

        }

        // Nextafter returns the next representable float64 value after x towards y.
        //
        // Special cases are:
        //    Nextafter(x, x)   = x
        //    Nextafter(NaN, y) = NaN
        //    Nextafter(x, NaN) = NaN
        public static double Nextafter(double x, double y)
        {
            double r = default;


            if (IsNaN(x) || IsNaN(y)) // special case
                r = NaN();
            else if (x == y) 
                r = x;
            else if (x == 0L) 
                r = Copysign(Float64frombits(1L), y);
            else if ((y > x) == (x > 0L)) 
                r = Float64frombits(Float64bits(x) + 1L);
            else 
                r = Float64frombits(Float64bits(x) - 1L);
                        return ;

        }
    }
}
