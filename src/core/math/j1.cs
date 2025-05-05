// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

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
//
//	J1(±Inf) = 0
//	J1(NaN) = NaN
public static float64 J1(float64 x) {
    static readonly UntypedFloat TwoM27 = /* 1.0 / (1 << 27) */ 7.45058e-09; // 2**-27 0x3e40000000000000
    GoUntyped Two129 = /* 1 << 129 */       // 2**129 0x4800000000000000
            GoUntyped.Parse("680564733841876926926749214863536422912");
    static readonly UntypedFloat R00 = -0.0625; // 0xBFB0000000000000
    static readonly UntypedFloat R01 = /* 1.40705666955189706048e-03 */ 0.00140706;   // 0x3F570D9F98472C61
    static readonly UntypedFloat R02 = /* -1.59955631084035597520e-05 */ -1.59956e-05; // 0xBEF0C5C6BA169668
    static readonly UntypedFloat R03 = /* 4.96727999609584448412e-08 */ 4.96728e-08;  // 0x3E6AAAFA46CA0BD9
    static readonly UntypedFloat S01 = /* 1.91537599538363460805e-02 */ 0.0191538;    // 0x3F939D0B12637E53
    static readonly UntypedFloat S02 = /* 1.85946785588630915560e-04 */ 0.000185947;  // 0x3F285F56B9CDF664
    static readonly UntypedFloat S03 = /* 1.17718464042623683263e-06 */ 1.17718e-06;  // 0x3EB3BFF8333F8498
    static readonly UntypedFloat S04 = /* 5.04636257076217042715e-09 */ 5.04636e-09;  // 0x3E35AC88C97DFF2C
    static readonly UntypedFloat S05 = /* 1.23542274426137913908e-11 */ 1.23542e-11;  // 0x3DAB2ACFCFB97ED8
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x): {
        return x;
    }
    case {} when IsInf(x, 0) || x == 0: {
        return 0;
    }}

    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    if (x >= 2) {
        var (sΔ1, c) = Sincos(x);
        var ss = -sΔ1 - c;
        var cc = sΔ1 - c;
        // make sure x+x does not overflow
        if (x < MaxFloat64 / 2) {
            var zΔ1 = Cos(x + x);
            if (sΔ1 * c > 0){
                cc = zΔ1 / ss;
            } else {
                ss = zΔ1 / cc;
            }
        }
        // j1(x) = 1/sqrt(pi) * (P(1,x)*cc - Q(1,x)*ss) / sqrt(x)
        // y1(x) = 1/sqrt(pi) * (P(1,x)*ss + Q(1,x)*cc) / sqrt(x)
        float64 z = default!;
        if (x > Two129){
            z = (1 / SqrtPi) * cc / Sqrt(x);
        } else {
            var u = pone(x);
            var v = qone(x);
            z = (1 / SqrtPi) * (u * cc - v * ss) / Sqrt(x);
        }
        if (sign) {
            return -z;
        }
        return z;
    }
    if (x < TwoM27) {
        // |x|<2**-27
        return 0.5F * x;
    }
    // inexact if x!=0 necessary
    var z = x * x;
    var r = z * (R00 + z * (R01 + z * (R02 + z * R03)));
    var s = 1.0F + z * (S01 + z * (S02 + z * (S03 + z * (S04 + z * S05))));
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
//
//	Y1(+Inf) = 0
//	Y1(0) = -Inf
//	Y1(x < 0) = NaN
//	Y1(NaN) = NaN
public static float64 Y1(float64 x) {
    static readonly UntypedFloat TwoM54 = /* 1.0 / (1 << 54) */ 5.55112e-17; // 2**-54 0x3c90000000000000
    GoUntyped Two129 = /* 1 << 129 */                   // 2**129 0x4800000000000000
            GoUntyped.Parse("680564733841876926926749214863536422912");
    static readonly UntypedFloat U00 = /* -1.96057090646238940668e-01 */ -0.196057; // 0xBFC91866143CBC8A
    static readonly UntypedFloat U01 = /* 5.04438716639811282616e-02 */ 0.0504439;    // 0x3FA9D3C776292CD1
    static readonly UntypedFloat U02 = /* -1.91256895875763547298e-03 */ -0.00191257; // 0xBF5F55E54844F50F
    static readonly UntypedFloat U03 = /* 2.35252600561610495928e-05 */ 2.35253e-05;  // 0x3EF8AB038FA6B88E
    static readonly UntypedFloat U04 = /* -9.19099158039878874504e-08 */ -9.19099e-08; // 0xBE78AC00569105B8
    static readonly UntypedFloat V00 = /* 1.99167318236649903973e-02 */ 0.0199167;    // 0x3F94650D3F4DA9F0
    static readonly UntypedFloat V01 = /* 2.02552581025135171496e-04 */ 0.000202553;  // 0x3F2A8C896C257764
    static readonly UntypedFloat V02 = /* 1.35608801097516229404e-06 */ 1.35609e-06;  // 0x3EB6C05A894E8CA6
    static readonly UntypedFloat V03 = /* 6.22741452364621501295e-09 */ 6.22741e-09;  // 0x3E3ABF1D5BA69A86
    static readonly UntypedFloat V04 = /* 1.66559246207992079114e-11 */ 1.66559e-11;  // 0x3DB25039DACA772A
    // special cases
    switch (ᐧ) {
    case {} when x < 0 || IsNaN(x): {
        return NaN();
    }
    case {} when IsInf(x, 1): {
        return 0;
    }
    case {} when x is 0: {
        return Inf(-1);
    }}

    if (x >= 2) {
        var (s, c) = Sincos(x);
        var ss = -s - c;
        var cc = s - c;
        // make sure x+x does not overflow
        if (x < MaxFloat64 / 2) {
            var zΔ1 = Cos(x + x);
            if (s * c > 0){
                cc = zΔ1 / ss;
            } else {
                ss = zΔ1 / cc;
            }
        }
        // y1(x) = sqrt(2/(pi*x))*(p1(x)*sin(x0)+q1(x)*cos(x0))
        // where x0 = x-3pi/4
        //     Better formula:
        //         cos(x0) = cos(x)cos(3pi/4)+sin(x)sin(3pi/4)
        //                 =  1/sqrt(2) * (sin(x) - cos(x))
        //         sin(x0) = sin(x)cos(3pi/4)-cos(x)sin(3pi/4)
        //                 = -1/sqrt(2) * (cos(x) + sin(x))
        // To avoid cancellation, use
        //     sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
        // to compute the worse one.
        float64 z = default!;
        if (x > Two129){
            z = (1 / SqrtPi) * ss / Sqrt(x);
        } else {
            var uΔ1 = pone(x);
            var vΔ1 = qone(x);
            z = (1 / SqrtPi) * (uΔ1 * ss + vΔ1 * cc) / Sqrt(x);
        }
        return z;
    }
    if (x <= TwoM54) {
        // x < 2**-54
        return -(2 / Pi) / x;
    }
    var z = x * x;
    var u = U00 + z * (U01 + z * (U02 + z * (U03 + z * U04)));
    var v = 1 + z * (V00 + z * (V01 + z * (V02 + z * (V03 + z * V04))));
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
// 0x0000000000000000
// 0x3FBDFFFFFFFFFCCE
// 0x402A7A9D357F7FCE
// 0x4079C0D4652EA590
// 0x40AE457DA3A532CC
// 0x40BEEA7AC32782DD
// for x in [inf, 8]=1/[0,0.125]
internal static array<float64> p1R8 = new float64[]{
    0.00000000000000000000e+00F,
    1.17187499999988647970e-01F,
    1.32394806593073575129e+01F,
    4.12051854307378562225e+02F,
    3.87474538913960532227e+03F,
    7.91447954031891731574e+03F
}.array();

// 0x405C8D458E656CAC
// 0x40AC85DC964D274F
// 0x40E20B8697C5BB7F
// 0x40F7D42CB28F17BB
// 0x40DE1511697A0B2D
internal static array<float64> p1S8 = new float64[]{
    1.14207370375678408436e+02F,
    3.65093083420853463394e+03F,
    3.69562060269033463555e+04F,
    9.76027935934950801311e+04F,
    3.08042720627888811578e+04F
}.array();

// 0x3DAD0667DAE1CA7D
// 0x3FBDFFFFE2C10043
// 0x401B36046E6315E3
// 0x405B13B9452602ED
// 0x40802D16D052D649
// 0x408085B8BB7E0CB7
// for x in [8,4.5454] = 1/[0.125,0.22001]
internal static array<float64> p1R5 = new float64[]{
    1.31990519556243522749e-11F,
    1.17187493190614097638e-01F,
    6.80275127868432871736e+00F,
    1.08308182990189109773e+02F,
    5.17636139533199752805e+02F,
    5.28715201363337541807e+02F
}.array();

// 0x404DA3EAA8AF633D
// 0x408EFB361B066701
// 0x40B4E9445706B6FB
// 0x40BEA4B0B8A5BB15
// 0x40978030036F5E51
internal static array<float64> p1S5 = new float64[]{
    5.92805987221131331921e+01F,
    9.91401418733614377743e+02F,
    5.35326695291487976647e+03F,
    7.84469031749551231769e+03F,
    1.50404688810361062679e+03F
}.array();

// 0x3E29FC21A7AD9EDD
// 0x3FBDFFF55B21D17B
// 0x400F76BCE85EAD8A
// 0x40418F489DA6D129
// 0x4056C3854D2C1837
// 0x4048478F8EA83EE5
// for x in[4.5453,2.8571] = 1/[0.2199,0.35001]
internal static array<float64> p1R3 = new float64[]{
    3.02503916137373618024e-09F,
    1.17186865567253592491e-01F,
    3.93297750033315640650e+00F,
    3.51194035591636932736e+01F,
    9.10550110750781271918e+01F,
    4.85590685197364919645e+01F
}.array();

// 0x40416549A134069C
// 0x40750C3307F1A75F
// 0x40905B7C5037D523
// 0x408BD67DA32E31E9
// 0x4059F26D7C2EED53
internal static array<float64> p1S3 = new float64[]{
    3.47913095001251519989e+01F,
    3.36762458747825746741e+02F,
    1.04687139975775130551e+03F,
    8.90811346398256432622e+02F,
    1.03787932439639277504e+02F
}.array();

// 0x3E7CE9D4F65544F4
// 0x3FBDFF42BE760D83
// 0x4002F2B7F98FAEC0
// 0x40287C377F71A964
// 0x4031B1A8177F8EE2
// 0x40144B49A574C1FE
// for x in [2.8570,2] = 1/[0.3499,0.5]
internal static array<float64> p1R2 = new float64[]{
    1.07710830106873743082e-07F,
    1.17176219462683348094e-01F,
    2.36851496667608785174e+00F,
    1.22426109148261232917e+01F,
    1.76939711271687727390e+01F,
    5.07352312588818499250e+00F
}.array();

// 0x40356FBD8AD5ECDC
// 0x405F529314F92CD5
// 0x406D08D8D5A2DBD9
// 0x405D6B7ADA1884A9
// 0x4020BAB1F44E5192
internal static array<float64> p1S2 = new float64[]{
    2.14364859363821409488e+01F,
    1.25290227168402751090e+02F,
    2.32276469057162813669e+02F,
    1.17679373287147100768e+02F,
    8.36463893371618283368e+00F
}.array();

internal static float64 pone(float64 x) {
    ж<array<float64>> p = default!;
    ж<array<float64>> q = default!;
    if (x >= 8){
        p = Ꮡ(p1R8);
        q = Ꮡ(p1S8);
    } else 
    if (x >= 4.5454F){
        p = Ꮡ(p1R5);
        q = Ꮡ(p1S5);
    } else 
    if (x >= 2.8571F){
        p = Ꮡ(p1R3);
        q = Ꮡ(p1S3);
    } else 
    if (x >= 2) {
        p = Ꮡ(p1R2);
        q = Ꮡ(p1S2);
    }
    var z = 1 / (x * x);
    var r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
    var s = 1.0F + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * q[4]))));
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
// 0x0000000000000000
// 0xBFBA3FFFFFFFFDF3
// 0xC0304591A26779F7
// 0xC087BCD053E4B576
// 0xC0C724E740F87415
// 0xC0E7A6D065D09C6A
// for x in [inf, 8] = 1/[0,0.125]
internal static array<float64> q1R8 = new float64[]{
    0.00000000000000000000e+00F,
    -1.02539062499992714161e-01F,
    -1.62717534544589987888e+01F,
    -7.59601722513950107896e+02F,
    -1.18498066702429587167e+04F,
    -4.84385124285750353010e+04F
}.array();

