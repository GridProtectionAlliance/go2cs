// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:53 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\jn.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        /*
            Bessel function of the first and second kinds of order n.
        */

        // The original C code and the long comment below are
        // from FreeBSD's /usr/src/lib/msun/src/e_jn.c and
        // came with this notice. The go code is a simplified
        // version of the original C.
        //
        // ====================================================
        // Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
        //
        // Developed at SunPro, a Sun Microsystems, Inc. business.
        // Permission to use, copy, modify, and distribute this
        // software is freely granted, provided that this notice
        // is preserved.
        // ====================================================
        //
        // __ieee754_jn(n, x), __ieee754_yn(n, x)
        // floating point Bessel's function of the 1st and 2nd kind
        // of order n
        //
        // Special cases:
        //      y0(0)=y1(0)=yn(n,0) = -inf with division by zero signal;
        //      y0(-ve)=y1(-ve)=yn(n,-ve) are NaN with invalid signal.
        // Note 2. About jn(n,x), yn(n,x)
        //      For n=0, j0(x) is called,
        //      for n=1, j1(x) is called,
        //      for n<x, forward recursion is used starting
        //      from values of j0(x) and j1(x).
        //      for n>x, a continued fraction approximation to
        //      j(n,x)/j(n-1,x) is evaluated and then backward
        //      recursion is used starting from a supposed value
        //      for j(n,x). The resulting value of j(0,x) is
        //      compared with the actual value to correct the
        //      supposed value of j(n,x).
        //
        //      yn(n,x) is similar in all respects, except
        //      that forward recursion is used for all
        //      values of n>1.

        // Jn returns the order-n Bessel function of the first kind.
        //
        // Special cases are:
        //    Jn(n, ±Inf) = 0
        //    Jn(n, NaN) = NaN
        public static double Jn(long n, double x)
        {
            const float TwoM29 = 1.0F / (1L << (int)(29L)); // 2**-29 0x3e10000000000000
            const long Two302 = 1L << (int)(302L); // 2**302 0x52D0000000000000 
            // special cases

            if (IsNaN(x)) 
                return x;
            else if (IsInf(x, 0L)) 
                return 0L;
            // J(-n, x) = (-1)**n * J(n, x), J(n, -x) = (-1)**n * J(n, x)
            // Thus, J(-n, x) = J(n, -x)

            if (n == 0L)
            {
                return J0(x);
            }
            if (x == 0L)
            {
                return 0L;
            }
            if (n < 0L)
            {
                n = -n;
                x = -x;
            }
            if (n == 1L)
            {
                return J1(x);
            }
            var sign = false;
            if (x < 0L)
            {
                x = -x;
                if (n & 1L == 1L)
                {
                    sign = true; // odd n and negative x
                }
            }
            double b = default;
            if (float64(n) <= x)
            { 
                // Safe to use J(n+1,x)=2n/x *J(n,x)-J(n-1,x)
                if (x >= Two302)
                { // x > 2**302

                    // (x >> n**2)
                    //          Jn(x) = cos(x-(2n+1)*pi/4)*sqrt(2/x*pi)
                    //          Yn(x) = sin(x-(2n+1)*pi/4)*sqrt(2/x*pi)
                    //          Let s=sin(x), c=cos(x),
                    //              xn=x-(2n+1)*pi/4, sqt2 = sqrt(2),then
                    //
                    //                 n    sin(xn)*sqt2    cos(xn)*sqt2
                    //              ----------------------------------
                    //                 0     s-c             c+s
                    //                 1    -s-c            -c+s
                    //                 2    -s+c            -c-s
                    //                 3     s+c             c-s

                    double temp = default;
                    switch (n & 3L)
                    {
                        case 0L: 
                            temp = Cos(x) + Sin(x);
                            break;
                        case 1L: 
                            temp = -Cos(x) + Sin(x);
                            break;
                        case 2L: 
                            temp = -Cos(x) - Sin(x);
                            break;
                        case 3L: 
                            temp = Cos(x) - Sin(x);
                            break;
                    }
                    b = (1L / SqrtPi) * temp / Sqrt(x);
                }
                else
                {
                    b = J1(x);
                    {
                        long i__prev1 = i;
                        var a__prev1 = a;

                        for (long i = 1L;
                        var a = J0(x); i < n; i++)
                        {
                            a = b;
                            b = b * (float64(i + i) / x) - a; // avoid underflow
                        }

                        i = i__prev1;
                        a = a__prev1;
                    }
                }
            else
            }            {
                if (x < TwoM29)
                { // x < 2**-29
                    // x is tiny, return the first Taylor expansion of J(n,x)
                    // J(n,x) = 1/n!*(x/2)**n  - ...

                    if (n > 33L)
                    { // underflow
                        b = 0L;
                    }
                    else
                    {
                        temp = x * 0.5F;
                        b = temp;
                        a = 1.0F;
                        {
                            long i__prev1 = i;

                            for (i = 2L; i <= n; i++)
                            {
                                a *= float64(i); // a = n!
                                b *= temp; // b = (x/2)**n
                            }

                            i = i__prev1;
                        }
                        b /= a;
                    }
                else
                }                { 
                    // use backward recurrence
                    //                      x      x**2      x**2
                    //  J(n,x)/J(n-1,x) =  ----   ------   ------   .....
                    //                      2n  - 2(n+1) - 2(n+2)
                    //
                    //                      1      1        1
                    //  (for large x)   =  ----  ------   ------   .....
                    //                      2n   2(n+1)   2(n+2)
                    //                      -- - ------ - ------ -
                    //                       x     x         x
                    //
                    // Let w = 2n/x and h=2/x, then the above quotient
                    // is equal to the continued fraction:
                    //                  1
                    //      = -----------------------
                    //                     1
                    //         w - -----------------
                    //                        1
                    //              w+h - ---------
                    //                     w+2h - ...
                    //
                    // To determine how many terms needed, let
                    // Q(0) = w, Q(1) = w(w+h) - 1,
                    // Q(k) = (w+k*h)*Q(k-1) - Q(k-2),
                    // When Q(k) > 1e4    good for single
                    // When Q(k) > 1e9    good for double
                    // When Q(k) > 1e17    good for quadruple

                    // determine k
                    var w = float64(n + n) / x;
                    long h = 2L / x;
                    var q0 = w;
                    var z = w + h;
                    var q1 = w * z - 1L;
                    long k = 1L;
                    while (q1 < 1e9F)
                    {
                        k++;
                        z += h;
                        q0 = q1;
                        q1 = z * q1 - q0;
                    }
                    var m = n + n;
                    float t = 0.0F;
                    {
                        long i__prev1 = i;

                        i = 2L * (n + k);

                        while (i >= m)
                        {
                            t = 1L / (float64(i) / x - t);
                            i -= 2L;
                        }

                        i = i__prev1;
                    }
                    a = t;
                    b = 1L; 
                    //  estimate log((2/x)**n*n!) = n*log(2/x)+n*ln(n)
                    //  Hence, if n*(log(2n/x)) > ...
                    //  single 8.8722839355e+01
                    //  double 7.09782712893383973096e+02
                    //  long double 1.1356523406294143949491931077970765006170e+04
                    //  then recurrent value may overflow and the result is
                    //  likely underflow to zero

                    var tmp = float64(n);
                    long v = 2L / x;
                    tmp = tmp * Log(Abs(v * tmp));
                    if (tmp < 7.09782712893383973096e+02F)
                    {
                        {
                            long i__prev1 = i;

                            for (i = n - 1L; i > 0L; i--)
                            {
                                var di = float64(i + i);
                                a = b;
                                b = b * di / x - a;
                            }
                    else


                            i = i__prev1;
                        }
                    }                    {
                        {
                            long i__prev1 = i;

                            for (i = n - 1L; i > 0L; i--)
                            {
                                di = float64(i + i);
                                a = b;
                                b = b * di / x - a; 
                                // scale b to avoid spurious overflow
                                if (b > 1e100F)
                                {
                                    a /= b;
                                    t /= b;
                                    b = 1L;
                                }
                            }

                            i = i__prev1;
                        }
                    }
                    b = t * J0(x) / b;
                }
            }
            if (sign)
            {
                return -b;
            }
            return b;
        }

        // Yn returns the order-n Bessel function of the second kind.
        //
        // Special cases are:
        //    Yn(n, +Inf) = 0
        //    Yn(n ≥ 0, 0) = -Inf
        //    Yn(n < 0, 0) = +Inf if n is odd, -Inf if n is even
        //    Yn(n, x < 0) = NaN
        //    Yn(n, NaN) = NaN
        public static double Yn(long n, double x)
        {
            const long Two302 = 1L << (int)(302L); // 2**302 0x52D0000000000000
            // special cases
 // 2**302 0x52D0000000000000
            // special cases

            if (x < 0L || IsNaN(x)) 
                return NaN();
            else if (IsInf(x, 1L)) 
                return 0L;
                        if (n == 0L)
            {
                return Y0(x);
            }
            if (x == 0L)
            {
                if (n < 0L && n & 1L == 1L)
                {
                    return Inf(1L);
                }
                return Inf(-1L);
            }
            var sign = false;
            if (n < 0L)
            {
                n = -n;
                if (n & 1L == 1L)
                {
                    sign = true; // sign true if n < 0 && |n| odd
                }
            }
            if (n == 1L)
            {
                if (sign)
                {
                    return -Y1(x);
                }
                return Y1(x);
            }
            double b = default;
            if (x >= Two302)
            { // x > 2**302
                // (x >> n**2)
                //        Jn(x) = cos(x-(2n+1)*pi/4)*sqrt(2/x*pi)
                //        Yn(x) = sin(x-(2n+1)*pi/4)*sqrt(2/x*pi)
                //        Let s=sin(x), c=cos(x),
                //        xn=x-(2n+1)*pi/4, sqt2 = sqrt(2),then
                //
                //           n    sin(xn)*sqt2    cos(xn)*sqt2
                //        ----------------------------------
                //           0     s-c         c+s
                //           1    -s-c         -c+s
                //           2    -s+c        -c-s
                //           3     s+c         c-s

                double temp = default;
                switch (n & 3L)
                {
                    case 0L: 
                        temp = Sin(x) - Cos(x);
                        break;
                    case 1L: 
                        temp = -Sin(x) - Cos(x);
                        break;
                    case 2L: 
                        temp = -Sin(x) + Cos(x);
                        break;
                    case 3L: 
                        temp = Sin(x) + Cos(x);
                        break;
                }
                b = (1L / SqrtPi) * temp / Sqrt(x);
            }
            else
            {
                var a = Y0(x);
                b = Y1(x); 
                // quit if b is -inf
                for (long i = 1L; i < n && !IsInf(b, -1L); i++)
                {
                    a = b;
                    b = (float64(i + i) / x) * b - a;
                }

            }
            if (sign)
            {
                return -b;
            }
            return b;
        }
    }
}
