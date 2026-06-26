// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Bessel function of the first and second kinds of order zero.
*/
// The original C code and the long comment below are
// from FreeBSD's /usr/src/lib/msun/src/e_j0.c and
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
// __ieee754_j0(x), __ieee754_y0(x)
// Bessel function of the first and second kinds of order zero.
// Method -- j0(x):
//      1. For tiny x, we use j0(x) = 1 - x**2/4 + x**4/64 - ...
//      2. Reduce x to |x| since j0(x)=j0(-x),  and
//         for x in (0,2)
//              j0(x) = 1-z/4+ z**2*R0/S0,  where z = x*x;
//         (precision:  |j0-1+z/4-z**2R0/S0 |<2**-63.67 )
//         for x in (2,inf)
//              j0(x) = sqrt(2/(pi*x))*(p0(x)*cos(x0)-q0(x)*sin(x0))
//         where x0 = x-pi/4. It is better to compute sin(x0),cos(x0)
//         as follow:
//              cos(x0) = cos(x)cos(pi/4)+sin(x)sin(pi/4)
//                      = 1/sqrt(2) * (cos(x) + sin(x))
//              sin(x0) = sin(x)cos(pi/4)-cos(x)sin(pi/4)
//                      = 1/sqrt(2) * (sin(x) - cos(x))
//         (To avoid cancellation, use
//              sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
//         to compute the worse one.)
//
//      3 Special cases
//              j0(nan)= nan
//              j0(0) = 1
//              j0(inf) = 0
//
// Method -- y0(x):
//      1. For x<2.
//         Since
//              y0(x) = 2/pi*(j0(x)*(ln(x/2)+Euler) + x**2/4 - ...)
//         therefore y0(x)-2/pi*j0(x)*ln(x) is an even function.
//         We use the following function to approximate y0,
//              y0(x) = U(z)/V(z) + (2/pi)*(j0(x)*ln(x)), z= x**2
//         where
//              U(z) = u00 + u01*z + ... + u06*z**6
//              V(z) = 1  + v01*z + ... + v04*z**4
//         with absolute approximation error bounded by 2**-72.
//         Note: For tiny x, U/V = u0 and j0(x)~1, hence
//              y0(tiny) = u0 + (2/pi)*ln(tiny), (choose tiny<2**-27)
//      2. For x>=2.
//              y0(x) = sqrt(2/(pi*x))*(p0(x)*cos(x0)+q0(x)*sin(x0))
//         where x0 = x-pi/4. It is better to compute sin(x0),cos(x0)
//         by the method mentioned above.
//      3. Special cases: y0(0)=-inf, y0(x<0)=NaN, y0(inf)=0.
//

