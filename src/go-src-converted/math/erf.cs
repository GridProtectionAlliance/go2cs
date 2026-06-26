// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point error function and complementary error function.
*/
// The original C code and the long comment below are
// from FreeBSD's /usr/src/lib/msun/src/s_erf.c and
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
//
// double erf(double x)
// double erfc(double x)
//                           x
//                    2      |\
//     erf(x)  =  ---------  | exp(-t*t)dt
//                 sqrt(pi) \|
//                           0
//
//     erfc(x) =  1-erf(x)
//  Note that
//              erf(-x) = -erf(x)
//              erfc(-x) = 2 - erfc(x)
//
// Method:
//      1. For |x| in [0, 0.84375]
//          erf(x)  = x + x*R(x**2)
//          erfc(x) = 1 - erf(x)           if x in [-.84375,0.25]
//                  = 0.5 + ((0.5-x)-x*R)  if x in [0.25,0.84375]
//         where R = P/Q where P is an odd poly of degree 8 and
//         Q is an odd poly of degree 10.
//                                               -57.90
//                      | R - (erf(x)-x)/x | <= 2
//
//
//         Remark. The formula is derived by noting
//          erf(x) = (2/sqrt(pi))*(x - x**3/3 + x**5/10 - x**7/42 + ....)
//         and that
//          2/sqrt(pi) = 1.128379167095512573896158903121545171688
//         is close to one. The interval is chosen because the fix
//         point of erf(x) is near 0.6174 (i.e., erf(x)=x when x is
//         near 0.6174), and by some experiment, 0.84375 is chosen to
//         guarantee the error is less than one ulp for erf.
//
//      2. For |x| in [0.84375,1.25], let s = |x| - 1, and
//         c = 0.84506291151 rounded to single (24 bits)
//              erf(x)  = sign(x) * (c  + P1(s)/Q1(s))
//              erfc(x) = (1-c)  - P1(s)/Q1(s) if x > 0
//                        1+(c+P1(s)/Q1(s))    if x < 0
//              |P1/Q1 - (erf(|x|)-c)| <= 2**-59.06
//         Remark: here we use the taylor series expansion at x=1.
//              erf(1+s) = erf(1) + s*Poly(s)
//                       = 0.845.. + P1(s)/Q1(s)
//         That is, we use rational approximation to approximate
//                      erf(1+s) - (c = (single)0.84506291151)
//         Note that |P1/Q1|< 0.078 for x in [0.84375,1.25]
//         where
//              P1(s) = degree 6 poly in s
//              Q1(s) = degree 6 poly in s
//
//      3. For x in [1.25,1/0.35(~2.857143)],
//              erfc(x) = (1/x)*exp(-x*x-0.5625+R1/S1)
//              erf(x)  = 1 - erfc(x)
//         where
//              R1(z) = degree 7 poly in z, (z=1/x**2)
//              S1(z) = degree 8 poly in z
//
//      4. For x in [1/0.35,28]
//              erfc(x) = (1/x)*exp(-x*x-0.5625+R2/S2) if x > 0
//                      = 2.0 - (1/x)*exp(-x*x-0.5625+R2/S2) if -6<x<0
//                      = 2.0 - tiny            (if x <= -6)
//              erf(x)  = sign(x)*(1.0 - erfc(x)) if x < 6, else
//              erf(x)  = sign(x)*(1.0 - tiny)
//         where
//              R2(z) = degree 6 poly in z, (z=1/x**2)
//              S2(z) = degree 7 poly in z
//
//      Note1:
//         To compute exp(-x*x-0.5625+R/S), let s be a single
//         precision number and s := x; then
//              -x*x = -s*s + (s-x)*(s+x)
//              exp(-x*x-0.5626+R/S) =
//                      exp(-s*s-0.5625)*exp((s-x)*(s+x)+R/S);
//      Note2:
//         Here 4 and 5 make use of the asymptotic series
//                        exp(-x*x)
//              erfc(x) ~ ---------- * ( 1 + Poly(1/x**2) )
//                        x*sqrt(pi)
//         We use rational approximation to approximate
//              g(s)=f(1/x**2) = log(erfc(x)*x) - x*x + 0.5625
//         Here is the error bound for R1/S1 and R2/S2
//              |R1/S1 - f(x)|  < 2**(-62.57)
//              |R2/S2 - f(x)|  < 2**(-61.52)
//
//      5. For inf > x >= 28
//              erf(x)  = sign(x) *(1 - tiny)  (raise inexact)
//              erfc(x) = tiny*tiny (raise underflow) if x > 0
//                      = 2 - tiny if x<0
//
//      7. Special case:
//              erf(0)  = 0, erf(inf)  = 1, erf(-inf) = -1,
//              erfc(0) = 1, erfc(inf) = 0, erfc(-inf) = 2,
//              erfc/erf(NaN) is NaN
internal static readonly UntypedFloat erx = /* 8.45062911510467529297e-01 */ 0.845063;  // 0x3FEB0AC160000000
internal static readonly UntypedFloat efx = /* 1.28379167095512586316e-01 */ 0.128379;   // 0x3FC06EBA8214DB69
internal static readonly UntypedFloat efx8 = /* 1.02703333676410069053e+00 */ 1.02703;    // 0x3FF06EBA8214DB69
internal static readonly UntypedFloat pp0 = /* 1.28379167095512558561e-01 */ 0.128379;   // 0x3FC06EBA8214DB68
internal static readonly UntypedFloat pp1 = /* -3.25042107247001499370e-01 */ -0.325042; // 0xBFD4CD7D691CB913
internal static readonly UntypedFloat pp2 = /* -2.84817495755985104766e-02 */ -0.0284817; // 0xBF9D2A51DBD7194F
internal static readonly UntypedFloat pp3 = /* -5.77027029648944159157e-03 */ -0.00577027; // 0xBF77A291236668E4
internal static readonly UntypedFloat pp4 = /* -2.37630166566501626084e-05 */ -2.3763e-05; // 0xBEF8EAD6120016AC
internal static readonly UntypedFloat qq1 = /* 3.97917223959155352819e-01 */ 0.397917;   // 0x3FD97779CDDADC09
internal static readonly UntypedFloat qq2 = /* 6.50222499887672944485e-02 */ 0.0650222;  // 0x3FB0A54C5536CEBA
internal static readonly UntypedFloat qq3 = /* 5.08130628187576562776e-03 */ 0.00508131; // 0x3F74D022C4D36B0F
internal static readonly UntypedFloat qq4 = /* 1.32494738004321644526e-04 */ 0.000132495; // 0x3F215DC9221C1A10
internal static readonly UntypedFloat qq5 = /* -3.96022827877536812320e-06 */ -3.96023e-06; // 0xBED09C4342A26120
internal static readonly UntypedFloat pa0 = /* -2.36211856075265944077e-03 */ -0.00236212; // 0xBF6359B8BEF77538
internal static readonly UntypedFloat pa1 = /* 4.14856118683748331666e-01 */ 0.414856;   // 0x3FDA8D00AD92B34D
internal static readonly UntypedFloat pa2 = /* -3.72207876035701323847e-01 */ -0.372208; // 0xBFD7D240FBB8C3F1
internal static readonly UntypedFloat pa3 = /* 3.18346619901161753674e-01 */ 0.318347;   // 0x3FD45FCA805120E4
internal static readonly UntypedFloat pa4 = /* -1.10894694282396677476e-01 */ -0.110895; // 0xBFBC63983D3E28EC
internal static readonly UntypedFloat pa5 = /* 3.54783043256182359371e-02 */ 0.0354783;  // 0x3FA22A36599795EB
internal static readonly UntypedFloat pa6 = /* -2.16637559486879084300e-03 */ -0.00216638; // 0xBF61BF380A96073F
internal static readonly UntypedFloat qa1 = /* 1.06420880400844228286e-01 */ 0.106421;   // 0x3FBB3E6618EEE323
internal static readonly UntypedFloat qa2 = /* 5.40397917702171048937e-01 */ 0.540398;   // 0x3FE14AF092EB6F33
internal static readonly UntypedFloat qa3 = /* 7.18286544141962662868e-02 */ 0.0718287;  // 0x3FB2635CD99FE9A7
internal static readonly UntypedFloat qa4 = /* 1.26171219808761642112e-01 */ 0.126171;   // 0x3FC02660E763351F
internal static readonly UntypedFloat qa5 = /* 1.36370839120290507362e-02 */ 0.0136371;  // 0x3F8BEDC26B51DD1C
internal static readonly UntypedFloat qa6 = /* 1.19844998467991074170e-02 */ 0.0119845;  // 0x3F888B545735151D
internal static readonly UntypedFloat ra0 = /* -9.86494403484714822705e-03 */ -0.00986494; // 0xBF843412600D6435
internal static readonly UntypedFloat ra1 = /* -6.93858572707181764372e-01 */ -0.693859; // 0xBFE63416E4BA7360
internal static readonly UntypedFloat ra2 = /* -1.05586262253232909814e+01 */ -10.5586; // 0xC0251E0441B0E726
internal static readonly UntypedFloat ra3 = /* -6.23753324503260060396e+01 */ -62.3753; // 0xC04F300AE4CBA38D
internal static readonly UntypedFloat ra4 = /* -1.62396669462573470355e+02 */ -162.397; // 0xC0644CB184282266
internal static readonly UntypedFloat ra5 = /* -1.84605092906711035994e+02 */ -184.605; // 0xC067135CEBCCABB2
internal static readonly UntypedFloat ra6 = /* -8.12874355063065934246e+01 */ -81.2874; // 0xC054526557E4D2F2
internal static readonly UntypedFloat ra7 = /* -9.81432934416914548592e+00 */ -9.81433; // 0xC023A0EFC69AC25C
internal static readonly UntypedFloat sa1 = /* 1.96512716674392571292e+01 */ 19.6513;    // 0x4033A6B9BD707687
internal static readonly UntypedFloat sa2 = /* 1.37657754143519042600e+02 */ 137.658;    // 0x4061350C526AE721
internal static readonly UntypedFloat sa3 = /* 4.34565877475229228821e+02 */ 434.566;    // 0x407B290DD58A1A71
internal static readonly UntypedFloat sa4 = /* 6.45387271733267880336e+02 */ 645.387;    // 0x40842B1921EC2868
internal static readonly UntypedFloat sa5 = /* 4.29008140027567833386e+02 */ 429.008;    // 0x407AD02157700314
internal static readonly UntypedFloat sa6 = /* 1.08635005541779435134e+02 */ 108.635;    // 0x405B28A3EE48AE2C
internal static readonly UntypedFloat sa7 = /* 6.57024977031928170135e+00 */ 6.57025;    // 0x401A47EF8E484A93
internal static readonly UntypedFloat sa8 = /* -6.04244152148580987438e-02 */ -0.0604244; // 0xBFAEEFF2EE749A62
internal static readonly UntypedFloat rb0 = /* -9.86494292470009928597e-03 */ -0.00986494; // 0xBF84341239E86F4A
internal static readonly UntypedFloat rb1 = /* -7.99283237680523006574e-01 */ -0.799283; // 0xBFE993BA70C285DE
internal static readonly UntypedFloat rb2 = /* -1.77579549177547519889e+01 */ -17.758; // 0xC031C209555F995A
internal static readonly UntypedFloat rb3 = /* -1.60636384855821916062e+02 */ -160.636; // 0xC064145D43C5ED98
internal static readonly UntypedFloat rb4 = /* -6.37566443368389627722e+02 */ -637.566; // 0xC083EC881375F228
internal static readonly UntypedFloat rb5 = /* -1.02509513161107724954e+03 */ -1025.1; // 0xC09004616A2E5992
internal static readonly UntypedFloat rb6 = /* -4.83519191608651397019e+02 */ -483.519; // 0xC07E384E9BDC383F
internal static readonly UntypedFloat sb1 = /* 3.03380607434824582924e+01 */ 30.3381;    // 0x403E568B261D5190
internal static readonly UntypedFloat sb2 = /* 3.25792512996573918826e+02 */ 325.793;    // 0x40745CAE221B9F0A
internal static readonly UntypedFloat sb3 = /* 1.53672958608443695994e+03 */ 1536.73;    // 0x409802EB189D5118
internal static readonly UntypedFloat sb4 = /* 3.19985821950859553908e+03 */ 3199.86;    // 0x40A8FFB7688C246A
internal static readonly UntypedFloat sb5 = /* 2.55305040643316442583e+03 */ 2553.05;    // 0x40A3F219CEDF3BE6
internal static readonly UntypedFloat sb6 = /* 4.74528541206955367215e+02 */ 474.529;    // 0x407DA874E79FE763
internal static readonly UntypedFloat sb7 = /* -2.24409524465858183362e+01 */ -22.441; // 0xC03670E242712D62

