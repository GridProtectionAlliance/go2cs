// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:07 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\j1.go


namespace go;

public static partial class math_package {

    /*
        Bessel function of the first and second kinds of order one.
    */

    // The original C code and the long comment below are
    // from FreeBSD's /usr/src/lib/msun/src/e_j1.c and
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
    // __ieee754_j1(x), __ieee754_y1(x)
    // Bessel function of the first and second kinds of order one.
    // Method -- j1(x):
    //      1. For tiny x, we use j1(x) = x/2 - x**3/16 + x**5/384 - ...
    //      2. Reduce x to |x| since j1(x)=-j1(-x),  and
    //         for x in (0,2)
    //              j1(x) = x/2 + x*z*R0/S0,  where z = x*x;
    //         (precision:  |j1/x - 1/2 - R0/S0 |<2**-61.51 )
    //         for x in (2,inf)
    //              j1(x) = sqrt(2/(pi*x))*(p1(x)*cos(x1)-q1(x)*sin(x1))
    //              y1(x) = sqrt(2/(pi*x))*(p1(x)*sin(x1)+q1(x)*cos(x1))
    //         where x1 = x-3*pi/4. It is better to compute sin(x1),cos(x1)
    //         as follow:
    //              cos(x1) =  cos(x)cos(3pi/4)+sin(x)sin(3pi/4)
    //                      =  1/sqrt(2) * (sin(x) - cos(x))
    //              sin(x1) =  sin(x)cos(3pi/4)-cos(x)sin(3pi/4)
    //                      = -1/sqrt(2) * (sin(x) + cos(x))
    //         (To avoid cancellation, use
    //              sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
    //         to compute the worse one.)
    //
    //      3 Special cases
    //              j1(nan)= nan
    //              j1(0) = 0
    //              j1(inf) = 0
    //
    // Method -- y1(x):
    //      1. screen out x<=0 cases: y1(0)=-inf, y1(x<0)=NaN
    //      2. For x<2.
    //         Since
    //              y1(x) = 2/pi*(j1(x)*(ln(x/2)+Euler)-1/x-x/2+5/64*x**3-...)
    //         therefore y1(x)-2/pi*j1(x)*ln(x)-1/x is an odd function.
    //         We use the following function to approximate y1,
    //              y1(x) = x*U(z)/V(z) + (2/pi)*(j1(x)*ln(x)-1/x), z= x**2
    //         where for x in [0,2] (abs err less than 2**-65.89)
    //              U(z) = U0[0] + U0[1]*z + ... + U0[4]*z**4
    //              V(z) = 1  + v0[0]*z + ... + v0[4]*z**5
    //         Note: For tiny x, 1/x dominate y1 and hence
    //              y1(tiny) = -2/pi/tiny, (choose tiny<2**-54)
    //      3. For x>=2.
    //               y1(x) = sqrt(2/(pi*x))*(p1(x)*sin(x1)+q1(x)*cos(x1))
    //         where x1 = x-3*pi/4. It is better to compute sin(x1),cos(x1)
    //         by method mentioned above.