// J0 returns the order-zero Bessel function of the first kind.
//
// Special cases are:
//
//	J0(±Inf) = 0
//	J0(0) = 1
//	J0(NaN) = NaN
public static float64 J0(float64 x) {
    static readonly UntypedFloat Huge = 1e+300;
    static readonly UntypedFloat TwoM27 = /* 1.0 / (1 << 27) */ 7.45058e-09; // 2**-27 0x3e40000000000000
    static readonly UntypedFloat TwoM13 = /* 1.0 / (1 << 13) */ 0.00012207; // 2**-13 0x3f20000000000000
    GoUntyped Two129 = /* 1 << 129 */       // 2**129 0x4800000000000000
            GoUntyped.Parse("680564733841876926926749214863536422912");
    static readonly UntypedFloat R02 = /* 1.56249999999999947958e-02 */ 0.015625;     // 0x3F8FFFFFFFFFFFFD
    static readonly UntypedFloat R03 = /* -1.89979294238854721751e-04 */ -0.000189979; // 0xBF28E6A5B61AC6E9
    static readonly UntypedFloat R04 = /* 1.82954049532700665670e-06 */ 1.82954e-06;  // 0x3EBEB1D10C503919
    static readonly UntypedFloat R05 = /* -4.61832688532103189199e-09 */ -4.61833e-09; // 0xBE33D5E773D63FCE
    static readonly UntypedFloat S01 = /* 1.56191029464890010492e-02 */ 0.0156191;    // 0x3F8FFCE882C8C2A4
    static readonly UntypedFloat S02 = /* 1.16926784663337450260e-04 */ 0.000116927;  // 0x3F1EA6D2DD57DBF4
    static readonly UntypedFloat S03 = /* 5.13546550207318111446e-07 */ 5.13547e-07;  // 0x3EA13B54CE84D5A9
    static readonly UntypedFloat S04 = /* 1.16614003333790000205e-09 */ 1.16614e-09;  // 0x3E1408BCF4745D8F
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x): {
        return x;
    }
    case {} when IsInf(x, 0): {
        return 0;
    }
    case {} when x is 0: {
        return 1;
    }}

    x = Abs(x);
    if (x >= 2) {
        var (sΔ1, c) = Sincos(x);
        var ss = sΔ1 - c;
        var cc = sΔ1 + c;
        // make sure x+x does not overflow
        if (x < MaxFloat64 / 2) {
            var zΔ1 = -Cos(x + x);
            if (sΔ1 * c < 0){
                cc = zΔ1 / ss;
            } else {
                ss = zΔ1 / cc;
            }
        }
        // j0(x) = 1/sqrt(pi) * (P(0,x)*cc - Q(0,x)*ss) / sqrt(x)
        // y0(x) = 1/sqrt(pi) * (P(0,x)*ss + Q(0,x)*cc) / sqrt(x)
        float64 z = default!;
        if (x > Two129){
            // |x| > ~6.8056e+38
            z = (1 / SqrtPi) * cc / Sqrt(x);
        } else {
            var uΔ1 = pzero(x);
            var v = qzero(x);
            z = (1 / SqrtPi) * (uΔ1 * cc - v * ss) / Sqrt(x);
        }
        return z;
    }
    // |x| >= 2.0
    if (x < TwoM13) {
        // |x| < ~1.2207e-4
        if (x < TwoM27) {
            return 1;
        }
        // |x| < ~7.4506e-9
        return 1 - 0.25F * x * x;
    }
    // ~7.4506e-9 < |x| < ~1.2207e-4
    var z = x * x;
    var r = z * (R02 + z * (R03 + z * (R04 + z * R05)));
    var s = 1 + z * (S01 + z * (S02 + z * (S03 + z * S04)));
    if (x < 1) {
        return 1 + z * (-0.25F + (r / s));
    }
    // |x| < 1.00
    var u = 0.5F * x;
    return (1 + u) * (1 - u) + z * (r / s);
}

// 1.0 < |x| < 2.0

// Y0 returns the order-zero Bessel function of the second kind.
//
// Special cases are:
//
//	Y0(+Inf) = 0
//	Y0(0) = -Inf
//	Y0(x < 0) = NaN
//	Y0(NaN) = NaN
public static float64 Y0(float64 x) {
    static readonly UntypedFloat TwoM27 = /* 1.0 / (1 << 27) */ 7.45058e-09; // 2**-27 0x3e40000000000000
    GoUntyped Two129 = /* 1 << 129 */                   // 2**129 0x4800000000000000
            GoUntyped.Parse("680564733841876926926749214863536422912");
    static readonly UntypedFloat U00 = /* -7.38042951086872317523e-02 */ -0.0738043; // 0xBFB2E4D699CBD01F
    static readonly UntypedFloat U01 = /* 1.76666452509181115538e-01 */ 0.176666;     // 0x3FC69D019DE9E3FC
    static readonly UntypedFloat U02 = /* -1.38185671945596898896e-02 */ -0.0138186; // 0xBF8C4CE8B16CFA97
    static readonly UntypedFloat U03 = /* 3.47453432093683650238e-04 */ 0.000347453;  // 0x3F36C54D20B29B6B
    static readonly UntypedFloat U04 = /* -3.81407053724364161125e-06 */ -3.81407e-06; // 0xBECFFEA773D25CAD
    static readonly UntypedFloat U05 = /* 1.95590137035022920206e-08 */ 1.9559e-08;   // 0x3E5500573B4EABD4
    static readonly UntypedFloat U06 = /* -3.98205194132103398453e-11 */ -3.98205e-11; // 0xBDC5E43D693FB3C8
    static readonly UntypedFloat V01 = /* 1.27304834834123699328e-02 */ 0.0127305;    // 0x3F8A127091C9C71A
    static readonly UntypedFloat V02 = /* 7.60068627350353253702e-05 */ 7.60069e-05;  // 0x3F13ECBBF578C6C1
    static readonly UntypedFloat V03 = /* 2.59150851840457805467e-07 */ 2.59151e-07;  // 0x3E91642D7FF202FD
    static readonly UntypedFloat V04 = /* 4.41110311332675467403e-10 */ 4.4111e-10;   // 0x3DFE50183BD6D9EF
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
        // |x| >= 2.0
        // y0(x) = sqrt(2/(pi*x))*(p0(x)*sin(x0)+q0(x)*cos(x0))
        //     where x0 = x-pi/4
        // Better formula:
        //     cos(x0) = cos(x)cos(pi/4)+sin(x)sin(pi/4)
        //             =  1/sqrt(2) * (sin(x) + cos(x))
        //     sin(x0) = sin(x)cos(3pi/4)-cos(x)sin(3pi/4)
        //             =  1/sqrt(2) * (sin(x) - cos(x))
        // To avoid cancellation, use
        //     sin(x) +- cos(x) = -cos(2x)/(sin(x) -+ cos(x))
        // to compute the worse one.
        var (s, c) = Sincos(x);
        var ss = s - c;
        var cc = s + c;
        // j0(x) = 1/sqrt(pi) * (P(0,x)*cc - Q(0,x)*ss) / sqrt(x)
        // y0(x) = 1/sqrt(pi) * (P(0,x)*ss + Q(0,x)*cc) / sqrt(x)
        // make sure x+x does not overflow
        if (x < MaxFloat64 / 2) {
            var zΔ1 = -Cos(x + x);
            if (s * c < 0){
                cc = zΔ1 / ss;
            } else {
                ss = zΔ1 / cc;
            }
        }
        float64 z = default!;
        if (x > Two129){
            // |x| > ~6.8056e+38
            z = (1 / SqrtPi) * ss / Sqrt(x);
        } else {
            var uΔ1 = pzero(x);
            var vΔ1 = qzero(x);
            z = (1 / SqrtPi) * (uΔ1 * ss + vΔ1 * cc) / Sqrt(x);
        }
        return z;
    }
    // |x| >= 2.0
    if (x <= TwoM27) {
        return U00 + (2 / Pi) * Log(x);
    }
    // |x| < ~7.4506e-9
    var z = x * x;
    var u = U00 + z * (U01 + z * (U02 + z * (U03 + z * (U04 + z * (U05 + z * U06)))));
    var v = 1 + z * (V01 + z * (V02 + z * (V03 + z * V04)));
    return u / v + (2 / Pi) * J0(x) * Log(x);
}