// Erf returns the error function of x.
//
// Special cases are:
//
//	Erf(+Inf) = 1
//	Erf(-Inf) = -1
//	Erf(NaN) = NaN
public static float64 Erf(float64 x) {
    if (haveArchErf) {
        return archErf(x);
    }
    return erf(x);
}

internal static float64 erf(float64 x) {
    static readonly UntypedFloat VeryTiny = /* 2.848094538889218e-306 */ 2.84809e-306; // 0x0080000000000000
    static readonly UntypedFloat Small = /* 1.0 / (1 << 28) */ 3.72529e-09; // 2**-28
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x): {
        return NaN();
    }
    case {} when IsInf(x, 1): {
        return 1;
    }
    case {} when IsInf(x, -1): {
        return -1;
    }}

    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    if (x < 0.84375F) {
        // |x| < 0.84375
        float64 temp = default!;
        if (x < Small){
            // |x| < 2**-28
            if (x < VeryTiny){
                temp = 0.125F * (8.0F * x + efx8 * x);
            } else {
                // avoid underflow
                temp = x + efx * x;
            }
        } else {
            var zΔ1 = x * x;
            var rΔ1 = pp0 + zΔ1 * (pp1 + zΔ1 * (pp2 + zΔ1 * (pp3 + zΔ1 * pp4)));
            var sΔ1 = 1 + zΔ1 * (qq1 + zΔ1 * (qq2 + zΔ1 * (qq3 + zΔ1 * (qq4 + zΔ1 * qq5))));
            var y = rΔ1 / sΔ1;
            temp = x + x * y;
        }
        if (sign) {
            return -temp;
        }
        return temp;
    }
    if (x < 1.25F) {
        // 0.84375 <= |x| < 1.25
        var sΔ2 = x - 1;
        var P = pa0 + sΔ2 * (pa1 + sΔ2 * (pa2 + sΔ2 * (pa3 + sΔ2 * (pa4 + sΔ2 * (pa5 + sΔ2 * pa6)))));
        var Q = 1 + sΔ2 * (qa1 + sΔ2 * (qa2 + sΔ2 * (qa3 + sΔ2 * (qa4 + sΔ2 * (qa5 + sΔ2 * qa6)))));
        if (sign) {
            return -erx - P / Q;
        }
        return erx + P / Q;
    }
    if (x >= 6) {
        // inf > |x| >= 6
        if (sign) {
            return -1;
        }
        return 1;
    }
    var s = 1 / (x * x);
    float64 R = default!;
    float64 S = default!;
    if (x < 1 / 0.35F){
        // |x| < 1 / 0.35  ~ 2.857143
        R = ra0 + s * (ra1 + s * (ra2 + s * (ra3 + s * (ra4 + s * (ra5 + s * (ra6 + s * ra7))))));
        S = 1 + s * (sa1 + s * (sa2 + s * (sa3 + s * (sa4 + s * (sa5 + s * (sa6 + s * (sa7 + s * sa8)))))));
    } else {
        // |x| >= 1 / 0.35  ~ 2.857143
        R = rb0 + s * (rb1 + s * (rb2 + s * (rb3 + s * (rb4 + s * (rb5 + s * rb6)))));
        S = 1 + s * (sb1 + s * (sb2 + s * (sb3 + s * (sb4 + s * (sb5 + s * (sb6 + s * sb7))))));
    }
    var z = Float64frombits((uint64)(Float64bits(x) & (nuint)18446744069414584320UL));
    // pseudo-single (20-bit) precision x
    var r = Exp(-z * z - 0.5625F) * Exp((z - x) * (z + x) + R / S);
    if (sign) {
        return r / x - 1;
    }
    return 1 - r / x;
}

