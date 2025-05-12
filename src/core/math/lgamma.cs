// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point logarithm of the Gamma function.
*/
// The original C code and the long comment below are
// from FreeBSD's /usr/src/lib/msun/src/e_lgamma_r.c and
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
// __ieee754_lgamma_r(x, signgamp)
// Reentrant version of the logarithm of the Gamma function
// with user provided pointer for the sign of Gamma(x).
//
// Method:
//   1. Argument Reduction for 0 < x <= 8
//      Since gamma(1+s)=s*gamma(s), for x in [0,8], we may
//      reduce x to a number in [1.5,2.5] by
//              lgamma(1+s) = log(s) + lgamma(s)
//      for example,
//              lgamma(7.3) = log(6.3) + lgamma(6.3)
//                          = log(6.3*5.3) + lgamma(5.3)
//                          = log(6.3*5.3*4.3*3.3*2.3) + lgamma(2.3)
//   2. Polynomial approximation of lgamma around its
//      minimum (ymin=1.461632144968362245) to maintain monotonicity.
//      On [ymin-0.23, ymin+0.27] (i.e., [1.23164,1.73163]), use
//              Let z = x-ymin;
//              lgamma(x) = -1.214862905358496078218 + z**2*poly(z)
//              poly(z) is a 14 degree polynomial.
//   2. Rational approximation in the primary interval [2,3]
//      We use the following approximation:
//              s = x-2.0;
//              lgamma(x) = 0.5*s + s*P(s)/Q(s)
//      with accuracy
//              |P/Q - (lgamma(x)-0.5s)| < 2**-61.71
//      Our algorithms are based on the following observation
//
//                             zeta(2)-1    2    zeta(3)-1    3
// lgamma(2+s) = s*(1-Euler) + --------- * s  -  --------- * s  + ...
//                                 2                 3
//
//      where Euler = 0.5772156649... is the Euler constant, which
//      is very close to 0.5.
//
//   3. For x>=8, we have
//      lgamma(x)~(x-0.5)log(x)-x+0.5*log(2pi)+1/(12x)-1/(360x**3)+....
//      (better formula:
//         lgamma(x)~(x-0.5)*(log(x)-1)-.5*(log(2pi)-1) + ...)
//      Let z = 1/x, then we approximation
//              f(z) = lgamma(x) - (x-0.5)(log(x)-1)
//      by
//                                  3       5             11
//              w = w0 + w1*z + w2*z  + w3*z  + ... + w6*z
//      where
//              |w - f(z)| < 2**-58.74
//
//   4. For negative x, since (G is gamma function)
//              -x*G(-x)*G(x) = pi/sin(pi*x),
//      we have
//              G(x) = pi/(sin(pi*x)*(-x)*G(-x))
//      since G(-x) is positive, sign(G(x)) = sign(sin(pi*x)) for x<0
//      Hence, for x<0, signgam = sign(sin(pi*x)) and
//              lgamma(x) = log(|Gamma(x)|)
//                        = log(pi/(|x*sin(pi*x)|)) - lgamma(-x);
//      Note: one should avoid computing pi*(-x) directly in the
//            computation of sin(pi*(-x)).
//
//   5. Special Cases
//              lgamma(2+s) ~ s*(1-Euler) for tiny s
//              lgamma(1)=lgamma(2)=0
//              lgamma(x) ~ -log(x) for tiny x
//              lgamma(0) = lgamma(inf) = inf
//              lgamma(-integer) = +-inf
//
//
// 0x3FB3C467E37DB0C8
// 0x3FD4A34CC4A60FAD
// 0x3FB13E001A5562A7
// 0x3F951322AC92547B
// 0x3F7E404FB68FEFE8
// 0x3F67ADD8CCB7926B
// 0x3F538A94116F3F5D
// 0x3F40B6C689B99C00
// 0x3F2CF2ECED10E54D
// 0x3F1C5088987DFB07
// 0x3EFA7074428CFA52
// 0x3F07858E90A45837
internal static array<float64> _lgamA = new float64[]{
    7.72156649015328655494e-02F,
    3.22467033424113591611e-01F,
    6.73523010531292681824e-02F,
    2.05808084325167332806e-02F,
    7.38555086081402883957e-03F,
    2.89051383673415629091e-03F,
    1.19270763183362067845e-03F,
    5.10069792153511336608e-04F,
    2.20862790713908385557e-04F,
    1.08011567247583939954e-04F,
    2.52144565451257326939e-05F,
    4.48640949618915160150e-05F
}.array();