    // J1 returns the order-one Bessel function of the first kind.
    //
    // Special cases are:
    //    J1(±Inf) = 0
    //    J1(NaN) = NaN
public static double J1(double x) {
    const float TwoM27 = 1.0F / (1 << 27); // 2**-27 0x3e40000000000000
    const nint Two129 = 1 << 129; // 2**129 0x4800000000000000
    // R0/S0 on [0, 2]
    const float R00 = -6.25000000000000000000e-02F; // 0xBFB0000000000000
    const float R01 = 1.40705666955189706048e-03F; // 0x3F570D9F98472C61
    const float R02 = -1.59955631084035597520e-05F; // 0xBEF0C5C6BA169668
    const float R03 = 4.96727999609584448412e-08F; // 0x3E6AAAFA46CA0BD9
    const float S01 = 1.91537599538363460805e-02F; // 0x3F939D0B12637E53
    const float S02 = 1.85946785588630915560e-04F; // 0x3F285F56B9CDF664
    const float S03 = 1.17718464042623683263e-06F; // 0x3EB3BFF8333F8498
    const float S04 = 5.04636257076217042715e-09F; // 0x3E35AC88C97DFF2C
    const float S05 = 1.23542274426137913908e-11F; // 0x3DAB2ACFCFB97ED8 
    // special cases

    if (IsNaN(x)) 
        return x;
    else if (IsInf(x, 0) || x == 0) 
        return 0;
        var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    if (x >= 2) {
        var (s, c) = Sincos(x);
        var ss = -s - c;
        var cc = s - c; 

        // make sure x+x does not overflow
        if (x < MaxFloat64 / 2) {
            var z = Cos(x + x);
            if (s * c > 0) {
                cc = z / ss;
            }
            else
 {
                ss = z / cc;
            }
        }
        z = default;
        if (x > Two129) {
            z = (1 / SqrtPi) * cc / Sqrt(x);
        }
        else
 {
            var u = pone(x);
            var v = qone(x);
            z = (1 / SqrtPi) * (u * cc - v * ss) / Sqrt(x);
        }
        if (sign) {
            return -z;
        }
        return z;

    }
    if (x < TwoM27) { // |x|<2**-27
        return 0.5F * x; // inexact if x!=0 necessary
    }
    z = x * x;
    var r = z * (R00 + z * (R01 + z * (R02 + z * R03)));
    float s = 1.0F + z * (S01 + z * (S02 + z * (S03 + z * (S04 + z * S05))));
    r *= x;
    z = 0.5F * x + r / s;
    if (sign) {
        return -z;
    }
    return z;

}

// Y1 returns the order-one Bessel function of the second kind.
//
// Special cases are:
//    Y1(+Inf) = 0
//    Y1(0) = -Inf
//    Y1(x < 0) = NaN
//    Y1(NaN) = NaN
public static double Y1(double x) {
    const float TwoM54 = 1.0F / (1 << 54); // 2**-54 0x3c90000000000000
    const nint Two129 = 1 << 129; // 2**129 0x4800000000000000
    const float U00 = -1.96057090646238940668e-01F; // 0xBFC91866143CBC8A
    const float U01 = 5.04438716639811282616e-02F; // 0x3FA9D3C776292CD1
    const float U02 = -1.91256895875763547298e-03F; // 0xBF5F55E54844F50F
    const float U03 = 2.35252600561610495928e-05F; // 0x3EF8AB038FA6B88E
    const float U04 = -9.19099158039878874504e-08F; // 0xBE78AC00569105B8
    const float V00 = 1.99167318236649903973e-02F; // 0x3F94650D3F4DA9F0
    const float V01 = 2.02552581025135171496e-04F; // 0x3F2A8C896C257764
    const float V02 = 1.35608801097516229404e-06F; // 0x3EB6C05A894E8CA6
    const float V03 = 6.22741452364621501295e-09F; // 0x3E3ABF1D5BA69A86
    const float V04 = 1.66559246207992079114e-11F; // 0x3DB25039DACA772A 
    // special cases

    if (x < 0 || IsNaN(x)) 
        return NaN();
    else if (IsInf(x, 1)) 
        return 0;
    else if (x == 0) 
        return Inf(-1);
        if (x >= 2) {
        var (s, c) = Sincos(x);
        var ss = -s - c;
        var cc = s - c; 

        // make sure x+x does not overflow
        if (x < MaxFloat64 / 2) {
            var z = Cos(x + x);
            if (s * c > 0) {
                cc = z / ss;
            }
            else
 {
                ss = z / cc;
            }

        }
        z = default;
        if (x > Two129) {
            z = (1 / SqrtPi) * ss / Sqrt(x);
        }
        else
 {
            var u = pone(x);
            var v = qone(x);
            z = (1 / SqrtPi) * (u * ss + v * cc) / Sqrt(x);
        }
        return z;

    }
    if (x <= TwoM54) { // x < 2**-54
        return -(2 / Pi) / x;

    }
    z = x * x;
    u = U00 + z * (U01 + z * (U02 + z * (U03 + z * U04)));
    v = 1 + z * (V00 + z * (V01 + z * (V02 + z * (V03 + z * V04))));
    return x * (u / v) + (2 / Pi) * (J1(x) * Log(x) - 1 / x);

}

// For x >= 8, the asymptotic expansions of pone is
//      1 + 15/128 s**2 - 4725/2**15 s**4 - ..., where s = 1/x.
// We approximate pone by
//      pone(x) = 1 + (R/S)
// where R = pr0 + pr1*s**2 + pr2*s**4 + ... + pr5*s**10
//       S = 1 + ps0*s**2 + ... + ps4*s**10
// and
//      | pone(x)-1-R/S | <= 2**(-60.06)

// for x in [inf, 8]=1/[0,0.125]
private static array<double> p1R8 = new array<double>(new double[] { 0.00000000000000000000e+00, 1.17187499999988647970e-01, 1.32394806593073575129e+01, 4.12051854307378562225e+02, 3.87474538913960532227e+03, 7.91447954031891731574e+03 });
private static array<double> p1S8 = new array<double>(new double[] { 1.14207370375678408436e+02, 3.65093083420853463394e+03, 3.69562060269033463555e+04, 9.76027935934950801311e+04, 3.08042720627888811578e+04 });

// for x in [8,4.5454] = 1/[0.125,0.22001]
private static array<double> p1R5 = new array<double>(new double[] { 1.31990519556243522749e-11, 1.17187493190614097638e-01, 6.80275127868432871736e+00, 1.08308182990189109773e+02, 5.17636139533199752805e+02, 5.28715201363337541807e+02 });
private static array<double> p1S5 = new array<double>(new double[] { 5.92805987221131331921e+01, 9.91401418733614377743e+02, 5.35326695291487976647e+03, 7.84469031749551231769e+03, 1.50404688810361062679e+03 });

// for x in[4.5453,2.8571] = 1/[0.2199,0.35001]
private static array<double> p1R3 = new array<double>(new double[] { 3.02503916137373618024e-09, 1.17186865567253592491e-01, 3.93297750033315640650e+00, 3.51194035591636932736e+01, 9.10550110750781271918e+01, 4.85590685197364919645e+01 });
private static array<double> p1S3 = new array<double>(new double[] { 3.47913095001251519989e+01, 3.36762458747825746741e+02, 1.04687139975775130551e+03, 8.90811346398256432622e+02, 1.03787932439639277504e+02 });

// for x in [2.8570,2] = 1/[0.3499,0.5]
private static array<double> p1R2 = new array<double>(new double[] { 1.07710830106873743082e-07, 1.17176219462683348094e-01, 2.36851496667608785174e+00, 1.22426109148261232917e+01, 1.76939711271687727390e+01, 5.07352312588818499250e+00 });
private static array<double> p1S2 = new array<double>(new double[] { 2.14364859363821409488e+01, 1.25290227168402751090e+02, 2.32276469057162813669e+02, 1.17679373287147100768e+02, 8.36463893371618283368e+00 });

private static double pone(double x) {
    ptr<array<double>> p;
    ptr<array<double>> q;
    if (x >= 8) {
        p = _addr_p1R8;
        q = _addr_p1S8;
    }
    else if (x >= 4.5454F) {
        p = _addr_p1R5;
        q = _addr_p1S5;
    }
    else if (x >= 2.8571F) {
        p = _addr_p1R3;
        q = _addr_p1S3;
    }
    else if (x >= 2) {
        p = _addr_p1R2;
        q = _addr_p1S2;
    }
    nint z = 1 / (x * x);
    var r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
    float s = 1.0F + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * q[4]))));
    return 1 + r / s;

}

