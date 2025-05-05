// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bits = math.bits_package;
using math;

partial class strconv_package {

// binary to decimal conversion using the Ryū algorithm.
//
// See Ulf Adams, "Ryū: Fast Float-to-String Conversion" (doi:10.1145/3192366.3192369)
//
// Fixed precision formatting is a variant of the original paper's
// algorithm, where a single multiplication by 10^k is required,
// sharing the same rounding guarantees.

// ryuFtoaFixed32 formats mant*(2^exp) with prec decimal digits.
internal static void ryuFtoaFixed32(ж<decimalSlice> Ꮡd, uint32 mant, nint exp, nint prec) {
    ref var d = ref Ꮡd.val;

    if (prec < 0) {
        throw panic("ryuFtoaFixed32 called with negative prec");
    }
    if (prec > 9) {
        throw panic("ryuFtoaFixed32 called with prec > 9");
    }
    // Zero input.
    if (mant == 0) {
        (d.nd, d.dp) = (0, 0);
        return;
    }
    // Renormalize to a 25-bit mantissa.
    nint e2 = exp;
    {
        nint b = bits.Len32(mant); if (b < 25) {
            mant <<= (nuint)(((nuint)(25 - b)));
            e2 += b - 25;
        }
    }
    // Choose an exponent such that rounded mant*(2^e2)*(10^q) has
    // at least prec decimal digits, i.e
    //     mant*(2^e2)*(10^q) >= 10^(prec-1)
    // Because mant >= 2^24, it is enough to choose:
    //     2^(e2+24) >= 10^(-q+prec-1)
    // or q = -mulByLog2Log10(e2+24) + prec - 1
    nint q = -mulByLog2Log10(e2 + 24) + prec - 1;
    // Now compute mant*(2^e2)*(10^q).
    // Is it an exact computation?
    // Only small positive powers of 10 are exact (5^28 has 66 bits).
    var exact = q <= 27 && q >= 0;
    var (di, dexp2, d0) = mult64bitPow10(mant, e2, q);
    if (dexp2 >= 0) {
        throw panic("not enough significant bits after mult64bitPow10");
    }
    // As a special case, computation might still be exact, if exponent
    // was negative and if it amounts to computing an exact division.
    // In that case, we ignore all lower bits.
    // Note that division by 10^11 cannot be exact as 5^11 has 26 bits.
    if (q < 0 && q >= -10 && divisibleByPower5(((uint64)mant), -q)) {
        exact = true;
        d0 = true;
    }
    // Remove extra lower bits and keep rounding info.
    nuint extra = ((nuint)(-dexp2));
    var extraMask = ((uint32)(1 << (int)(extra) - 1));
    di = di >> (int)(extra);
    var dfrac = (uint32)(di & extraMask);
    var roundUp = false;
    if (exact){
        // If we computed an exact product, d + 1/2
        // should round to d+1 if 'd' is odd.
        roundUp = dfrac > 1 << (int)((extra - 1)) || (dfrac == 1 << (int)((extra - 1)) && !d0) || (dfrac == 1 << (int)((extra - 1)) && d0 && (uint32)(di & 1) == 1);
    } else {
        // otherwise, d+1/2 always rounds up because
        // we truncated below.
        roundUp = dfrac >> (int)((extra - 1)) == 1;
    }
    if (dfrac != 0) {
        d0 = false;
    }
    // Proceed to the requested number of digits
    formatDecimal(Ꮡd, ((uint64)di), !d0, roundUp, prec);
    // Adjust exponent
    d.dp -= q;
}

// ryuFtoaFixed64 formats mant*(2^exp) with prec decimal digits.
internal static void ryuFtoaFixed64(ж<decimalSlice> Ꮡd, uint64 mant, nint exp, nint prec) {
    ref var d = ref Ꮡd.val;

    if (prec > 18) {
        throw panic("ryuFtoaFixed64 called with prec > 18");
    }
    // Zero input.
    if (mant == 0) {
        (d.nd, d.dp) = (0, 0);
        return;
    }
    // Renormalize to a 55-bit mantissa.
    nint e2 = exp;
    {
        nint b = bits.Len64(mant); if (b < 55) {
            mant = mant << (int)(((nuint)(55 - b)));
            e2 += b - 55;
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
    nint q = -mulByLog2Log10(e2 + 54) + prec - 1;
    // Now compute mant*(2^e2)*(10^q).
    // Is it an exact computation?
    // Only small positive powers of 10 are exact (5^55 has 128 bits).
    var exact = q <= 55 && q >= 0;
    var (di, dexp2, d0) = mult128bitPow10(mant, e2, q);
    if (dexp2 >= 0) {
        throw panic("not enough significant bits after mult128bitPow10");
    }
    // As a special case, computation might still be exact, if exponent
    // was negative and if it amounts to computing an exact division.
    // In that case, we ignore all lower bits.
    // Note that division by 10^23 cannot be exact as 5^23 has 54 bits.
    if (q < 0 && q >= -22 && divisibleByPower5(mant, -q)) {
        exact = true;
        d0 = true;
    }
    // Remove extra lower bits and keep rounding info.
    nuint extra = ((nuint)(-dexp2));
    var extraMask = ((uint64)(1 << (int)(extra) - 1));
    di = di >> (int)(extra);
    var dfrac = (uint64)(di & extraMask);
    var roundUp = false;
    if (exact){
        // If we computed an exact product, d + 1/2
        // should round to d+1 if 'd' is odd.
        roundUp = dfrac > 1 << (int)((extra - 1)) || (dfrac == 1 << (int)((extra - 1)) && !d0) || (dfrac == 1 << (int)((extra - 1)) && d0 && (uint64)(di & 1) == 1);
    } else {
        // otherwise, d+1/2 always rounds up because
        // we truncated below.
        roundUp = dfrac >> (int)((extra - 1)) == 1;
    }
    if (dfrac != 0) {
        d0 = false;
    }
    // Proceed to the requested number of digits
    formatDecimal(Ꮡd, di, !d0, roundUp, prec);
    // Adjust exponent
    d.dp -= q;
}

internal static array<uint64> uint64pow10 = new uint64[]{
    1, 1e1F, 1e2F, 1e3F, 1e4F, 1e5F, 1e6F, 1e7F, 1e8F, 1e9F,
    1e10F, 1e11F, 1e12F, 1e13F, 1e14F, 1e15F, 1e16F, 1e17F, 1e18F, 1e19F
}.array();

// formatDecimal fills d with at most prec decimal digits
// of mantissa m. The boolean trunc indicates whether m
// is truncated compared to the original number being formatted.
internal static void formatDecimal(ж<decimalSlice> Ꮡd, uint64 m, bool trunc, bool roundUp, nint prec) {
    ref var d = ref Ꮡd.val;

    var max = uint64pow10[prec];
    nint trimmed = 0;
    while (m >= max) {
        var (a, b) = (m / 10, m % 10);
        m = a;
        trimmed++;
        if (b > 5){
            roundUp = true;
        } else 
        if (b < 5){
            roundUp = false;
        } else {
            // b == 5
            // round up if there are trailing digits,
            // or if the new value of m is odd (round-to-even convention)
            roundUp = trunc || (uint64)(m & 1) == 1;
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
    // render digits (similar to formatBits)
    nuint n = ((nuint)prec);
    d.nd = prec;
    var v = m;
    while (v >= 100) {
        uint64 v1 = default!;
        uint64 v2 = default!;
        if (v >> (int)(32) == 0){
            (v1, v2) = (((uint64)(((uint32)v) / 100)), ((uint64)(((uint32)v) % 100)));
        } else {
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
    while (d.d[d.nd - 1] == (rune)'0') {
        d.nd--;
        trimmed++;
    }
    d.dp = d.nd + trimmed;
}

// ryuFtoaShortest formats mant*2^exp with prec decimal digits.
internal static void ryuFtoaShortest(ж<decimalSlice> Ꮡd, uint64 mant, nint exp, ж<floatInfo> Ꮡflt) {
    ref var d = ref Ꮡd.val;
    ref var flt = ref Ꮡflt.val;

    if (mant == 0) {
        (d.nd, d.dp) = (0, 0);
        return;
    }
    // If input is an exact integer with fewer bits than the mantissa,
    // the previous and next integer are not admissible representations.
    if (exp <= 0 && bits.TrailingZeros64(mant) >= -exp) {
        mant >>= (nuint)(((nuint)(-exp)));
        ryuDigits(Ꮡd, mant, mant, mant, true, false);
        return;
    }
    var (ml, mc, mu, e2) = computeBounds(mant, exp, Ꮡflt);
    if (e2 == 0) {
        ryuDigits(Ꮡd, ml, mc, mu, true, false);
        return;
    }
    // Find 10^q *larger* than 2^-e2
    nint q = mulByLog2Log10(-e2) + 1;
    // We are going to multiply by 10^q using 128-bit arithmetic.
    // The exponent is the same for all 3 numbers.
    uint64 dl = default!;
    uint64 dc = default!;
    uint64 du = default!;
    bool dl0 = default!;
    bool dc0 = default!;
    bool du0 = default!;
    if (Ꮡflt == Ꮡ(float32info)){
        uint32 dl32 = default!;
        uint32 dc32 = default!;
        uint32 du32 = default!;
        (dl32, _, dl0) = mult64bitPow10(((uint32)ml), e2, q);
        (dc32, _, dc0) = mult64bitPow10(((uint32)mc), e2, q);
        (du32, e2, du0) = mult64bitPow10(((uint32)mu), e2, q);
        (dl, dc, du) = (((uint64)dl32), ((uint64)dc32), ((uint64)du32));
    } else {
        (dl, _, dl0) = mult128bitPow10(ml, e2, q);
        (dc, _, dc0) = mult128bitPow10(mc, e2, q);
        (du, e2, du0) = mult128bitPow10(mu, e2, q);
    }
    if (e2 >= 0) {
        throw panic("not enough significant bits after mult128bitPow10");
    }
    // Is it an exact computation?
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
    // Express the results (dl, dc, du)*2^e2 as integers.
    // Extra bits must be removed and rounding hints computed.
    nuint extra = ((nuint)(-e2));
    var extraMask = ((uint64)(1 << (int)(extra) - 1));
    // Now compute the floored, integral base 10 mantissas.
    dl = dl >> (int)(extra);
    var fracl = (uint64)(dl & extraMask);
    dc = dc >> (int)(extra);
    var fracc = (uint64)(dc & extraMask);
    du = du >> (int)(extra);
    var fracu = (uint64)(du & extraMask);
    // Is it allowed to use 'du' as a result?
    // It is always allowed when it is truncated, but also
    // if it is exact and the original binary mantissa is even
    // When disallowed, we can subtract 1.
    var uok = !du0 || fracu > 0;
    if (du0 && fracu == 0) {
        uok = (uint64)(mant & 1) == 0;
    }
    if (!uok) {
        du--;
    }
    // Is 'dc' the correctly rounded base 10 mantissa?
    // The correct rounding might be dc+1
    var cup = false;
    // don't round up.
    if (dc0){
        // If we computed an exact product, the half integer
        // should round to next (even) integer if 'dc' is odd.
        cup = fracc > 1 << (int)((extra - 1)) || (fracc == 1 << (int)((extra - 1)) && (uint64)(dc & 1) == 1);
    } else {
        // otherwise, the result is a lower truncation of the ideal
        // result.
        cup = fracc >> (int)((extra - 1)) == 1;
    }
    // Is 'dl' an allowed representation?
    // Only if it is an exact value, and if the original binary mantissa
    // was even.
    var lok = dl0 && fracl == 0 && ((uint64)(mant & 1) == 0);
    if (!lok) {
        dl++;
    }
    // We need to remember whether the trimmed digits of 'dc' are zero.
    var c0 = dc0 && fracc == 0;
    // render digits
    ryuDigits(Ꮡd, dl, dc, du, c0, cup);
    d.dp -= q;
}

// mulByLog2Log10 returns math.Floor(x * log(2)/log(10)) for an integer x in
// the range -1600 <= x && x <= +1600.
//
// The range restriction lets us work in faster integer arithmetic instead of
// slower floating point arithmetic. Correctness is verified by unit tests.
internal static nint mulByLog2Log10(nint x) {
    // log(2)/log(10) ≈ 0.30102999566 ≈ 78913 / 2^18
    return (x * 78913) >> (int)(18);
}

// mulByLog10Log2 returns math.Floor(x * log(10)/log(2)) for an integer x in
// the range -500 <= x && x <= +500.
//
// The range restriction lets us work in faster integer arithmetic instead of
// slower floating point arithmetic. Correctness is verified by unit tests.
internal static nint mulByLog10Log2(nint x) {
    // log(10)/log(2) ≈ 3.32192809489 ≈ 108853 / 2^15
    return (x * 108853) >> (int)(15);
}

// computeBounds returns a floating-point vector (l, c, u)×2^e2
// where the mantissas are 55-bit (or 26-bit) integers, describing the interval
// represented by the input float64 or float32.
internal static (uint64 lower, uint64 central, uint64 upper, nint e2) computeBounds(uint64 mant, nint exp, ж<floatInfo> Ꮡflt) {
    uint64 lower = default!;
    uint64 central = default!;
    uint64 upper = default!;
    nint e2 = default!;

    ref var flt = ref Ꮡflt.val;
    if (mant != 1 << (int)(flt.mantbits) || exp == flt.bias + 1 - ((nint)flt.mantbits)){
        // regular case (or denormals)
        (lower, central, upper) = (2 * mant - 1, 2 * mant, 2 * mant + 1);
        e2 = exp - 1;
        return (lower, central, upper, e2);
    } else {
        // border of an exponent
        (lower, central, upper) = (4 * mant - 1, 4 * mant, 4 * mant + 2);
        e2 = exp - 2;
        return (lower, central, upper, e2);
    }
}

internal static void ryuDigits(ж<decimalSlice> Ꮡd, uint64 lower, uint64 central, uint64 upper, bool c0, bool cup) {
    ref var d = ref Ꮡd.val;

    var (lhi, llo) = divmod1e9(lower);
    var (chi, clo) = divmod1e9(central);
    var (uhi, ulo) = divmod1e9(upper);
    if (uhi == 0){
        // only low digits (for denormals)
        ryuDigits32(Ꮡd, llo, clo, ulo, c0, cup, 8);
    } else 
    if (lhi < uhi){
        // truncate 9 digits at once.
        if (llo != 0) {
            lhi++;
        }
        c0 = c0 && clo == 0;
        cup = (clo > 5e8F) || (clo == 5e8F && cup);
        ryuDigits32(Ꮡd, lhi, chi, uhi, c0, cup, 8);
        d.dp += 9;
    } else {
        d.nd = 0;
        // emit high part
        nuint n = ((nuint)9);
        for (var v = chi; v > 0; ) {
            var (v1, v2) = (v / 10, v % 10);
            v = v1;
            n--;
            d.d[n] = ((byte)(v2 + (rune)'0'));
        }
        d.d = d.d[(int)(n)..];
        d.nd = ((nint)(9 - n));
        // emit low part
        ryuDigits32(Ꮡd, llo, clo, ulo,
            c0, cup, d.nd + 8);
    }
    // trim trailing zeros
    while (d.nd > 0 && d.d[d.nd - 1] == (rune)'0') {
        d.nd--;
    }
    // trim initial zeros
    while (d.nd > 0 && d.d[0] == (rune)'0') {
        d.nd--;
        d.dp--;
        d.d = d.d[1..];
    }
}

// ryuDigits32 emits decimal digits for a number less than 1e9.
internal static void ryuDigits32(ж<decimalSlice> Ꮡd, uint32 lower, uint32 central, uint32 upper, bool c0, bool cup, nint endindex) {
    ref var d = ref Ꮡd.val;

    if (upper == 0) {
        d.dp = endindex + 1;
        return;
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
        var (c, cdigit) = (central / 10, central % 10);
        var u = upper / 10;
        if (l > u) {
            // don't trim the last digit as it is forbidden to go below l
            // other, trim and exit now.
            break;
        }
        // Check that we didn't cross the lower boundary.
        // The case where l < u but c == l-1 is essentially impossible,
        // but may happen if:
        //    lower   = ..11
        //    central = ..19
        //    upper   = ..31
        // and means that 'central' is very close but less than
        // an integer ending with many zeros, and usually
        // the "round-up" logic hides the problem.
        if (l == c + 1 && c < u) {
            c++;
            cdigit = 0;
            cup = false;
        }
        trimmed++;
        // Remember trimmed digits of c
        c0 = c0 && cNextDigit == 0;
        cNextDigit = ((nint)cdigit);
        (lower, central, upper) = (l, c, u);
    }
    // should we round up?
    if (trimmed > 0) {
        cup = cNextDigit > 5 || (cNextDigit == 5 && !c0) || (cNextDigit == 5 && c0 && (uint32)(central & 1) == 1);
    }
    if (central < upper && cup) {
        central++;
    }
    // We know where the number ends, fill directly
    endindex -= trimmed;
    var v = central;
    nint n = endindex;
    while (n > d.nd) {
        var (v1, v2) = (v / 100, v % 100);
        d.d[n] = smallsString[2 * v2 + 1];
        d.d[n - 1] = smallsString[2 * v2 + 0];
        n -= 2;
        v = v1;
    }
    if (n == d.nd) {
        d.d[n] = ((byte)(v + (rune)'0'));
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
//
//	m*2^e2 * round(10^q) = resM * 2^resE + ε
//	exact = ε == 0
internal static (uint32 resM, nint resE, bool exact) mult64bitPow10(uint32 m, nint e2, nint q) {
    uint32 resM = default!;
    nint resE = default!;
    bool exact = default!;

    if (q == 0) {
        // P == 1<<63
        return (m << (int)(6), e2 - 6, true);
    }
    if (q < detailedPowersOfTenMinExp10 || detailedPowersOfTenMaxExp10 < q) {
        // This never happens due to the range of float32/float64 exponent
        throw panic("mult64bitPow10: power of 10 is out of range");
    }
    var pow = detailedPowersOfTen[q - detailedPowersOfTenMinExp10][1];
    if (q < 0) {
        // Inverse powers of ten must be rounded up.
        pow += 1;
    }
    var (hi, lo) = bits.Mul64(((uint64)m), pow);
    e2 += mulByLog10Log2(q) - 63 + 57;
    return (((uint32)((uint64)(hi << (int)(7) | lo >> (int)(57)))), e2, lo << (int)(7) == 0);
}

// mult128bitPow10 takes a floating-point input with a 55-bit
// mantissa and multiplies it with 10^q. The resulting mantissa
// is m*P >> 119 where P is a 128-bit element of the detailedPowersOfTen tables.
// It is typically 63 or 64-bit wide.
// The returned boolean is true is all trimmed bits were zero.
//
// That is:
//
//	m*2^e2 * round(10^q) = resM * 2^resE + ε
//	exact = ε == 0
internal static (uint64 resM, nint resE, bool exact) mult128bitPow10(uint64 m, nint e2, nint q) {
    uint64 resM = default!;
    nint resE = default!;
    bool exact = default!;

    if (q == 0) {
        // P == 1<<127
        return (m << (int)(8), e2 - 8, true);
    }
    if (q < detailedPowersOfTenMinExp10 || detailedPowersOfTenMaxExp10 < q) {
        // This never happens due to the range of float32/float64 exponent
        throw panic("mult128bitPow10: power of 10 is out of range");
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
    return ((uint64)(h1 << (int)(9) | mid >> (int)(55)), e2, mid << (int)(9) == 0 && l0 == 0);
}

internal static bool divisibleByPower5(uint64 m, nint k) {
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
internal static (uint32, uint32) divmod1e9(uint64 x) {
    if (!host32bit) {
        return (((uint32)(x / 1e9F)), ((uint32)(x % 1e9F)));
    }
    // Use the same sequence of operations as the amd64 compiler.
    var (hi, _) = bits.Mul64(x >> (int)(1), (nuint)9903520314283042200UL);
    // binary digits of 1e-9
    var q = hi >> (int)(28);
    return (((uint32)q), ((uint32)(x - q * 1e9F)));
}

} // end strconv_package
