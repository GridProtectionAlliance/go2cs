// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2022 March 06 22:30:34 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Program Files\Go\src\strconv\ftoaryu.go
using bits = go.math.bits_package;

namespace go;

public static partial class strconv_package {

    // binary to decimal conversion using the Ryū algorithm.
    //
    // See Ulf Adams, "Ryū: Fast Float-to-String Conversion" (doi:10.1145/3192366.3192369)
    //
    // Fixed precision formatting is a variant of the original paper's
    // algorithm, where a single multiplication by 10^k is required,
    // sharing the same rounding guarantees.

    // ryuFtoaFixed32 formats mant*(2^exp) with prec decimal digits.
private static void ryuFtoaFixed32(ptr<decimalSlice> _addr_d, uint mant, nint exp, nint prec) => func((_, panic, _) => {
    ref decimalSlice d = ref _addr_d.val;

    if (prec < 0) {
        panic("ryuFtoaFixed32 called with negative prec");
    }
    if (prec > 9) {
        panic("ryuFtoaFixed32 called with prec > 9");
    }
    if (mant == 0) {
        (d.nd, d.dp) = (0, 0);        return ;
    }
    var e2 = exp;
    {
        var b = bits.Len32(mant);

        if (b < 25) {
            mant<<=uint(25 - b);
            e2 += int(b) - 25;
        }
    } 
    // Choose an exponent such that rounded mant*(2^e2)*(10^q) has
    // at least prec decimal digits, i.e
    //     mant*(2^e2)*(10^q) >= 10^(prec-1)
    // Because mant >= 2^24, it is enough to choose:
    //     2^(e2+24) >= 10^(-q+prec-1)
    // or q = -mulByLog2Log10(e2+24) + prec - 1
    var q = -mulByLog2Log10(e2 + 24) + prec - 1; 

    // Now compute mant*(2^e2)*(10^q).
    // Is it an exact computation?
    // Only small positive powers of 10 are exact (5^28 has 66 bits).
    var exact = q <= 27 && q >= 0;

    var (di, dexp2, d0) = mult64bitPow10(mant, e2, q);
    if (dexp2 >= 0) {
        panic("not enough significant bits after mult64bitPow10");
    }
    if (q < 0 && q >= -10 && divisibleByPower5(uint64(mant), -q)) {
        exact = true;
        d0 = true;
    }
    var extra = uint(-dexp2);
    var extraMask = uint32(1 << (int)(extra) - 1);

    var di = di >> (int)(extra);
    var dfrac = di & extraMask;
    var roundUp = false;
    if (exact) { 
        // If we computed an exact product, d + 1/2
        // should round to d+1 if 'd' is odd.
        roundUp = dfrac > 1 << (int)((extra - 1)) || (dfrac == 1 << (int)((extra - 1)) && !d0) || (dfrac == 1 << (int)((extra - 1)) && d0 && di & 1 == 1);

    }
    else
 { 
        // otherwise, d+1/2 always rounds up because
        // we truncated below.
        roundUp = dfrac >> (int)((extra - 1)) == 1;

    }
    if (dfrac != 0) {
        d0 = false;
    }
    formatDecimal(_addr_d, uint64(di), !d0, roundUp, prec); 
    // Adjust exponent
    d.dp -= q;

});

// ryuFtoaFixed64 formats mant*(2^exp) with prec decimal digits.
private static void ryuFtoaFixed64(ptr<decimalSlice> _addr_d, ulong mant, nint exp, nint prec) => func((_, panic, _) => {
    ref decimalSlice d = ref _addr_d.val;

    if (prec > 18) {
        panic("ryuFtoaFixed64 called with prec > 18");
    }
    if (mant == 0) {
        (d.nd, d.dp) = (0, 0);        return ;
    }
    var e2 = exp;
    {
        var b = bits.Len64(mant);

        if (b < 55) {
            mant = mant << (int)(uint(55 - b));
            e2 += int(b) - 55;
        }
    } 
    // Choose an exponent such that rounded mant*(2^e2)*(10^q) has
    // at least prec decimal digits, i.e
    //     mant*(2^e2)*(10^q) >= 10^(prec-1)
    // Because mant >= 2^54, it is enough to choose:
    //     2^(e2+54) >= 10^(-q+prec-1)
    // or q = -mulByLog2Log10(e2+54) + prec - 1
    //
    // The minimal required exponent is -mulByLog2Log10(1025)+18 = -291
    // The maximal required exponent is mulByLog2Log10(1074)+18 = 342
    var q = -mulByLog2Log10(e2 + 54) + prec - 1; 

    // Now compute mant*(2^e2)*(10^q).
    // Is it an exact computation?
    // Only small positive powers of 10 are exact (5^55 has 128 bits).
    var exact = q <= 55 && q >= 0;

    var (di, dexp2, d0) = mult128bitPow10(mant, e2, q);
    if (dexp2 >= 0) {
        panic("not enough significant bits after mult128bitPow10");
    }
    if (q < 0 && q >= -22 && divisibleByPower5(mant, -q)) {
        exact = true;
        d0 = true;
    }
    var extra = uint(-dexp2);
    var extraMask = uint64(1 << (int)(extra) - 1);

    var di = di >> (int)(extra);
    var dfrac = di & extraMask;
    var roundUp = false;
    if (exact) { 
        // If we computed an exact product, d + 1/2
        // should round to d+1 if 'd' is odd.
        roundUp = dfrac > 1 << (int)((extra - 1)) || (dfrac == 1 << (int)((extra - 1)) && !d0) || (dfrac == 1 << (int)((extra - 1)) && d0 && di & 1 == 1);

    }
    else
 { 
        // otherwise, d+1/2 always rounds up because
        // we truncated below.
        roundUp = dfrac >> (int)((extra - 1)) == 1;

    }
    if (dfrac != 0) {
        d0 = false;
    }
    formatDecimal(_addr_d, di, !d0, roundUp, prec); 
    // Adjust exponent
    d.dp -= q;

});

private static array<ulong> uint64pow10 = new array<ulong>(new ulong[] { 1, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19 });

// formatDecimal fills d with at most prec decimal digits
// of mantissa m. The boolean trunc indicates whether m
// is truncated compared to the original number being formatted.
private static void formatDecimal(ptr<decimalSlice> _addr_d, ulong m, bool trunc, bool roundUp, nint prec) {
    ref decimalSlice d = ref _addr_d.val;

    var max = uint64pow10[prec];
    nint trimmed = 0;
    while (m >= max) {
        var a = m / 10;
        var b = m % 10;
        m = a;
        trimmed++;
        if (b > 5) {
            roundUp = true;
        }
        else if (b < 5) {
            roundUp = false;
        }
        else
 { // b == 5
            // round up if there are trailing digits,
            // or if the new value of m is odd (round-to-even convention)
            roundUp = trunc || m & 1 == 1;

        }
        if (b != 0) {
            trunc = true;
        }
    }
    if (roundUp) {
        m++;
    }
    if (m >= max) { 
        // Happens if di was originally 99999....xx
        m /= 10;
        trimmed++;

    }
    var n = uint(prec);
    d.nd = int(prec);
    var v = m;
    while (v >= 100) {
        ulong v1 = default;        ulong v2 = default;

        if (v >> 32 == 0) {
            (v1, v2) = (uint64(uint32(v) / 100), uint64(uint32(v) % 100));
        }
        else
 {
            (v1, v2) = (v / 100, v % 100);
        }
        n -= 2;
        d.d[n + 1] = smallsString[2 * v2 + 1];
        d.d[n + 0] = smallsString[2 * v2 + 0];
        v = v1;

    }
    if (v > 0) {
        n--;
        d.d[n] = smallsString[2 * v + 1];
    }
    if (v >= 10) {
        n--;
        d.d[n] = smallsString[2 * v];
    }
    while (d.d[d.nd - 1] == '0') {
        d.nd--;
        trimmed++;
    }
    d.dp = d.nd + trimmed;

}

// ryuFtoaShortest formats mant*2^exp with prec decimal digits.
private static void ryuFtoaShortest(ptr<decimalSlice> _addr_d, ulong mant, nint exp, ptr<floatInfo> _addr_flt) => func((_, panic, _) => {
    ref decimalSlice d = ref _addr_d.val;
    ref floatInfo flt = ref _addr_flt.val;

    if (mant == 0) {
        (d.nd, d.dp) = (0, 0);        return ;
    }
    if (exp <= 0 && bits.TrailingZeros64(mant) >= -exp) {
        mant>>=uint(-exp);
        ryuDigits(_addr_d, mant, mant, mant, true, false);
        return ;
    }
    var (ml, mc, mu, e2) = computeBounds(mant, exp, _addr_flt);
    if (e2 == 0) {
        ryuDigits(_addr_d, ml, mc, mu, true, false);
        return ;
    }
    var q = mulByLog2Log10(-e2) + 1; 

    // We are going to multiply by 10^q using 128-bit arithmetic.
    // The exponent is the same for all 3 numbers.
    ulong dl = default;    ulong dc = default;    ulong du = default;

    bool dl0 = default;    bool dc0 = default;    bool du0 = default;

    if (flt == _addr_float32info) {
        uint dl32 = default;        uint dc32 = default;        uint du32 = default;

        dl32, _, dl0 = mult64bitPow10(uint32(ml), e2, q);
        dc32, _, dc0 = mult64bitPow10(uint32(mc), e2, q);
        du32, e2, du0 = mult64bitPow10(uint32(mu), e2, q);
        (dl, dc, du) = (uint64(dl32), uint64(dc32), uint64(du32));
    }
    else
 {
        dl, _, dl0 = mult128bitPow10(ml, e2, q);
        dc, _, dc0 = mult128bitPow10(mc, e2, q);
        du, e2, du0 = mult128bitPow10(mu, e2, q);
    }
    if (e2 >= 0) {
        panic("not enough significant bits after mult128bitPow10");
    }
    if (q > 55) { 
        // Large positive powers of ten are not exact
        (dl0, dc0, du0) = (false, false, false);
    }
    if (q < 0 && q >= -24) { 
        // Division by a power of ten may be exact.
        // (note that 5^25 is a 59-bit number so division by 5^25 is never exact).
        if (divisibleByPower5(ml, -q)) {
            dl0 = true;
        }
        if (divisibleByPower5(mc, -q)) {
            dc0 = true;
        }
        if (divisibleByPower5(mu, -q)) {
            du0 = true;
        }
    }
    var extra = uint(-e2);
    var extraMask = uint64(1 << (int)(extra) - 1); 
    // Now compute the floored, integral base 10 mantissas.
    dl = dl >> (int)(extra);
    var fracl = dl & extraMask;
    dc = dc >> (int)(extra);
    var fracc = dc & extraMask;
    du = du >> (int)(extra);
    var fracu = du & extraMask; 
    // Is it allowed to use 'du' as a result?
    // It is always allowed when it is truncated, but also
    // if it is exact and the original binary mantissa is even
    // When disallowed, we can substract 1.
    var uok = !du0 || fracu > 0;
    if (du0 && fracu == 0) {
        uok = mant & 1 == 0;
    }
    if (!uok) {
        du--;
    }
    var cup = false; // don't round up.
    if (dc0) { 
        // If we computed an exact product, the half integer
        // should round to next (even) integer if 'dc' is odd.
        cup = fracc > 1 << (int)((extra - 1)) || (fracc == 1 << (int)((extra - 1)) && dc & 1 == 1);

    }
    else
 { 
        // otherwise, the result is a lower truncation of the ideal
        // result.
        cup = fracc >> (int)((extra - 1)) == 1;

    }
    var lok = dl0 && fracl == 0 && (mant & 1 == 0);
    if (!lok) {
        dl++;
    }
    var c0 = dc0 && fracc == 0; 
    // render digits
    ryuDigits(_addr_d, dl, dc, du, c0, cup);
    d.dp -= q;

});

// mulByLog2Log10 returns math.Floor(x * log(2)/log(10)) for an integer x in
// the range -1600 <= x && x <= +1600.
//
// The range restriction lets us work in faster integer arithmetic instead of
// slower floating point arithmetic. Correctness is verified by unit tests.
private static nint mulByLog2Log10(nint x) { 
    // log(2)/log(10) ≈ 0.30102999566 ≈ 78913 / 2^18
    return (x * 78913) >> 18;

}

// mulByLog10Log2 returns math.Floor(x * log(10)/log(2)) for an integer x in
// the range -500 <= x && x <= +500.
//
// The range restriction lets us work in faster integer arithmetic instead of
// slower floating point arithmetic. Correctness is verified by unit tests.
private static nint mulByLog10Log2(nint x) { 
    // log(10)/log(2) ≈ 3.32192809489 ≈ 108853 / 2^15
    return (x * 108853) >> 15;

}

// computeBounds returns a floating-point vector (l, c, u)×2^e2
// where the mantissas are 55-bit (or 26-bit) integers, describing the interval
// represented by the input float64 or float32.
private static (ulong, ulong, ulong, nint) computeBounds(ulong mant, nint exp, ptr<floatInfo> _addr_flt) {
    ulong lower = default;
    ulong central = default;
    ulong upper = default;
    nint e2 = default;
    ref floatInfo flt = ref _addr_flt.val;

    if (mant != 1 << (int)(flt.mantbits) || exp == flt.bias + 1 - int(flt.mantbits)) { 
        // regular case (or denormals)
        (lower, central, upper) = (2 * mant - 1, 2 * mant, 2 * mant + 1);        e2 = exp - 1;
        return ;

    }
    else
 { 
        // border of an exponent
        (lower, central, upper) = (4 * mant - 1, 4 * mant, 4 * mant + 2);        e2 = exp - 2;
        return ;

    }
}

private static void ryuDigits(ptr<decimalSlice> _addr_d, ulong lower, ulong central, ulong upper, bool c0, bool cup) {
    ref decimalSlice d = ref _addr_d.val;

    var (lhi, llo) = divmod1e9(lower);
    var (chi, clo) = divmod1e9(central);
    var (uhi, ulo) = divmod1e9(upper);
    if (uhi == 0) { 
        // only low digits (for denormals)
        ryuDigits32(_addr_d, llo, clo, ulo, c0, cup, 8);

    }
    else if (lhi < uhi) { 
        // truncate 9 digits at once.
        if (llo != 0) {
            lhi++;
        }
        c0 = c0 && clo == 0;
        cup = (clo > 5e8F) || (clo == 5e8F && cup);
        ryuDigits32(_addr_d, lhi, chi, uhi, c0, cup, 8);
        d.dp += 9;

    }
    else
 {
        d.nd = 0; 
        // emit high part
        var n = uint(9);
        {
            var v = chi;

            while (v > 0) {
                var v1 = v / 10;
                var v2 = v % 10;
                v = v1;
                n--;
                d.d[n] = byte(v2 + '0');

            }

        }
        d.d = d.d[(int)n..];
        d.nd = int(9 - n); 
        // emit low part
        ryuDigits32(_addr_d, llo, clo, ulo, c0, cup, d.nd + 8);

    }
    while (d.nd > 0 && d.d[d.nd - 1] == '0') {
        d.nd--;
    } 
    // trim initial zeros
    while (d.nd > 0 && d.d[0] == '0') {
        d.nd--;
        d.dp--;
        d.d = d.d[(int)1..];
    }

}

// ryuDigits32 emits decimal digits for a number less than 1e9.
private static void ryuDigits32(ptr<decimalSlice> _addr_d, uint lower, uint central, uint upper, bool c0, bool cup, nint endindex) {
    ref decimalSlice d = ref _addr_d.val;

    if (upper == 0) {
        d.dp = endindex + 1;
        return ;
    }
    nint trimmed = 0; 
    // Remember last trimmed digit to check for round-up.
    // c0 will be used to remember zeroness of following digits.
    nint cNextDigit = 0;
    while (upper > 0) { 
        // Repeatedly compute:
        // l = Ceil(lower / 10^k)
        // c = Round(central / 10^k)
        // u = Floor(upper / 10^k)
        // and stop when c goes out of the (l, u) interval.
        var l = (lower + 9) / 10;
        var c = central / 10;
        var cdigit = central % 10;
        var u = upper / 10;
        if (l > u) { 
            // don't trim the last digit as it is forbidden to go below l
            // other, trim and exit now.
            break;

        }
        if (l == c + 1 && c < u) {
            c++;
            cdigit = 0;
            cup = false;
        }
        trimmed++; 
        // Remember trimmed digits of c
        c0 = c0 && cNextDigit == 0;
        cNextDigit = int(cdigit);
        (lower, central, upper) = (l, c, u);
    } 
    // should we round up?
    if (trimmed > 0) {
        cup = cNextDigit > 5 || (cNextDigit == 5 && !c0) || (cNextDigit == 5 && c0 && central & 1 == 1);
    }
    if (central < upper && cup) {
        central++;
    }
    endindex -= trimmed;
    var v = central;
    var n = endindex;
    while (n > d.nd) {
        var v1 = v / 100;
        var v2 = v % 100;
        d.d[n] = smallsString[2 * v2 + 1];
        d.d[n - 1] = smallsString[2 * v2 + 0];
        n -= 2;
        v = v1;

    }
    if (n == d.nd) {
        d.d[n] = byte(v + '0');
    }
    d.nd = endindex + 1;
    d.dp = d.nd + trimmed;

}

// mult64bitPow10 takes a floating-point input with a 25-bit
// mantissa and multiplies it with 10^q. The resulting mantissa
// is m*P >> 57 where P is a 64-bit element of the detailedPowersOfTen tables.
// It is typically 31 or 32-bit wide.
// The returned boolean is true if all trimmed bits were zero.
//
// That is:
//     m*2^e2 * round(10^q) = resM * 2^resE + ε
//     exact = ε == 0
private static (uint, nint, bool) mult64bitPow10(uint m, nint e2, nint q) => func((_, panic, _) => {
    uint resM = default;
    nint resE = default;
    bool exact = default;

    if (q == 0) { 
        // P == 1<<63
        return (m << 6, e2 - 6, true);

    }
    if (q < detailedPowersOfTenMinExp10 || detailedPowersOfTenMaxExp10 < q) { 
        // This never happens due to the range of float32/float64 exponent
        panic("mult64bitPow10: power of 10 is out of range");

    }
    var pow = detailedPowersOfTen[q - detailedPowersOfTenMinExp10][1];
    if (q < 0) { 
        // Inverse powers of ten must be rounded up.
        pow += 1;

    }
    var (hi, lo) = bits.Mul64(uint64(m), pow);
    e2 += mulByLog10Log2(q) - 63 + 57;
    return (uint32(hi << 7 | lo >> 57), e2, lo << 7 == 0);

});

// mult128bitPow10 takes a floating-point input with a 55-bit
// mantissa and multiplies it with 10^q. The resulting mantissa
// is m*P >> 119 where P is a 128-bit element of the detailedPowersOfTen tables.
// It is typically 63 or 64-bit wide.
// The returned boolean is true is all trimmed bits were zero.
//
// That is:
//     m*2^e2 * round(10^q) = resM * 2^resE + ε
//     exact = ε == 0
private static (ulong, nint, bool) mult128bitPow10(ulong m, nint e2, nint q) => func((_, panic, _) => {
    ulong resM = default;
    nint resE = default;
    bool exact = default;

    if (q == 0) { 
        // P == 1<<127
        return (m << 8, e2 - 8, true);

    }
    if (q < detailedPowersOfTenMinExp10 || detailedPowersOfTenMaxExp10 < q) { 
        // This never happens due to the range of float32/float64 exponent
        panic("mult128bitPow10: power of 10 is out of range");

    }
    var pow = detailedPowersOfTen[q - detailedPowersOfTenMinExp10];
    if (q < 0) { 
        // Inverse powers of ten must be rounded up.
        pow[0] += 1;

    }
    e2 += mulByLog10Log2(q) - 127 + 119; 

    // long multiplication
    var (l1, l0) = bits.Mul64(m, pow[0]);
    var (h1, h0) = bits.Mul64(m, pow[1]);
    var (mid, carry) = bits.Add64(l1, h0, 0);
    h1 += carry;
    return (h1 << 9 | mid >> 55, e2, mid << 9 == 0 && l0 == 0);

});

private static bool divisibleByPower5(ulong m, nint k) {
    if (m == 0) {
        return true;
    }
    for (nint i = 0; i < k; i++) {
        if (m % 5 != 0) {
            return false;
        }
        m /= 5;

    }
    return true;

}

// divmod1e9 computes quotient and remainder of division by 1e9,
// avoiding runtime uint64 division on 32-bit platforms.
private static (uint, uint) divmod1e9(ulong x) {
    uint _p0 = default;
    uint _p0 = default;

    if (!host32bit) {
        return (uint32(x / 1e9F), uint32(x % 1e9F));
    }
    var (hi, _) = bits.Mul64(x >> 1, 0x89705f4136b4a598); // binary digits of 1e-9
    var q = hi >> 28;
    return (uint32(q), uint32(x - q * 1e9F));

}

} // end strconv_package