// placeholder
// 0x3FF645A762C4AB74
// 0x3FE71A1893D3DCDC
// 0x3FC601EDCCFBDF27
// 0x3F9317EA742ED475
// 0x3F497DDACA41A95B
// 0x3EDEBAF7A5B38140
internal static array<float64> _lgamR = new float64[]{
    1.0F,
    1.39200533467621045958e+00F,
    7.21935547567138069525e-01F,
    1.71933865632803078993e-01F,
    1.86459191715652901344e-02F,
    7.77942496381893596434e-04F,
    7.32668430744625636189e-06F
}.array();

// 0xBFB3C467E37DB0C8
// 0x3FCB848B36E20878
// 0x3FD4D98F4F139F59
// 0x3FC2BB9CBEE5F2F7
// 0x3F9B481C7E939961
// 0x3F5E26B67368F239
// 0x3F00BFECDD17E945
internal static array<float64> _lgamS = new float64[]{
    -7.72156649015328655494e-02F,
    2.14982415960608852501e-01F,
    3.25778796408930981787e-01F,
    1.46350472652464452805e-01F,
    2.66422703033638609560e-02F,
    1.84028451407337715652e-03F,
    3.19475326584100867617e-05F
}.array();

// 0x3FDEF72BC8EE38A2
// 0xBFC2E4278DC6C509
// 0x3FB08B4294D5419B
// 0xBFA0C9A8DF35B713
// 0x3F9266E7970AF9EC
// 0xBF851F9FBA91EC6A
// 0x3F78FCE0E370E344
// 0xBF6E2EFFB3E914D7
// 0x3F6282D32E15C915
// 0xBF56FE8EBF2D1AF1
// 0x3F4CDF0CEF61A8E9
// 0xBF41A6109C73E0EC
// 0x3F34AF6D6C0EBBF7
// 0xBF347F24ECC38C38
// 0x3F35FD3EE8C2D3F4
internal static array<float64> _lgamT = new float64[]{
    4.83836122723810047042e-01F,
    -1.47587722994593911752e-01F,
    6.46249402391333854778e-02F,
    -3.27885410759859649565e-02F,
    1.79706750811820387126e-02F,
    -1.03142241298341437450e-02F,
    6.10053870246291332635e-03F,
    -3.68452016781138256760e-03F,
    2.25964780900612472250e-03F,
    -1.40346469989232843813e-03F,
    8.81081882437654011382e-04F,
    -5.38595305356740546715e-04F,
    3.15632070903625950361e-04F,
    -3.12754168375120860518e-04F,
    3.35529192635519073543e-04F
}.array();

// 0xBFB3C467E37DB0C8
// 0x3FE4401E8B005DFF
// 0x3FF7475CD119BD6F
// 0x3FEF497644EA8450
// 0x3FCD4EAEF6010924
// 0x3F8B678BBF2BAB09
internal static array<float64> _lgamU = new float64[]{
    -7.72156649015328655494e-02F,
    6.32827064025093366517e-01F,
    1.45492250137234768737e+00F,
    9.77717527963372745603e-01F,
    2.28963728064692451092e-01F,
    1.33810918536787660377e-02F
}.array();

// 0x4003A5D7C2BD619C
// 0x40010725A42B18F5
// 0x3FE89DFBE45050AF
// 0x3FBAAE55D6537C88
// 0x3F6A5ABB57D0CF61
internal static array<float64> _lgamV = new float64[]{
    1.0F,
    2.45597793713041134822e+00F,
    2.12848976379893395361e+00F,
    7.69285150456672783825e-01F,
    1.04222645593369134254e-01F,
    3.21709242282423911810e-03F
}.array();

// 0x3FDACFE390C97D69
// 0x3FB555555555553B
// 0xBF66C16C16B02E5C
// 0x3F4A019F98CF38B6
// 0xBF4380CB8C0FE741
// 0x3F4B67BA4CDAD5D1
// 0xBF5AB89D0B9E43E4
internal static array<float64> _lgamW = new float64[]{
    4.18938533204672725052e-01F,
    8.33333333333329678849e-02F,
    -2.77777777728775536470e-03F,
    7.93650558643019558500e-04F,
    -5.95187557450339963135e-04F,
    8.36339918996282139126e-04F,
    -1.63092934096575273989e-03F
}.array();