// ~7.4506e-9 < |x| < 2.0
// The asymptotic expansions of pzero is
//      1 - 9/128 s**2 + 11025/98304 s**4 - ..., where s = 1/x.
// For x >= 2, We approximate pzero by
// 	pzero(x) = 1 + (R/S)
// where  R = pR0 + pR1*s**2 + pR2*s**4 + ... + pR5*s**10
// 	  S = 1 + pS0*s**2 + ... + pS4*s**10
// and
//      | pzero(x)-1-R/S | <= 2  ** ( -60.26)
// 0x0000000000000000
// 0xBFB1FFFFFFFFFD32
// 0xC02029D0B44FA779
// 0xC07011027B19E863
// 0xC0A36A6ECD4DCAFC
// 0xC0B4850B36CC643D
// for x in [inf, 8]=1/[0,0.125]
internal static array<float64> p0R8 = new float64[]{
    0.00000000000000000000e+00F,
    -7.03124999999900357484e-02F,
    -8.08167041275349795626e+00F,
    -2.57063105679704847262e+02F,
    -2.48521641009428822144e+03F,
    -5.25304380490729545272e+03F
}.array();

// 0x405D223307A96751
// 0x40ADF37D50596938
// 0x40E3D2BB6EB6B05F
// 0x40FC810F8F9FA9BD
// 0x40E741774F2C49DC
internal static array<float64> p0S8 = new float64[]{
    1.16534364619668181717e+02F,
    3.83374475364121826715e+03F,
    4.05978572648472545552e+04F,
    1.16752972564375915681e+05F,
    4.76277284146730962675e+04F
}.array();

// 0xBDA918B147E495CC
// 0xBFB1FFFFE69AFBC6
// 0xC010A370F90C6BBF
// 0xC050EB2F5A7D1783
// 0xC074B3B36742CC63
// 0xC075A6EF28A38BD7
// for x in [8,4.5454]=1/[0.125,0.22001]
internal static array<float64> p0R5 = new float64[]{
    -1.14125464691894502584e-11F,
    -7.03124940873599280078e-02F,
    -4.15961064470587782438e+00F,
    -6.76747652265167261021e+01F,
    -3.31231299649172967747e+02F,
    -3.46433388365604912451e+02F
}.array();

// 0x404E60810C98C5DE
// 0x40906D025C7E2864
// 0x40B75AF88FBE1D60
// 0x40C2CCB8FA76FA38
// 0x40A2CC1DC70BE864
internal static array<float64> p0S5 = new float64[]{
    6.07539382692300335975e+01F,
    1.05125230595704579173e+03F,
    5.97897094333855784498e+03F,
    9.62544514357774460223e+03F,
    2.40605815922939109441e+03F
}.array();

