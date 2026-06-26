// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Inverse of the floating-point error function.
*/
// This implementation is based on the rational approximation
// of percentage points of normal distribution available from
// https://www.jstor.org/stable/2347330.
internal static readonly UntypedFloat a0 = /* 1.1975323115670912564578e0 */ 1.19753;
internal static readonly UntypedFloat a1 = /* 4.7072688112383978012285e1 */ 47.0727;
internal static readonly UntypedFloat a2 = /* 6.9706266534389598238465e2 */ 697.063;
internal static readonly UntypedFloat a3 = /* 4.8548868893843886794648e3 */ 4854.89;
internal static readonly UntypedFloat a4 = /* 1.6235862515167575384252e4 */ 16235.9;
internal static readonly UntypedFloat a5 = 23782.041382114385;
internal static readonly UntypedFloat a6 = /* 1.1819493347062294404278e4 */ 11819.5;
internal static readonly UntypedFloat a7 = /* 8.8709406962545514830200e2 */ 887.094;
internal static readonly UntypedFloat b0 = 1;
internal static readonly UntypedFloat b1 = /* 4.2313330701600911252e1 */ 42.3133;
internal static readonly UntypedFloat b2 = /* 6.8718700749205790830e2 */ 687.187;
internal static readonly UntypedFloat b3 = /* 5.3941960214247511077e3 */ 5394.2;
internal static readonly UntypedFloat b4 = /* 2.1213794301586595867e4 */ 21213.8;
internal static readonly UntypedFloat b5 = /* 3.9307895800092710610e4 */ 39307.9;
internal static readonly UntypedFloat b6 = /* 2.8729085735721942674e4 */ 28729.1;
internal static readonly UntypedFloat b7 = /* 5.2264952788528545610e3 */ 5226.5;
internal static readonly UntypedFloat c0 = /* 1.42343711074968357734e0 */ 1.42344;
internal static readonly UntypedFloat c1 = /* 4.63033784615654529590e0 */ 4.63034;
internal static readonly UntypedFloat c2 = /* 5.76949722146069140550e0 */ 5.7695;
internal static readonly UntypedFloat c3 = /* 3.64784832476320460504e0 */ 3.64785;
internal static readonly UntypedFloat c4 = /* 1.27045825245236838258e0 */ 1.27046;
internal static readonly UntypedFloat c5 = /* 2.41780725177450611770e-1 */ 0.241781;
internal static readonly UntypedFloat c6 = /* 2.27238449892691845833e-2 */ 0.0227238;
internal static readonly UntypedFloat c7 = /* 7.74545014278341407640e-4 */ 0.000774545;
internal static readonly UntypedFloat d0 = /* 1.4142135623730950488016887e0 */ 1.41421;
internal static readonly UntypedFloat d1 = /* 2.9036514445419946173133295e0 */ 2.90365;
internal static readonly UntypedFloat d2 = /* 2.3707661626024532365971225e0 */ 2.37077;
internal static readonly UntypedFloat d3 = /* 9.7547832001787427186894837e-1 */ 0.975478;
internal static readonly UntypedFloat d4 = /* 2.0945065210512749128288442e-1 */ 0.209451;
internal static readonly UntypedFloat d5 = /* 2.1494160384252876777097297e-2 */ 0.0214942;
internal static readonly UntypedFloat d6 = /* 7.7441459065157709165577218e-4 */ 0.000774415;
internal static readonly UntypedFloat d7 = /* 1.4859850019840355905497876e-9 */ 1.48599e-09;
internal static readonly UntypedFloat e0 = /* 6.65790464350110377720e0 */ 6.6579;
internal static readonly UntypedFloat e1 = /* 5.46378491116411436990e0 */ 5.46378;
internal static readonly UntypedFloat e2 = /* 1.78482653991729133580e0 */ 1.78483;
internal static readonly UntypedFloat e3 = /* 2.96560571828504891230e-1 */ 0.296561;
internal static readonly UntypedFloat e4 = /* 2.65321895265761230930e-2 */ 0.0265322;
internal static readonly UntypedFloat e5 = /* 1.24266094738807843860e-3 */ 0.00124266;
internal static readonly UntypedFloat e6 = /* 2.71155556874348757815e-5 */ 2.71156e-05;
internal static readonly UntypedFloat e7 = /* 2.01033439929228813265e-7 */ 2.01033e-07;
internal static readonly UntypedFloat f0 = /* 1.414213562373095048801689e0 */ 1.41421;
internal static readonly UntypedFloat f1 = /* 8.482908416595164588112026e-1 */ 0.848291;
internal static readonly UntypedFloat f2 = /* 1.936480946950659106176712e-1 */ 0.193648;
internal static readonly UntypedFloat f3 = /* 2.103693768272068968719679e-2 */ 0.0210369;
internal static readonly UntypedFloat f4 = /* 1.112800997078859844711555e-3 */ 0.0011128;
internal static readonly UntypedFloat f5 = /* 2.611088405080593625138020e-5 */ 2.61109e-05;
internal static readonly UntypedFloat f6 = /* 2.010321207683943062279931e-7 */ 2.01032e-07;
internal static readonly UntypedFloat f7 = /* 2.891024605872965461538222e-15 */ 2.89102e-15;

// Erfinv returns the inverse error function of x.
//
// Special cases are:
//
//	Erfinv(1) = +Inf
//	Erfinv(-1) = -Inf
//	Erfinv(x) = NaN if x < -1 or x > 1
//	Erfinv(NaN) = NaN
public static float64 Erfinv(float64 x) {
    // special cases
    if (IsNaN(x) || x <= -1 || x >= 1) {
        if (x == -1 || x == 1) {
            return Inf(((nint)x));
        }
        return NaN();
    }
    var sign = false;
    if (x < 0) {
        x = -x;
        sign = true;
    }
    float64 ans = default!;
    if (x <= 0.85F){
        // |x| <= 0.85
        var r = 0.180625F - 0.25F * x * x;
        var z1Δ1 = ((((((a7 * r + a6) * r + a5) * r + a4) * r + a3) * r + a2) * r + a1) * r + a0;
        var z2Δ1 = ((((((b7 * r + b6) * r + b5) * r + b4) * r + b3) * r + b2) * r + b1) * r + b0;
        ans = (x * z1Δ1) / z2Δ1;
    } else {
        float64 z1 = default!;
        float64 z2 = default!;
        var r = Sqrt(Ln2 - Log(1.0F - x));
        if (r <= 5.0F){
            r -= 1.6F;
            z1 = ((((((c7 * r + c6) * r + c5) * r + c4) * r + c3) * r + c2) * r + c1) * r + c0;
            z2 = ((((((d7 * r + d6) * r + d5) * r + d4) * r + d3) * r + d2) * r + d1) * r + d0;
        } else {
            r -= 5.0F;
            z1 = ((((((e7 * r + e6) * r + e5) * r + e4) * r + e3) * r + e2) * r + e1) * r + e0;
            z2 = ((((((f7 * r + f6) * r + f5) * r + f4) * r + f3) * r + f2) * r + f1) * r + f0;
        }
        ans = z1 / z2;
    }
    if (sign) {
        return -ans;
    }
    return ans;
}

// Erfcinv returns the inverse of [Erfc](x).
//
// Special cases are:
//
//	Erfcinv(0) = +Inf
//	Erfcinv(2) = -Inf
//	Erfcinv(x) = NaN if x < 0 or x > 2
//	Erfcinv(NaN) = NaN
public static float64 Erfcinv(float64 x) {
    return Erfinv(1 - x);
}

} // end math_package