// For x >= 8, the asymptotic expansions of qone is
//      3/8 s - 105/1024 s**3 - ..., where s = 1/x.
// We approximate qone by
//      qone(x) = s*(0.375 + (R/S))
// where R = qr1*s**2 + qr2*s**4 + ... + qr5*s**10
//       S = 1 + qs1*s**2 + ... + qs6*s**12
// and
//      | qone(x)/s -0.375-R/S | <= 2**(-61.13)

// for x in [inf, 8] = 1/[0,0.125]
private static array<double> q1R8 = new array<double>(new double[] { 0.00000000000000000000e+00, -1.02539062499992714161e-01, -1.62717534544589987888e+01, -7.59601722513950107896e+02, -1.18498066702429587167e+04, -4.84385124285750353010e+04 });
private static array<double> q1S8 = new array<double>(new double[] { 1.61395369700722909556e+02, 7.82538599923348465381e+03, 1.33875336287249578163e+05, 7.19657723683240939863e+05, 6.66601232617776375264e+05, -2.94490264303834643215e+05 });

// for x in [8,4.5454] = 1/[0.125,0.22001]
private static array<double> q1R5 = new array<double>(new double[] { -2.08979931141764104297e-11, -1.02539050241375426231e-01, -8.05644828123936029840e+00, -1.83669607474888380239e+02, -1.37319376065508163265e+03, -2.61244440453215656817e+03 });
private static array<double> q1S5 = new array<double>(new double[] { 8.12765501384335777857e+01, 1.99179873460485964642e+03, 1.74684851924908907677e+04, 4.98514270910352279316e+04, 2.79480751638918118260e+04, -4.71918354795128470869e+03 });

// for x in [4.5454,2.8571] = 1/[0.2199,0.35001] ???
private static array<double> q1R3 = new array<double>(new double[] { -5.07831226461766561369e-09, -1.02537829820837089745e-01, -4.61011581139473403113e+00, -5.78472216562783643212e+01, -2.28244540737631695038e+02, -2.19210128478909325622e+02 });
private static array<double> q1S3 = new array<double>(new double[] { 4.76651550323729509273e+01, 6.73865112676699709482e+02, 3.38015286679526343505e+03, 5.54772909720722782367e+03, 1.90311919338810798763e+03, -1.35201191444307340817e+02 });

// for x in [2.8570,2] = 1/[0.3499,0.5]
private static array<double> q1R2 = new array<double>(new double[] { -1.78381727510958865572e-07, -1.02517042607985553460e-01, -2.75220568278187460720e+00, -1.96636162643703720221e+01, -4.23253133372830490089e+01, -2.13719211703704061733e+01 });
private static array<double> q1S2 = new array<double>(new double[] { 2.95333629060523854548e+01, 2.52981549982190529136e+02, 7.57502834868645436472e+02, 7.39393205320467245656e+02, 1.55949003336666123687e+02, -4.95949898822628210127e+00 });

private static double qone(double x) {
    ptr<array<double>> p;    ptr<array<double>> q;

    if (x >= 8) {
        p = _addr_q1R8;
        q = _addr_q1S8;
    }
    else if (x >= 4.5454F) {
        p = _addr_q1R5;
        q = _addr_q1S5;
    }
    else if (x >= 2.8571F) {
        p = _addr_q1R3;
        q = _addr_q1S3;
    }
    else if (x >= 2) {
        p = _addr_q1R2;
        q = _addr_q1S2;
    }
    nint z = 1 / (x * x);
    var r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
    nint s = 1 + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * (q[4] + z * q[5])))));
    return (0.375F + r / s) / x;

}

} // end math_package