// Lgamma returns the natural logarithm and sign (-1 or +1) of [Gamma](x).
//
// Special cases are:
//
//	Lgamma(+Inf) = +Inf
//	Lgamma(0) = +Inf
//	Lgamma(-integer) = +Inf
//	Lgamma(-Inf) = -Inf
//	Lgamma(NaN) = NaN
public static (float64 lgamma, nint sign) Lgamma(float64 x) {
    float64 lgamma = default!;
    nint sign = default!;

    static readonly UntypedFloat Ymin = /* 1.461632144968362245 */ 1.46163;
    static readonly UntypedInt Two52 = /* 1 << 52 */ 4503599627370496; // 0x4330000000000000 ~4.5036e+15
    static readonly UntypedInt Two53 = /* 1 << 53 */ 9007199254740992; // 0x4340000000000000 ~9.0072e+15
    static readonly UntypedInt Two58 = /* 1 << 58 */ 288230376151711744; // 0x4390000000000000 ~2.8823e+17
    static readonly UntypedFloat Tiny = /* 1.0 / (1 << 70) */ 8.47033e-22; // 0x3b90000000000000 ~8.47033e-22
    static readonly UntypedFloat Tc = /* 1.46163214496836224576e+00 */ 1.46163;      // 0x3FF762D86356BE3F
    static readonly UntypedFloat Tf = /* -1.21486290535849611461e-01 */ -0.121486; // 0xBFBF19B9BCC38A42
    static readonly UntypedFloat Tt = /* -3.63867699703950536541e-18 */ -3.63868e-18; // 0xBC50C7CAA48A971F
    // special cases
    sign = 1;
    switch (ᐧ) {
    case {} when IsNaN(x): {
        lgamma = x;
        return (lgamma, sign);
    }
    case {} when IsInf(x, 0): {
        lgamma = x;
        return (lgamma, sign);
    }
    case {} when x is 0: {
        lgamma = Inf(1);
        return (lgamma, sign);
    }}

    var neg = false;
    if (x < 0) {
        x = -x;
        neg = true;
    }
    if (x < Tiny) {
        // if |x| < 2**-70, return -log(|x|)
        if (neg) {
            sign = -1;
        }
        lgamma = -Log(x);
        return (lgamma, sign);
    }
    float64 nadj = default!;
    if (neg) {
        if (x >= Two52) {
            // |x| >= 2**52, must be -integer
            lgamma = Inf(1);
            return (lgamma, sign);
        }
        var t = sinPi(x);
        if (t == 0) {
            lgamma = Inf(1);
            // -integer
            return (lgamma, sign);
        }
        nadj = Log(Pi / Abs(t * x));
        if (t < 0) {
            sign = -1;
        }
    }
    switch (ᐧ) {
    case {} when x == 1 || x == 2: {
        lgamma = 0;
        return (lgamma, sign);
    }
    case {} when x is < 2: {
// purge off 1 and 2
// use lgamma(x) = lgamma(x+1) - log(x)
        float64 y = default!;
        nint i = default!;
        if (x <= 0.9F){
            lgamma = -Log(x);
            switch (ᐧ) {
            case {} when x >= (Ymin - 1 + 0.27F): {
                y = 1 - x;
                i = 0;
                break;
            }
            case {} when x >= (Ymin - 1 - 0.27F): {
                y = x - (Tc - 1);
                i = 1;
                break;
            }
            default: {
                y = x;
                i = 2;
                break;
            }}

        } else {
            // 0.7316 <= x <=  0.9
            // 0.2316 <= x < 0.7316
            // 0 < x < 0.2316
            lgamma = 0;
            switch (ᐧ) {
            case {} when x >= (Ymin + 0.27F): {
                y = 2 - x;
                i = 0;
                break;
            }
            case {} when x >= (Ymin - 0.27F): {
                y = x - Tc;
                i = 1;
                break;
            }
            default: {
                y = x - 1;
                i = 2;
                break;
            }}

        }
        switch (i) {
        case 0: {
            var z = y * y;
            var p1 = _lgamA[0] + z * (_lgamA[2] + z * (_lgamA[4] + z * (_lgamA[6] + z * (_lgamA[8] + z * _lgamA[10]))));
            var p2 = z * (_lgamA[1] + z * (+_lgamA[3] + z * (_lgamA[5] + z * (_lgamA[7] + z * (_lgamA[9] + z * _lgamA[11])))));
            var p = y * p1 + p2;
            lgamma += (p - 0.5F * y);
            break;
        }
        case 1: {
            var z = y * y;
            var w = z * y;
            var p1 = _lgamT[0] + w * (_lgamT[3] + w * (_lgamT[6] + w * (_lgamT[9] + w * _lgamT[12])));
            var p2 = _lgamT[1] + w * (_lgamT[4] + w * (_lgamT[7] + w * (_lgamT[10] + w * _lgamT[13])));
            var p3 = _lgamT[2] + w * (_lgamT[5] + w * (_lgamT[8] + w * (_lgamT[11] + w * _lgamT[14])));
            var p = z * p1 - (Tt - w * (p2 + y * p3));
            lgamma += (Tf + p);
            break;
        }
        case 2: {
            var p1 = y * (_lgamU[0] + y * (_lgamU[1] + y * (_lgamU[2] + y * (_lgamU[3] + y * (_lgamU[4] + y * _lgamU[5])))));
            var p2 = 1 + y * (_lgamV[1] + y * (_lgamV[2] + y * (_lgamV[3] + y * (_lgamV[4] + y * _lgamV[5]))));
            lgamma += (-0.5F * y + p1 / p2);
            break;
        }}

        break;
    }
    case {} when x is < 8: {
        nint i = ((nint)x);
        var y = x - ((float64)i);
        var p = y * (_lgamS[0] + y * (_lgamS[1] + y * (_lgamS[2] + y * (_lgamS[3] + y * (_lgamS[4] + y * (_lgamS[5] + y * _lgamS[6]))))));
        var q = 1 + y * (_lgamR[1] + y * (_lgamR[2] + y * (_lgamR[3] + y * (_lgamR[4] + y * (_lgamR[5] + y * _lgamR[6])))));
        lgamma = 0.5F * y + p / q;
        var z = 1.0F;
        var exprᴛ1 = i;
        var matchᴛ1 = false;
        if (exprᴛ1 is 7) { matchᴛ1 = true;
            z *= (y + 6);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 6)) { matchᴛ1 = true;
            z *= (y + 5);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 5)) { matchᴛ1 = true;
            z *= (y + 4);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 4)) {
            z *= (y + 3);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 3)) { matchᴛ1 = true;
            z *= (y + 2);
            lgamma += Log(z);
        }

        break;
    }
    case {} when x < Two58: {
        var t = Log(x);
        var z = 1 / x;
        var y = z * z;
        var w = _lgamW[0] + z * (_lgamW[1] + y * (_lgamW[2] + y * (_lgamW[3] + y * (_lgamW[4] + y * (_lgamW[5] + y * _lgamW[6])))));
        lgamma = (x - 0.5F) * (t - 1) + w;
        break;
    }
    default: {
        lgamma = x * (Log(x) - 1);
        break;
    }}

    // 1.7316 <= x < 2
    // 1.2316 <= x < 1.7316
    // 0.9 < x < 1.2316
    // parallel comp
    // 2 <= x < 8
    // Lgamma(1+s) = Log(s) + Lgamma(s)
    // 8 <= x < 2**58
    // 2**58 <= x <= Inf
    if (neg) {
        lgamma = nadj - lgamma;
    }
    return (lgamma, sign);
}

