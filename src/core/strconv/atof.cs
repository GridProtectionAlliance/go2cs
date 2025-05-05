// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// decimal to binary floating point conversion.
// Algorithm:
//   1) Store input in multiprecision decimal.
//   2) Multiply/divide decimal by powers of two until in range [0.5, 1)
//   3) Multiply by 2^precision and round to get mantissa.
using math = math_package;

partial class strconv_package {

internal static bool optimize = true; // set to false to force slow-path conversions for testing

// commonPrefixLenIgnoreCase returns the length of the common
// prefix of s and prefix, with the character case of s ignored.
// The prefix argument must be all lower-case.
internal static nint commonPrefixLenIgnoreCase(@string s, @string prefix) {
    nint n = len(prefix);
    if (n > len(s)) {
        n = len(s);
    }
    for (nint i = 0; i < n; i++) {
        var c = s[i];
        if ((rune)'A' <= c && c <= (rune)'Z') {
            c += (rune)'a' - (rune)'A';
        }
        if (c != prefix[i]) {
            return i;
        }
    }
    return n;
}

// special returns the floating-point value for the special,
// possibly signed floating-point representations inf, infinity,
// and NaN. The result is ok if a prefix of s contains one
// of these representations and n is the length of that prefix.
// The character case is ignored.
internal static (float64 f, nint n, bool ok) special(@string s) {
    float64 f = default!;
    nint n = default!;
    bool ok = default!;

    if (len(s) == 0) {
        return (0, 0, false);
    }
    nint sign = 1;
    nint nsign = 0;
    var exprᴛ1 = s[0];
    var matchᴛ1 = false;
    if (exprᴛ1 is (rune)'+' or (rune)'-') { matchᴛ1 = true;
        if (s[0] == (rune)'-') {
            sign = -1;
        }
        nsign = 1;
        s = s[1..];
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && (exprᴛ1 is (rune)'i' or (rune)'I')) {
        nint nΔ2 = commonPrefixLenIgnoreCase(s, "infinity"u8);
        if (3 < nΔ2 && nΔ2 < 8) {
            // Anything longer than "inf" is ok, but if we
            // don't have "infinity", only consume "inf".
            nΔ2 = 3;
        }
        if (nΔ2 == 3 || nΔ2 == 8) {
            return (math.Inf(sign), nsign + nΔ2, true);
        }
    }
    if (exprᴛ1 is (rune)'n' or (rune)'N') { matchᴛ1 = true;
        if (commonPrefixLenIgnoreCase(s, "nan"u8) == 3) {
            return (math.NaN(), 3, true);
        }
    }

    return (0, 0, false);
}

[GoRecv] internal static bool /*ok*/ set(this ref @decimal b, @string s) {
    bool ok = default!;

    nint i = 0;
    b.neg = false;
    b.trunc = false;
    // optional sign
    if (i >= len(s)) {
        return ok;
    }
    switch (ᐧ) {
    case {} when s[i] is (rune)'+': {
        i++;
        break;
    }
    case {} when s[i] is (rune)'-': {
        b.neg = true;
        i++;
        break;
    }}

    // digits
    var sawdot = false;
    var sawdigits = false;
    for (; i < len(s); i++) {
        switch (ᐧ) {
        case {} when s[i] is (rune)'_': {
            continue;
            break;
        }
        case {} when s[i] is (rune)'.': {
            if (sawdot) {
                // readFloat already checked underscores
                return ok;
            }
            sawdot = true;
            b.dp = b.nd;
            continue;
            break;
        }
        case {} when (rune)'0' <= s[i] && s[i] <= (rune)'9': {
            sawdigits = true;
            if (s[i] == (rune)'0' && b.nd == 0) {
                // ignore leading zeros
                b.dp--;
                continue;
            }
            if (b.nd < len(b.d)){
                b.d[b.nd] = s[i];
                b.nd++;
            } else 
            if (s[i] != (rune)'0') {
                b.trunc = true;
            }
            continue;
            break;
        }}

        break;
    }
    if (!sawdigits) {
        return ok;
    }
    if (!sawdot) {
        b.dp = b.nd;
    }
    // optional exponent moves decimal point.
    // if we read a very large, very long number,
    // just be sure to move the decimal point by
    // a lot (say, 100000).  it doesn't matter if it's
    // not the exact number.
    if (i < len(s) && lower(s[i]) == (rune)'e') {
        i++;
        if (i >= len(s)) {
            return ok;
        }
        nint esign = 1;
        if (s[i] == (rune)'+'){
            i++;
        } else 
        if (s[i] == (rune)'-') {
            i++;
            esign = -1;
        }
        if (i >= len(s) || s[i] < (rune)'0' || s[i] > (rune)'9') {
            return ok;
        }
        nint e = 0;
        for (; i < len(s) && ((rune)'0' <= s[i] && s[i] <= (rune)'9' || s[i] == (rune)'_'); i++) {
            if (s[i] == (rune)'_') {
                // readFloat already checked underscores
                continue;
            }
            if (e < 10000) {
                e = e * 10 + ((nint)s[i]) - (rune)'0';
            }
        }
        b.dp += e * esign;
    }
    if (i != len(s)) {
        return ok;
    }
    ok = true;
    return ok;
}

// readFloat reads a decimal or hexadecimal mantissa and exponent from a float
// string representation in s; the number may be followed by other characters.
// readFloat reports the number of bytes consumed (i), and whether the number
// is valid (ok).
internal static (uint64 mantissa, nint exp, bool neg, bool trunc, bool hex, nint i, bool ok) readFloat(@string s) {
    uint64 mantissa = default!;
    nint exp = default!;
    bool neg = default!;
    bool trunc = default!;
    bool hex = default!;
    nint i = default!;
    bool ok = default!;

    var underscores = false;
    // optional sign
    if (i >= len(s)) {
        return (mantissa, exp, neg, trunc, hex, i, ok);
    }
    switch (ᐧ) {
    case {} when s[i] is (rune)'+': {
        i++;
        break;
    }
    case {} when s[i] is (rune)'-': {
        neg = true;
        i++;
        break;
    }}

    // digits
    var @base = ((uint64)10);
    nint maxMantDigits = 19;
    // 10^19 fits in uint64
    var expChar = ((byte)(rune)'e');
    if (i + 2 < len(s) && s[i] == (rune)'0' && lower(s[i + 1]) == (rune)'x') {
        @base = 16;
        maxMantDigits = 16;
        // 16^16 fits in uint64
        i += 2;
        expChar = (rune)'p';
        hex = true;
    }
    var sawdot = false;
    var sawdigits = false;
    nint nd = 0;
    nint ndMant = 0;
    nint dp = 0;
loop:
    for (; i < len(s); i++) {
        {
            var c = s[i];
            switch (true) {
            case c == (rune)'_': {
                underscores = true;
                continue;
                break;
            }
            case c == (rune)'.': {
                if (sawdot) {
                    goto break_loop;
                }
                sawdot = true;
                dp = nd;
                continue;
                break;
            }
            case (rune)'0' <= c && c <= (rune)'9': {
                sawdigits = true;
                if (c == (rune)'0' && nd == 0) {
                    // ignore leading zeros
                    dp--;
                    continue;
                }
                nd++;
                if (ndMant < maxMantDigits){
                    mantissa *= @base;
                    mantissa += ((uint64)(c - (rune)'0'));
                    ndMant++;
                } else 
                if (c != (rune)'0') {
                    trunc = true;
                }
                continue;
                break;
            }
            case @base == 16 && (rune)'a' <= lower(c) && lower(c) <= (rune)'f': {
                sawdigits = true;
                nd++;
                if (ndMant < maxMantDigits){
                    mantissa *= 16;
                    mantissa += ((uint64)(lower(c) - (rune)'a' + 10));
                    ndMant++;
                } else {
                    trunc = true;
                }
                continue;
                break;
            }}
        }

        break;
continue_loop:;
    }
break_loop:;
    if (!sawdigits) {
        return (mantissa, exp, neg, trunc, hex, i, ok);
    }
    if (!sawdot) {
        dp = nd;
    }
    if (@base == 16) {
        dp *= 4;
        ndMant *= 4;
    }
    // optional exponent moves decimal point.
    // if we read a very large, very long number,
    // just be sure to move the decimal point by
    // a lot (say, 100000).  it doesn't matter if it's
    // not the exact number.
    if (i < len(s) && lower(s[i]) == expChar){
        i++;
        if (i >= len(s)) {
            return (mantissa, exp, neg, trunc, hex, i, ok);
        }
        nint esign = 1;
        if (s[i] == (rune)'+'){
            i++;
        } else 
        if (s[i] == (rune)'-') {
            i++;
            esign = -1;
        }
        if (i >= len(s) || s[i] < (rune)'0' || s[i] > (rune)'9') {
            return (mantissa, exp, neg, trunc, hex, i, ok);
        }
        nint e = 0;
        for (; i < len(s) && ((rune)'0' <= s[i] && s[i] <= (rune)'9' || s[i] == (rune)'_'); i++) {
            if (s[i] == (rune)'_') {
                underscores = true;
                continue;
            }
            if (e < 10000) {
                e = e * 10 + ((nint)s[i]) - (rune)'0';
            }
        }
        dp += e * esign;
    } else 
    if (@base == 16) {
        // Must have exponent.
        return (mantissa, exp, neg, trunc, hex, i, ok);
    }
    if (mantissa != 0) {
        exp = dp - ndMant;
    }
    if (underscores && !underscoreOK(s[..(int)(i)])) {
        return (mantissa, exp, neg, trunc, hex, i, ok);
    }
    ok = true;
    return (mantissa, exp, neg, trunc, hex, i, ok);
}

// decimal power of ten to binary power of two.
internal static slice<nint> powtab = new nint[]{1, 3, 6, 9, 13, 16, 19, 23, 26}.slice();

[GoRecv] public static (uint64 b, bool overflow) floatBits(this ref @decimal d, ж<floatInfo> Ꮡflt) {
    uint64 b = default!;
    bool overflow = default!;

    ref var flt = ref Ꮡflt.val;
    nint exp = default!;
    uint64 mant = default!;
    // Zero is always a special case.
    if (d.nd == 0) {
        mant = 0;
        exp = flt.bias;
        goto @out;
    }
    // Obvious overflow/underflow.
    // These bounds are for 64-bit floats.
    // Will have to change if we want to support 80-bit floats in the future.
    if (d.dp > 310) {
        goto overflow;
    }
    if (d.dp < -330) {
        // zero
        mant = 0;
        exp = flt.bias;
        goto @out;
    }
    // Scale by powers of two until in range [0.5, 1.0)
    exp = 0;
    while (d.dp > 0) {
        nint nΔ1 = default!;
        if (d.dp >= len(powtab)){
             = 27;
        } else {
             = powtab[d.dp];
        }
        d.Shift(-nΔ1);
        exp += nΔ1;
    }
    while (d.dp < 0 || d.dp == 0 && d.d[0] < (rune)'5') {
        nint n = default!;
        if (-d.dp >= len(powtab)){
            n = 27;
        } else {
            n = powtab[-d.dp];
        }
        d.Shift(n);
        exp -= n;
    }
    // Our range is [0.5,1) but floating point range is [1,2).
    exp--;
    // Minimum representable exponent is flt.bias+1.
    // If the exponent is smaller, move it up and
    // adjust d accordingly.
    if (exp < flt.bias + 1) {
        nint n = flt.bias + 1 - exp;
        d.Shift(-n);
        exp += n;
    }
    if (exp - flt.bias >= 1 << (int)(flt.expbits) - 1) {
        goto overflow;
    }
    // Extract 1+flt.mantbits bits.
    d.Shift(((nint)(1 + flt.mantbits)));
    mant = d.RoundedInteger();
    // Rounding might have added a bit; shift down.
    if (mant == 2 << (int)(flt.mantbits)) {
        mant >>= (UntypedInt)(1);
        exp++;
        if (exp - flt.bias >= 1 << (int)(flt.expbits) - 1) {
            goto overflow;
        }
    }
    // Denormalized?
    if ((uint64)(mant & (1 << (int)(flt.mantbits))) == 0) {
        exp = flt.bias;
    }
    goto @out;
overflow:
    mant = 0;
    // ±Inf
    exp = 1 << (int)(flt.expbits) - 1 + flt.bias;
    overflow = true;
@out:
    var bits = (uint64)(mant & (((uint64)1) << (int)(flt.mantbits) - 1));
    // Assemble bits.
    bits |= (uint64)(((uint64)((nint)((exp - flt.bias) & (1 << (int)(flt.expbits) - 1)))) << (int)(flt.mantbits));
    if (d.neg) {
        bits |= (uint64)(1 << (int)(flt.mantbits) << (int)(flt.expbits));
    }
    return (bits, overflow);
}

// Exact powers of 10.
internal static slice<float64> float64pow10 = new float64[]{
    1e0F, 1e1F, 1e2F, 1e3F, 1e4F, 1e5F, 1e6F, 1e7F, 1e8F, 1e9F,
    1e10F, 1e11F, 1e12F, 1e13F, 1e14F, 1e15F, 1e16F, 1e17F, 1e18F, 1e19F,
    1e20F, 1e21F, 1e22F
}.slice();

internal static slice<float32> float32pow10 = new float32[]{1e0F, 1e1F, 1e2F, 1e3F, 1e4F, 1e5F, 1e6F, 1e7F, 1e8F, 1e9F, 1e10F}.slice();

// If possible to convert decimal representation to 64-bit float f exactly,
// entirely in floating-point math, do so, avoiding the expense of decimalToFloatBits.
// Three common cases:
//
//	value is exact integer
//	value is exact integer * exact power of ten
//	value is exact integer / exact power of ten
//
// These all produce potentially inexact but correctly rounded answers.
internal static (float64 f, bool ok) atof64exact(uint64 mantissa, nint exp, bool neg) {
    float64 f = default!;
    bool ok = default!;

    if (mantissa >> (int)(float64info.mantbits) != 0) {
        return (f, ok);
    }
    f = ((float64)mantissa);
    if (neg) {
        f = -f;
    }
    switch (ᐧ) {
    case {} when exp is 0: {
        return (f, true);
    }
    case {} when exp > 0 && exp <= 15 + 22: {
        if (exp > 22) {
            // an integer.
            // Exact integers are <= 10^15.
            // Exact powers of ten are <= 10^22.
            // int * 10^k
            // If exponent is big but number of digits is not,
            // can move a few zeros into the integer part.
            f *= float64pow10[exp - 22];
            exp = 22;
        }
        if (f > 1e15F || f < -1e15F) {
            // the exponent was really too large.
            return (f, ok);
        }
        return (f * float64pow10[exp], true);
    }
    case {} when exp < 0 && exp >= -22: {
        return (f / float64pow10[-exp], true);
    }}

    // int / 10^k
    return (f, ok);
}

// If possible to compute mantissa*10^exp to 32-bit float f exactly,
// entirely in floating-point math, do so, avoiding the machinery above.
internal static (float32 f, bool ok) atof32exact(uint64 mantissa, nint exp, bool neg) {
    float32 f = default!;
    bool ok = default!;

    if (mantissa >> (int)(float32info.mantbits) != 0) {
        return (f, ok);
    }
    f = ((float32)mantissa);
    if (neg) {
        f = -f;
    }
    switch (ᐧ) {
    case {} when exp is 0: {
        return (f, true);
    }
    case {} when exp > 0 && exp <= 7 + 10: {
        if (exp > 10) {
            // Exact integers are <= 10^7.
            // Exact powers of ten are <= 10^10.
            // int * 10^k
            // If exponent is big but number of digits is not,
            // can move a few zeros into the integer part.
            f *= float32pow10[exp - 10];
            exp = 10;
        }
        if (f > 1e7F || f < -1e7F) {
            // the exponent was really too large.
            return (f, ok);
        }
        return (f * float32pow10[exp], true);
    }
    case {} when exp < 0 && exp >= -10: {
        return (f / float32pow10[-exp], true);
    }}

    // int / 10^k
    return (f, ok);
}

// atofHex converts the hex floating-point string s
// to a rounded float32 or float64 value (depending on flt==&float32info or flt==&float64info)
// and returns it as a float64.
// The string s has already been parsed into a mantissa, exponent, and sign (neg==true for negative).
// If trunc is true, trailing non-zero bits have been omitted from the mantissa.
internal static (float64, error) atofHex(@string s, ж<floatInfo> Ꮡflt, uint64 mantissa, nint exp, bool neg, bool trunc) {
    ref var flt = ref Ꮡflt.val;

    nint maxExp = 1 << (int)(flt.expbits) + flt.bias - 2;
    nint minExp = flt.bias + 1;
    exp += ((nint)flt.mantbits);
    // mantissa now implicitly divided by 2^mantbits.
    // Shift mantissa and exponent to bring representation into float range.
    // Eventually we want a mantissa with a leading 1-bit followed by mantbits other bits.
    // For rounding, we need two more, where the bottom bit represents
    // whether that bit or any later bit was non-zero.
    // (If the mantissa has already lost non-zero bits, trunc is true,
    // and we OR in a 1 below after shifting left appropriately.)
    while (mantissa != 0 && mantissa >> (int)((flt.mantbits + 2)) == 0) {
        mantissa <<= (UntypedInt)(1);
        exp--;
    }
    if (trunc) {
        mantissa |= (uint64)(1);
    }
    while (mantissa >> (int)((1 + flt.mantbits + 2)) != 0) {
        mantissa = (uint64)(mantissa >> (int)(1) | (uint64)(mantissa & 1));
        exp++;
    }
    // If exponent is too negative,
    // denormalize in hopes of making it representable.
    // (The -2 is for the rounding bits.)
    while (mantissa > 1 && exp < minExp - 2) {
        mantissa = (uint64)(mantissa >> (int)(1) | (uint64)(mantissa & 1));
        exp++;
    }
    // Round using two bottom bits.
    var round = (uint64)(mantissa & 3);
    mantissa >>= (UntypedInt)(2);
    round |= (uint64)((uint64)(mantissa & 1));
    // round to even (round up if mantissa is odd)
    exp += 2;
    if (round == 3) {
        mantissa++;
        if (mantissa == 1 << (int)((1 + flt.mantbits))) {
            mantissa >>= (UntypedInt)(1);
            exp++;
        }
    }
    if (mantissa >> (int)(flt.mantbits) == 0) {
        // Denormal or zero.
        exp = flt.bias;
    }
    error err = default!;
    if (exp > maxExp) {
        // infinity and range error
        mantissa = 1 << (int)(flt.mantbits);
        exp = maxExp + 1;
        err = ~rangeError(fnParseFloat, s);
    }
    var bits = (uint64)(mantissa & (1 << (int)(flt.mantbits) - 1));
    bits |= (uint64)(((uint64)((nint)((exp - flt.bias) & (1 << (int)(flt.expbits) - 1)))) << (int)(flt.mantbits));
    if (neg) {
        bits |= (uint64)(1 << (int)(flt.mantbits) << (int)(flt.expbits));
    }
    if (Ꮡflt == Ꮡ(float32info)) {
        return (((float64)math.Float32frombits(((uint32)bits))), err);
    }
    return (math.Float64frombits(bits), err);
}

internal static readonly @string fnParseFloat = "ParseFloat"u8;

internal static (float32 f, nint n, error err) atof32(@string s) {
    float32 f = default!;
    nint n = default!;
    error err = default!;

    {
        var (val, nΔ1, okΔ1) = special(s); if (okΔ1) {
            return (((float32)val), nΔ1, default!);
        }
    }
    var (mantissa, exp, neg, trunc, hex, n, ok) = readFloat(s);
    if (!ok) {
        return (0, n, ~syntaxError(fnParseFloat, s));
    }
    if (hex) {
        var (fΔ1, errΔ1) = atofHex(s[..(int)(n)], Ꮡ(float32info), mantissa, exp, neg, trunc);
        return (((float32)fΔ1), n, errΔ1);
    }
    if (optimize) {
        // Try pure floating-point arithmetic conversion, and if that fails,
        // the Eisel-Lemire algorithm.
        if (!trunc) {
            {
                var (fΔ2, okΔ2) = atof32exact(mantissa, exp, neg); if (okΔ2) {
                    return (fΔ2, n, default!);
                }
            }
        }
        var (fΔ3, ok) = eiselLemire32(mantissa, exp, neg);
        if (ok) {
            if (!trunc) {
                return (fΔ3, n, default!);
            }
            // Even if the mantissa was truncated, we may
            // have found the correct result. Confirm by
            // converting the upper mantissa bound.
            var (fUp, ok) = eiselLemire32(mantissa + 1, exp, neg);
            if (ok && fΔ3 == fUp) {
                return (fΔ3, n, default!);
            }
        }
    }
    // Slow fallback.
    @decimal d = default!;
    if (!d.set(s[..(int)(n)])) {
        return (0, n, ~syntaxError(fnParseFloat, s));
    }
    var (b, ovf) = d.floatBits(Ꮡ(float32info));
    f = math.Float32frombits(((uint32)b));
    if (ovf) {
        err = ~rangeError(fnParseFloat, s);
    }
    return (f, n, err);
}

internal static (float64 f, nint n, error err) atof64(@string s) {
    float64 f = default!;
    nint n = default!;
    error err = default!;

    {
        var (val, nΔ1, okΔ1) = special(s); if (okΔ1) {
            return (val, nΔ1, default!);
        }
    }
    var (mantissa, exp, neg, trunc, hex, n, ok) = readFloat(s);
    if (!ok) {
        return (0, n, ~syntaxError(fnParseFloat, s));
    }
    if (hex) {
        var (fΔ1, errΔ1) = atofHex(s[..(int)(n)], Ꮡ(float64info), mantissa, exp, neg, trunc);
        return (fΔ1, n, errΔ1);
    }
    if (optimize) {
        // Try pure floating-point arithmetic conversion, and if that fails,
        // the Eisel-Lemire algorithm.
        if (!trunc) {
            {
                var (fΔ2, okΔ2) = atof64exact(mantissa, exp, neg); if (okΔ2) {
                    return (fΔ2, n, default!);
                }
            }
        }
        var (fΔ3, ok) = eiselLemire64(mantissa, exp, neg);
        if (ok) {
            if (!trunc) {
                return (fΔ3, n, default!);
            }
            // Even if the mantissa was truncated, we may
            // have found the correct result. Confirm by
            // converting the upper mantissa bound.
            var (fUp, ok) = eiselLemire64(mantissa + 1, exp, neg);
            if (ok && fΔ3 == fUp) {
                return (fΔ3, n, default!);
            }
        }
    }
    // Slow fallback.
    @decimal d = default!;
    if (!d.set(s[..(int)(n)])) {
        return (0, n, ~syntaxError(fnParseFloat, s));
    }
    var (b, ovf) = d.floatBits(Ꮡ(float64info));
    f = math.Float64frombits(b);
    if (ovf) {
        err = ~rangeError(fnParseFloat, s);
    }
    return (f, n, err);
}

// ParseFloat converts the string s to a floating-point number
// with the precision specified by bitSize: 32 for float32, or 64 for float64.
// When bitSize=32, the result still has type float64, but it will be
// convertible to float32 without changing its value.
//
// ParseFloat accepts decimal and hexadecimal floating-point numbers
// as defined by the Go syntax for [floating-point literals].
// If s is well-formed and near a valid floating-point number,
// ParseFloat returns the nearest floating-point number rounded
// using IEEE754 unbiased rounding.
// (Parsing a hexadecimal floating-point value only rounds when
// there are more bits in the hexadecimal representation than
// will fit in the mantissa.)
//
// The errors that ParseFloat returns have concrete type *NumError
// and include err.Num = s.
//
// If s is not syntactically well-formed, ParseFloat returns err.Err = ErrSyntax.
//
// If s is syntactically well-formed but is more than 1/2 ULP
// away from the largest floating point number of the given size,
// ParseFloat returns f = ±Inf, err.Err = ErrRange.
//
// ParseFloat recognizes the string "NaN", and the (possibly signed) strings "Inf" and "Infinity"
// as their respective special floating point values. It ignores case when matching.
//
// [floating-point literals]: https://go.dev/ref/spec#Floating-point_literals
public static (float64, error) ParseFloat(@string s, nint bitSize) {
    var (f, n, err) = parseFloatPrefix(s, bitSize);
    if (n != len(s) && (err == default! || !AreEqual(err._<NumError.val>().Err, ErrSyntax))) {
        return (0, ~syntaxError(fnParseFloat, s));
    }
    return (f, err);
}

internal static (float64, nint, error) parseFloatPrefix(@string s, nint bitSize) {
    if (bitSize == 32) {
        var (f, n, err) = atof32(s);
        return (((float64)f), n, err);
    }
    return atof64(s);
}

} // end strconv_package