// 0xBE25E1036FE1AA86
// 0xBFB1FFF6F7C0E24B
// 0xC00345B2AEA48074
// 0xC035F74A4CB94E14
// 0xC04D0A22420A1A45
// 0xC03F72ACA892D80F
// for x in [4.547,2.8571]=1/[0.2199,0.35001]
internal static array<float64> p0R3 = new float64[]{
    -2.54704601771951915620e-09F,
    -7.03119616381481654654e-02F,
    -2.40903221549529611423e+00F,
    -2.19659774734883086467e+01F,
    -5.80791704701737572236e+01F,
    -3.14479470594888503854e+01F
}.array();

// 0x4041ED9284077DD3
// 0x40769839464A7C0E
// 0x4092A66E6D1061D6
// 0x40919FFCB8C39B7E
// 0x4065B296FC379081
internal static array<float64> p0S3 = new float64[]{
    3.58560338055209726349e+01F,
    3.61513983050303863820e+02F,
    1.19360783792111533330e+03F,
    1.12799679856907414432e+03F,
    1.73580930813335754692e+02F
}.array();

// 0xBE77D316E927026D
// 0xBFB1FF62495E1E42
// 0xBFF736398A24A843
// 0xC01E8AF3EDAFA7F3
// 0xC02662E6C5246303
// 0xC009DE81AF8FE70F
// for x in [2.8570,2]=1/[0.3499,0.5]
internal static array<float64> p0R2 = new float64[]{
    -8.87534333032526411254e-08F,
    -7.03030995483624743247e-02F,
    -1.45073846780952986357e+00F,
    -7.63569613823527770791e+00F,
    -1.11931668860356747786e+01F,
    -3.23364579351335335033e+00F
}.array();

// 0x40363865908B5959
// 0x4061069E0EE8878F
// 0x4070E78642EA079B
// 0x40633C033AB6FAFF
// 0x402D50B344391809
internal static array<float64> p0S2 = new float64[]{
    2.22202997532088808441e+01F,
    1.36206794218215208048e+02F,
    2.70470278658083486789e+02F,
    1.53875394208320329881e+02F,
    1.46576176948256193810e+01F
}.array();

internal static float64 pzero(float64 x) {
    ж<array<float64>> p = default!;
    ж<array<float64>> q = default!;
    if (x >= 8){
        p = Ꮡ(p0R8);
        q = Ꮡ(p0S8);
    } else 
    if (x >= 4.5454F){
        p = Ꮡ(p0R5);
        q = Ꮡ(p0S5);
    } else 
    if (x >= 2.8571F){
        p = Ꮡ(p0R3);
        q = Ꮡ(p0S3);
    } else 
    if (x >= 2) {
        p = Ꮡ(p0R2);
        q = Ꮡ(p0S2);
    }
    var z = 1 / (x * x);
    var r = p.val[0] + z * (p.val[1] + z * (p.val[2] + z * (p.val[3] + z * (p.val[4] + z * p.val[5]))));
    var s = 1 + z * (q.val[0] + z * (q.val[1] + z * (q.val[2] + z * (q.val[3] + z * q.val[4]))));
    return 1 + r / s;
}

// For x >= 8, the asymptotic expansions of qzero is
//      -1/8 s + 75/1024 s**3 - ..., where s = 1/x.
// We approximate pzero by
//      qzero(x) = s*(-1.25 + (R/S))
// where R = qR0 + qR1*s**2 + qR2*s**4 + ... + qR5*s**10
//       S = 1 + qS0*s**2 + ... + qS5*s**12
// and
//      | qzero(x)/s +1.25-R/S | <= 2**(-61.22)
// 0x0000000000000000
// 0x3FB2BFFFFFFFFE2C
// 0x402789525BB334D6
// 0x40816D6315301825
// 0x40C14D993E18F46D
// 0x40E212D40E901566
// for x in [inf, 8]=1/[0,0.125]
internal static array<float64> q0R8 = new float64[]{
    0.00000000000000000000e+00F,
    7.32421874999935051953e-02F,
    1.17682064682252693899e+01F,
    5.57673380256401856059e+02F,
    8.85919720756468632317e+03F,
    3.70146267776887834771e+04F
}.array();

// 0x406478D5365B39BC
// 0x40BFA2584E6B0563
// 0x4101665254D38C3F
// 0x412883DA83A52B43
// 0x4129A66B28DE0B3D
// 0xC114FD6D2C9530C5
internal static array<float64> q0S8 = new float64[]{
    1.63776026895689824414e+02F,
    8.09834494656449805916e+03F,
    1.42538291419120476348e+05F,
    8.03309257119514397345e+05F,
    8.40501579819060512818e+05F,
    -3.43899293537866615225e+05F
}.array();