// sinPi(x) is a helper function for negative x
internal static float64 sinPi(float64 x) {
    static readonly UntypedInt Two52 = /* 1 << 52 */ 4503599627370496; // 0x4330000000000000 ~4.5036e+15
    static readonly UntypedInt Two53 = /* 1 << 53 */ 9007199254740992; // 0x4340000000000000 ~9.0072e+15
    if (x < 0.25F) {
        return -Sin(Pi * x);
    }
    // argument reduction
    var z = Floor(x);
    nint n = default!;
    if (z != x){
        // inexact
        x = Mod(x, 2);
        n = ((nint)(x * 4));
    } else {
        if (x >= Two53){
            // x must be even
            x = 0;
            n = 0;
        } else {
            if (x < Two52) {
                z = x + Two52;
            }
            // exact
            n = ((nint)((uint64)(1 & Float64bits(z))));
            x = ((float64)n);
            n <<= (UntypedInt)(2);
        }
    }
    switch (n) {
    case 0: {
        x = Sin(Pi * x);
        break;
    }
    case 1 or 2: {
        x = Cos(Pi * (0.5F - x));
        break;
    }
    case 3 or 4: {
        x = Sin(Pi * (1 - x));
        break;
    }
    case 5 or 6: {
        x = -Cos(Pi * (x - 1.5F));
        break;
    }
    default: {
        x = Sin(Pi * (x - 2));
        break;
    }}

    return -x;
}

} // end math_package