// 0x40642CA6DE5BCDE5
// 0x40BE9162D0D88419
// 0x4100579AB0B75E98
// 0x4125F65372869C19
// 0x412457D27719AD5C
// 0xC111F9690EA5AA18
internal static array<float64> q1S8 = new float64[]{
    1.61395369700722909556e+02F,
    7.82538599923348465381e+03F,
    1.33875336287249578163e+05F,
    7.19657723683240939863e+05F,
    6.66601232617776375264e+05F,
    -2.94490264303834643215e+05F
}.array();

// 0xBDB6FA431AA1A098
// 0xBFBA3FFFCB597FEF
// 0xC0201CE6CA03AD4B
// 0xC066F56D6CA7B9B0
// 0xC09574C66931734F
// 0xC0A468E388FDA79D
// for x in [8,4.5454] = 1/[0.125,0.22001]
internal static array<float64> q1R5 = new float64[]{
    -2.08979931141764104297e-11F,
    -1.02539050241375426231e-01F,
    -8.05644828123936029840e+00F,
    -1.83669607474888380239e+02F,
    -1.37319376065508163265e+03F,
    -2.61244440453215656817e+03F
}.array();

// 0x405451B2FF5A11B2
// 0x409F1F31E77BF839
// 0x40D10F1F0D64CE29
// 0x40E8576DAABAD197
// 0x40DB4B04CF7C364B
// 0xC0B26F2EFCFFA004
internal static array<float64> q1S5 = new float64[]{
    8.12765501384335777857e+01F,
    1.99179873460485964642e+03F,
    1.74684851924908907677e+04F,
    4.98514270910352279316e+04F,
    2.79480751638918118260e+04F,
    -4.71918354795128470869e+03F
}.array();