// 0x3DB43D8F29CC8CD9
// 0x3FB2BFFFD172B04C
// 0x401757B0B9953DD3
// 0x4060E3920A8788E9
// 0x40900CF99DC8C481
// 0x409F17E953C6E3A6
// for x in [8,4.5454]=1/[0.125,0.22001]
internal static array<float64> q0R5 = new float64[]{
    1.84085963594515531381e-11F,
    7.32421766612684765896e-02F,
    5.83563508962056953777e+00F,
    1.35111577286449829671e+02F,
    1.02724376596164097464e+03F,
    1.98997785864605384631e+03F
}.array();

// 0x4054B1B3FB5E1543
// 0x40A03BA0DA21C0CE
// 0x40D267D27B591E6D
// 0x40EBB5E397E02372
// 0x40E191181F7A54A0
// 0xC0B4EA57BEDBC609
internal static array<float64> q0S5 = new float64[]{
    8.27766102236537761883e+01F,
    2.07781416421392987104e+03F,
    1.88472887785718085070e+04F,
    5.67511122894947329769e+04F,
    3.59767538425114471465e+04F,
    -5.35434275601944773371e+03F
}.array();

// 0x3E32CD036ADECB82
// 0x3FB2BFEE0E8D0842
// 0x400AC0FC61149CF5
// 0x40454F98962DAEDD
// 0x406559DBE25EFD1F
// 0x4064D77C81FA21E0
// for x in [4.547,2.8571]=1/[0.2199,0.35001]
internal static array<float64> q0R3 = new float64[]{
    4.37741014089738620906e-09F,
    7.32411180042911447163e-02F,
    3.34423137516170720929e+00F,
    4.26218440745412650017e+01F,
    1.70808091340565596283e+02F,
    1.66733948696651168575e+02F
}.array();

// 0x40486122BFE343A6
// 0x40862D8386544EB3
// 0x40ACF04BE44DFC63
// 0x40B93C6CD7C76A28
// 0x40A3A8AAD94FB1C0
// 0xC062A7EB201CF40F
internal static array<float64> q0S3 = new float64[]{
    4.87588729724587182091e+01F,
    7.09689221056606015736e+02F,
    3.70414822620111362994e+03F,
    6.46042516752568917582e+03F,
    2.51633368920368957333e+03F,
    -1.49247451836156386662e+02F
}.array();

// 0x3E84313B54F76BDB
// 0x3FB2BEC53E883E34
// 0x3FFFF897E727779C
// 0x402CFDBFAAF96FE5
// 0x403FAA8E29FBDC4A
// 0x403040B171814BB4
// for x in [2.8570,2]=1/[0.3499,0.5]
internal static array<float64> q0R2 = new float64[]{
    1.50444444886983272379e-07F,
    7.32234265963079278272e-02F,
    1.99819174093815998816e+00F,
    1.44956029347885735348e+01F,
    3.16662317504781540833e+01F,
    1.62527075710929267416e+01F
}.array();

// 0x403E5D96F7C07AED
// 0x4070D591E4D14B40
// 0x408A664522B3BF22
// 0x408B977C9C5CC214
// 0x406A95530E001365
// 0xC0153E6AF8B32931
internal static array<float64> q0S2 = new float64[]{
    3.03655848355219184498e+01F,
    2.69348118608049844624e+02F,
    8.44783757595320139444e+02F,
    8.82935845112488550512e+02F,
    2.12666388511798828631e+02F,
    -5.31095493882666946917e+00F
}.array();

internal static float64 qzero(float64 x) {
    ж<array<float64>> p = default!;
    ж<array<float64>> q = default!;
    if (x >= 8){
        p = Ꮡ(q0R8);
        q = Ꮡ(q0S8);
    } else 
    if (x >= 4.5454F){
        p = Ꮡ(q0R5);
        q = Ꮡ(q0S5);
    } else 
    if (x >= 2.8571F){
        p = Ꮡ(q0R3);
        q = Ꮡ(q0S3);
    } else 
    if (x >= 2) {
        p = Ꮡ(q0R2);
        q = Ꮡ(q0S2);
    }
    var z = 1 / (x * x);
    var r = p.val[0] + z * (p.val[1] + z * (p.val[2] + z * (p.val[3] + z * (p.val[4] + z * p.val[5]))));
    var s = 1 + z * (q.val[0] + z * (q.val[1] + z * (q.val[2] + z * (q.val[3] + z * (q.val[4] + z * q.val[5])))));
    return (-0.125F + r / s) / x;
}

} // end math_package