// Erfc returns the complementary error function of x.
//
// Special cases are:
//
//	Erfc(+Inf) = 0
//	Erfc(-Inf) = 2
//	Erfc(NaN) = NaN
public static float64 Erfc(float64 x) {
    if (haveArchErfc) {
        return archErfc(x);
    }
    return erfc(x);
}

internal static float64 erfc(float64 x) {
    static readonly UntypedFloat Tiny = /* 1.0 / (1 << 56) */ 1.38778e-17; // 2**-56
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x): {
        return NaN();
    }
    case {} when IsInf(x, 1): {
        return 0;
    }
    case {} when IsInf(x, -1): {
        return 2;
    }}

    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    if (x < 0.84375F) {
        // |x| < 0.84375
        float64 temp = default!;
        if (x < Tiny){
            // |x| < 2**-56
            temp = x;
        } else {
            var z = x * x;
            var r = pp0 + z * (pp1 + z * (pp2 + z * (pp3 + z * pp4)));
            var s = 1 + z * (qq1 + z * (qq2 + z * (qq3 + z * (qq4 + z * qq5))));
            var y = r / s;
            if (x < 0.25F){
                // |x| < 1/4
                temp = x + x * y;
            } else {
                temp = 0.5F + (x * y + (x - 0.5F));
            }
        }
        if (sign) {
            return 1 + temp;
        }
        return 1 - temp;
    }
    if (x < 1.25F) {
        // 0.84375 <= |x| < 1.25
        var s = x - 1;
        var P = pa0 + s * (pa1 + s * (pa2 + s * (pa3 + s * (pa4 + s * (pa5 + s * pa6)))));
        var Q = 1 + s * (qa1 + s * (qa2 + s * (qa3 + s * (qa4 + s * (qa5 + s * qa6)))));
        if (sign) {
            return 1 + erx + P / Q;
        }
        return 1 - erx - P / Q;
    }
    if (x < 28) {
        // |x| < 28
        var s = 1 / (x * x);
        float64 R = default!;
        float64 S = default!;
        if (x < 1 / 0.35F){
            // |x| < 1 / 0.35 ~ 2.857143
            R = ra0 + s * (ra1 + s * (ra2 + s * (ra3 + s * (ra4 + s * (ra5 + s * (ra6 + s * ra7))))));
            S = 1 + s * (sa1 + s * (sa2 + s * (sa3 + s * (sa4 + s * (sa5 + s * (sa6 + s * (sa7 + s * sa8)))))));
        } else {
            // |x| >= 1 / 0.35 ~ 2.857143
            if (sign && x > 6) {
                return 2;
            }
            // x < -6
            R = rb0 + s * (rb1 + s * (rb2 + s * (rb3 + s * (rb4 + s * (rb5 + s * rb6)))));
            S = 1 + s * (sb1 + s * (sb2 + s * (sb3 + s * (sb4 + s * (sb5 + s * (sb6 + s * sb7))))));
        }
        var z = Float64frombits((uint64)(Float64bits(x) & (nuint)18446744069414584320UL));
        // pseudo-single (20-bit) precision x
        var r = Exp(-z * z - 0.5625F) * Exp((z - x) * (z + x) + R / S);
        if (sign) {
            return 2 - r / x;
        }
        return r / x;
    }
    if (sign) {
        return 2;
    }
    return 0;
}

} // end math_package