// 0xBE35CFA9D38FC84F
// 0xBFBA3FEB51AEED54
// 0xC01270C23302D9FF
// 0xC04CEC71C25D16DA
// 0xC06C87D34718D55F
// 0xC06B66B95F5C1BF6
// for x in [4.5454,2.8571] = 1/[0.2199,0.35001] ???
internal static array<float64> q1R3 = new float64[]{
    -5.07831226461766561369e-09F,
    -1.02537829820837089745e-01F,
    -4.61011581139473403113e+00F,
    -5.78472216562783643212e+01F,
    -2.28244540737631695038e+02F,
    -2.19210128478909325622e+02F
}.array();

// 0x4047D523CCD367E4
// 0x40850EEBC031EE3E
// 0x40AA684E448E7C9A
// 0x40B5ABBAA61D54A6
// 0x409DBC7A0DD4DF4B
// 0xC060E670290A311F
internal static array<float64> q1S3 = new float64[]{
    4.76651550323729509273e+01F,
    6.73865112676699709482e+02F,
    3.38015286679526343505e+03F,
    5.54772909720722782367e+03F,
    1.90311919338810798763e+03F,
    -1.35201191444307340817e+02F
}.array();

// 0xBE87F12644C626D2
// 0xBFBA3E8E9148B010
// 0xC006048469BB4EDA
// 0xC033A9E2C168907F
// 0xC04529A3DE104AAA
// 0xC0355F3639CF6E52
// for x in [2.8570,2] = 1/[0.3499,0.5]
internal static array<float64> q1R2 = new float64[]{
    -1.78381727510958865572e-07F,
    -1.02517042607985553460e-01F,
    -2.75220568278187460720e+00F,
    -1.96636162643703720221e+01F,
    -4.23253133372830490089e+01F,
    -2.13719211703704061733e+01F
}.array();

// 0x403D888A78AE64FF
// 0x406F9F68DB821CBA
// 0x4087AC05CE49A0F7
// 0x40871B2548D4C029
// 0x40637E5E3C3ED8D4
// 0xC013D686E71BE86B
internal static array<float64> q1S2 = new float64[]{
    2.95333629060523854548e+01F,
    2.52981549982190529136e+02F,
    7.57502834868645436472e+02F,
    7.39393205320467245656e+02F,
    1.55949003336666123687e+02F,
    -4.95949898822628210127e+00F
}.array();

internal static float64 qone(float64 x) {
    ж<array<float64>> p = default!;
    ж<array<float64>> q = default!;
    if (x >= 8){
        p = Ꮡ(q1R8);
        q = Ꮡ(q1S8);
    } else 
    if (x >= 4.5454F){
        p = Ꮡ(q1R5);
        q = Ꮡ(q1S5);
    } else 
    if (x >= 2.8571F){
        p = Ꮡ(q1R3);
        q = Ꮡ(q1S3);
    } else 
    if (x >= 2) {
        p = Ꮡ(q1R2);
        q = Ꮡ(q1S2);
    }
    var z = 1 / (x * x);
    var r = p[0] + z * (p[1] + z * (p[2] + z * (p[3] + z * (p[4] + z * p[5]))));
    var s = 1 + z * (q[0] + z * (q[1] + z * (q[2] + z * (q[3] + z * (q[4] + z * q[5])))));
    return (0.375F + r / s) / x;
}

} // end math_package
