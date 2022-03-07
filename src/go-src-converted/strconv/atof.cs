// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2022 March 06 22:30:28 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Program Files\Go\src\strconv\atof.go
// decimal to binary floating point conversion.
// Algorithm:
//   1) Store input in multiprecision decimal.
//   2) Multiply/divide decimal by powers of two until in range [0.5, 1)
//   3) Multiply by 2^precision and round to get mantissa.

using math = go.math_package;

namespace go;

public static partial class strconv_package {

private static var optimize = true; // set to false to force slow-path conversions for testing

// commonPrefixLenIgnoreCase returns the length of the common
// prefix of s and prefix, with the character case of s ignored.
// The prefix argument must be all lower-case.
private static nint commonPrefixLenIgnoreCase(@string s, @string prefix) {
    var n = len(prefix);
    if (n > len(s)) {
        n = len(s);
    }
    for (nint i = 0; i < n; i++) {
        var c = s[i];
        if ('A' <= c && c <= 'Z') {
            c += 'a' - 'A';
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
private static (double, nint, bool) special(@string s) {
    double f = default;
    nint n = default;
    bool ok = default;

    if (len(s) == 0) {
        return (0, 0, false);
    }
    nint sign = 1;
    nint nsign = 0;

    if (s[0] == '+' || s[0] == '-')
    {
        if (s[0] == '-') {
            sign = -1;
        }
        nsign = 1;
        s = s[(int)1..];
        fallthrough = true;
    }
    if (fallthrough || s[0] == 'i' || s[0] == 'I')
    {
        var n = commonPrefixLenIgnoreCase(s, "infinity"); 
        // Anything longer than "inf" is ok, but if we
        // don't have "infinity", only consume "inf".
        if (3 < n && n < 8) {
            n = 3;
        }
        if (n == 3 || n == 8) {
            return (math.Inf(sign), nsign + n, true);
        }
        goto __switch_break0;
    }
    if (s[0] == 'n' || s[0] == 'N')
    {
        if (commonPrefixLenIgnoreCase(s, "nan") == 3) {
            return (math.NaN(), 3, true);
        }
        goto __switch_break0;
    }

    __switch_break0:;
    return (0, 0, false);

}

private static bool set(this ptr<decimal> _addr_b, @string s) {
    bool ok = default;
    ref decimal b = ref _addr_b.val;

    nint i = 0;
    b.neg = false;
    b.trunc = false; 

    // optional sign
    if (i >= len(s)) {
        return ;
    }

    if (s[i] == '+') 
        i++;
    else if (s[i] == '-') 
        b.neg = true;
        i++;
    // digits
    var sawdot = false;
    var sawdigits = false;
    while (i < len(s)) {

        if (s[i] == '_') 
            // readFloat already checked underscores
            continue;
        else if (s[i] == '.') 
            if (sawdot) {
                return ;
        i++;
            }

            sawdot = true;
            b.dp = b.nd;
            continue;
        else if ('0' <= s[i] && s[i] <= '9') 
            sawdigits = true;
            if (s[i] == '0' && b.nd == 0) { // ignore leading zeros
                b.dp--;
                continue;

            }

            if (b.nd < len(b.d)) {
                b.d[b.nd] = s[i];
                b.nd++;
            }
            else if (s[i] != '0') {
                b.trunc = true;
            }

            continue;
                break;

    }
    if (!sawdigits) {
        return ;
    }
    if (!sawdot) {
        b.dp = b.nd;
    }
    if (i < len(s) && lower(s[i]) == 'e') {
        i++;
        if (i >= len(s)) {
            return ;
        }
        nint esign = 1;
        if (s[i] == '+') {
            i++;
        }
        else if (s[i] == '-') {
            i++;
            esign = -1;
        }
        if (i >= len(s) || s[i] < '0' || s[i] > '9') {
            return ;
        }
        nint e = 0;
        while (i < len(s) && ('0' <= s[i] && s[i] <= '9' || s[i] == '_')) {
            if (s[i] == '_') { 
                // readFloat already checked underscores
                continue;
            i++;
            }

            if (e < 10000) {
                e = e * 10 + int(s[i]) - '0';
            }

        }
        b.dp += e * esign;

    }
    if (i != len(s)) {
        return ;
    }
    ok = true;
    return ;

}

// readFloat reads a decimal or hexadecimal mantissa and exponent from a float
// string representation in s; the number may be followed by other characters.
// readFloat reports the number of bytes consumed (i), and whether the number
// is valid (ok).
private static (ulong, nint, bool, bool, bool, nint, bool) readFloat(@string s) {
    ulong mantissa = default;
    nint exp = default;
    bool neg = default;
    bool trunc = default;
    bool hex = default;
    nint i = default;
    bool ok = default;

    var underscores = false; 

    // optional sign
    if (i >= len(s)) {
        return ;
    }

    if (s[i] == '+') 
        i++;
    else if (s[i] == '-') 
        neg = true;
        i++;
    // digits
    var @base = uint64(10);
    nint maxMantDigits = 19; // 10^19 fits in uint64
    var expChar = byte('e');
    if (i + 2 < len(s) && s[i] == '0' && lower(s[i + 1]) == 'x') {
        base = 16;
        maxMantDigits = 16; // 16^16 fits in uint64
        i += 2;
        expChar = 'p';
        hex = true;

    }
    var sawdot = false;
    var sawdigits = false;
    nint nd = 0;
    nint ndMant = 0;
    nint dp = 0;
loop:
    while (i < len(s)) {
        {
            var c = s[i];


            if (true == c == '_') 
                underscores = true;
                continue;
            else if (true == c == '.') 
                if (sawdot) {
                    _breakloop = true;
                    break;
        i++;
                }

                sawdot = true;
                dp = nd;
                continue;
            else if (true == '0' <= c && c <= '9') 
                sawdigits = true;
                if (c == '0' && nd == 0) { // ignore leading zeros
                    dp--;
                    continue;

                }

                nd++;
                if (ndMant < maxMantDigits) {
                    mantissa *= base;
                    mantissa += uint64(c - '0');
                    ndMant++;
                }
                else if (c != '0') {
                    trunc = true;
                }

                continue;
            else if (true == base == 16 && 'a' <= lower(c) && lower(c) <= 'f') 
                sawdigits = true;
                nd++;
                if (ndMant < maxMantDigits) {
                    mantissa *= 16;
                    mantissa += uint64(lower(c) - 'a' + 10);
                    ndMant++;
                }
                else
 {
                    trunc = true;
                }

                continue;

        }
        break;

    }
    if (!sawdigits) {
        return ;
    }
    if (!sawdot) {
        dp = nd;
    }
    if (base == 16) {
        dp *= 4;
        ndMant *= 4;
    }
    if (i < len(s) && lower(s[i]) == expChar) {
        i++;
        if (i >= len(s)) {
            return ;
        }
        nint esign = 1;
        if (s[i] == '+') {
            i++;
        }
        else if (s[i] == '-') {
            i++;
            esign = -1;
        }
        if (i >= len(s) || s[i] < '0' || s[i] > '9') {
            return ;
        }
        nint e = 0;
        while (i < len(s) && ('0' <= s[i] && s[i] <= '9' || s[i] == '_')) {
            if (s[i] == '_') {
                underscores = true;
                continue;
            i++;
            }

            if (e < 10000) {
                e = e * 10 + int(s[i]) - '0';
            }

        }
        dp += e * esign;

    }
    else if (base == 16) { 
        // Must have exponent.
        return ;

    }
    if (mantissa != 0) {
        exp = dp - ndMant;
    }
    if (underscores && !underscoreOK(s[..(int)i])) {
        return ;
    }
    ok = true;
    return ;

}

// decimal power of ten to binary power of two.
private static nint powtab = new slice<nint>(new nint[] { 1, 3, 6, 9, 13, 16, 19, 23, 26 });

private static (ulong, bool) floatBits(this ptr<decimal> _addr_d, ptr<floatInfo> _addr_flt) {
    ulong b = default;
    bool overflow = default;
    ref decimal d = ref _addr_d.val;
    ref floatInfo flt = ref _addr_flt.val;

    nint exp = default;
    ulong mant = default; 

    // Zero is always a special case.
    if (d.nd == 0) {
        mant = 0;
        exp = flt.bias;
        goto @out;
    }
    if (d.dp > 310) {
        goto overflow;
    }
    if (d.dp < -330) { 
        // zero
        mant = 0;
        exp = flt.bias;
        goto @out;

    }
    exp = 0;
    while (d.dp > 0) {
        nint n = default;
        if (d.dp >= len(powtab)) {
            n = 27;
        }
        else
 {
            n = powtab[d.dp];
        }
        d.Shift(-n);
        exp += n;

    }
    while (d.dp < 0 || d.dp == 0 && d.d[0] < '5') {
        n = default;
        if (-d.dp >= len(powtab)) {
            n = 27;
        }
        else
 {
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
        n = flt.bias + 1 - exp;
        d.Shift(-n);
        exp += n;
    }
    if (exp - flt.bias >= 1 << (int)(flt.expbits) - 1) {
        goto overflow;
    }
    d.Shift(int(1 + flt.mantbits));
    mant = d.RoundedInteger(); 

    // Rounding might have added a bit; shift down.
    if (mant == 2 << (int)(flt.mantbits)) {
        mant>>=1;
        exp++;
        if (exp - flt.bias >= 1 << (int)(flt.expbits) - 1) {
            goto overflow;
        }
    }
    if (mant & (1 << (int)(flt.mantbits)) == 0) {
        exp = flt.bias;
    }
    goto @out;

overflow:
    mant = 0;
    exp = 1 << (int)(flt.expbits) - 1 + flt.bias;
    overflow = true;

@out:
    var bits = mant & (uint64(1) << (int)(flt.mantbits) - 1);
    bits |= uint64((exp - flt.bias) & (1 << (int)(flt.expbits) - 1)) << (int)(flt.mantbits);
    if (d.neg) {
        bits |= 1 << (int)(flt.mantbits) << (int)(flt.expbits);
    }
    return (bits, overflow);

}

// Exact powers of 10.
private static double float64pow10 = new slice<double>(new double[] { 1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22 });
private static float float32pow10 = new slice<float>(new float[] { 1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10 });

// If possible to convert decimal representation to 64-bit float f exactly,
// entirely in floating-point math, do so, avoiding the expense of decimalToFloatBits.
// Three common cases:
//    value is exact integer
//    value is exact integer * exact power of ten
//    value is exact integer / exact power of ten
// These all produce potentially inexact but correctly rounded answers.
private static (double, bool) atof64exact(ulong mantissa, nint exp, bool neg) {
    double f = default;
    bool ok = default;

    if (mantissa >> (int)(float64info.mantbits) != 0) {
        return ;
    }
    f = float64(mantissa);
    if (neg) {
        f = -f;
    }

    if (exp == 0) 
        // an integer.
        return (f, true); 
        // Exact integers are <= 10^15.
        // Exact powers of ten are <= 10^22.
    else if (exp > 0 && exp <= 15 + 22) // int * 10^k
        // If exponent is big but number of digits is not,
        // can move a few zeros into the integer part.
        if (exp > 22) {
            f *= float64pow10[exp - 22];
            exp = 22;
        }
        if (f > 1e15F || f < -1e15F) { 
            // the exponent was really too large.
            return ;

        }
        return (f * float64pow10[exp], true);
    else if (exp < 0 && exp >= -22) // int / 10^k
        return (f / float64pow10[-exp], true);
        return ;

}

// If possible to compute mantissa*10^exp to 32-bit float f exactly,
// entirely in floating-point math, do so, avoiding the machinery above.
private static (float, bool) atof32exact(ulong mantissa, nint exp, bool neg) {
    float f = default;
    bool ok = default;

    if (mantissa >> (int)(float32info.mantbits) != 0) {
        return ;
    }
    f = float32(mantissa);
    if (neg) {
        f = -f;
    }

    if (exp == 0) 
        return (f, true); 
        // Exact integers are <= 10^7.
        // Exact powers of ten are <= 10^10.
    else if (exp > 0 && exp <= 7 + 10) // int * 10^k
        // If exponent is big but number of digits is not,
        // can move a few zeros into the integer part.
        if (exp > 10) {
            f *= float32pow10[exp - 10];
            exp = 10;
        }
        if (f > 1e7F || f < -1e7F) { 
            // the exponent was really too large.
            return ;

        }
        return (f * float32pow10[exp], true);
    else if (exp < 0 && exp >= -10) // int / 10^k
        return (f / float32pow10[-exp], true);
        return ;

}

// atofHex converts the hex floating-point string s
// to a rounded float32 or float64 value (depending on flt==&float32info or flt==&float64info)
// and returns it as a float64.
// The string s has already been parsed into a mantissa, exponent, and sign (neg==true for negative).
// If trunc is true, trailing non-zero bits have been omitted from the mantissa.
private static (double, error) atofHex(@string s, ptr<floatInfo> _addr_flt, ulong mantissa, nint exp, bool neg, bool trunc) {
    double _p0 = default;
    error _p0 = default!;
    ref floatInfo flt = ref _addr_flt.val;

    nint maxExp = 1 << (int)(flt.expbits) + flt.bias - 2;
    var minExp = flt.bias + 1;
    exp += int(flt.mantbits); // mantissa now implicitly divided by 2^mantbits.

    // Shift mantissa and exponent to bring representation into float range.
    // Eventually we want a mantissa with a leading 1-bit followed by mantbits other bits.
    // For rounding, we need two more, where the bottom bit represents
    // whether that bit or any later bit was non-zero.
    // (If the mantissa has already lost non-zero bits, trunc is true,
    // and we OR in a 1 below after shifting left appropriately.)
    while (mantissa != 0 && mantissa >> (int)((flt.mantbits + 2)) == 0) {
        mantissa<<=1;
        exp--;
    }
    if (trunc) {
        mantissa |= 1;
    }
    while (mantissa >> (int)((1 + flt.mantbits + 2)) != 0) {
        mantissa = mantissa >> 1 | mantissa & 1;
        exp++;
    } 

    // If exponent is too negative,
    // denormalize in hopes of making it representable.
    // (The -2 is for the rounding bits.)
    while (mantissa > 1 && exp < minExp - 2) {
        mantissa = mantissa >> 1 | mantissa & 1;
        exp++;
    } 

    // Round using two bottom bits.
    var round = mantissa & 3;
    mantissa>>=2;
    round |= mantissa & 1; // round to even (round up if mantissa is odd)
    exp += 2;
    if (round == 3) {
        mantissa++;
        if (mantissa == 1 << (int)((1 + flt.mantbits))) {
            mantissa>>=1;
            exp++;
        }
    }
    if (mantissa >> (int)(flt.mantbits) == 0) { // Denormal or zero.
        exp = flt.bias;

    }
    error err = default!;
    if (exp > maxExp) { // infinity and range error
        mantissa = 1 << (int)(flt.mantbits);
        exp = maxExp + 1;
        err = error.As(rangeError(fnParseFloat, s))!;

    }
    var bits = mantissa & (1 << (int)(flt.mantbits) - 1);
    bits |= uint64((exp - flt.bias) & (1 << (int)(flt.expbits) - 1)) << (int)(flt.mantbits);
    if (neg) {
        bits |= 1 << (int)(flt.mantbits) << (int)(flt.expbits);
    }
    if (flt == _addr_float32info) {
        return (float64(math.Float32frombits(uint32(bits))), error.As(err)!);
    }
    return (math.Float64frombits(bits), error.As(err)!);

}

private static readonly @string fnParseFloat = "ParseFloat";



private static (float, nint, error) atof32(@string s) {
    float f = default;
    nint n = default;
    error err = default!;

    {
        var (val, n, ok) = special(s);

        if (ok) {
            return (float32(val), n, error.As(null!)!);
        }
    }


    var (mantissa, exp, neg, trunc, hex, n, ok) = readFloat(s);
    if (!ok) {
        return (0, n, error.As(syntaxError(fnParseFloat, s))!);
    }
    if (hex) {
        var (f, err) = atofHex(s[..(int)n], _addr_float32info, mantissa, exp, neg, trunc);
        return (float32(f), n, error.As(err)!);
    }
    if (optimize) { 
        // Try pure floating-point arithmetic conversion, and if that fails,
        // the Eisel-Lemire algorithm.
        if (!trunc) {
            {
                var f__prev3 = f;

                var (f, ok) = atof32exact(mantissa, exp, neg);

                if (ok) {
                    return (f, n, error.As(null!)!);
                }

                f = f__prev3;

            }

        }
        (f, ok) = eiselLemire32(mantissa, exp, neg);
        if (ok) {
            if (!trunc) {
                return (f, n, error.As(null!)!);
            } 
            // Even if the mantissa was truncated, we may
            // have found the correct result. Confirm by
            // converting the upper mantissa bound.
            var (fUp, ok) = eiselLemire32(mantissa + 1, exp, neg);
            if (ok && f == fUp) {
                return (f, n, error.As(null!)!);
            }

        }
    }
    decimal d = default;
    if (!d.set(s[..(int)n])) {
        return (0, n, error.As(syntaxError(fnParseFloat, s))!);
    }
    var (b, ovf) = d.floatBits(_addr_float32info);
    f = math.Float32frombits(uint32(b));
    if (ovf) {
        err = rangeError(fnParseFloat, s);
    }
    return (f, n, error.As(err)!);

}

private static (double, nint, error) atof64(@string s) {
    double f = default;
    nint n = default;
    error err = default!;

    {
        var (val, n, ok) = special(s);

        if (ok) {
            return (val, n, error.As(null!)!);
        }
    }


    var (mantissa, exp, neg, trunc, hex, n, ok) = readFloat(s);
    if (!ok) {
        return (0, n, error.As(syntaxError(fnParseFloat, s))!);
    }
    if (hex) {
        var (f, err) = atofHex(s[..(int)n], _addr_float64info, mantissa, exp, neg, trunc);
        return (f, n, error.As(err)!);
    }
    if (optimize) { 
        // Try pure floating-point arithmetic conversion, and if that fails,
        // the Eisel-Lemire algorithm.
        if (!trunc) {
            {
                var f__prev3 = f;

                var (f, ok) = atof64exact(mantissa, exp, neg);

                if (ok) {
                    return (f, n, error.As(null!)!);
                }

                f = f__prev3;

            }

        }
        (f, ok) = eiselLemire64(mantissa, exp, neg);
        if (ok) {
            if (!trunc) {
                return (f, n, error.As(null!)!);
            } 
            // Even if the mantissa was truncated, we may
            // have found the correct result. Confirm by
            // converting the upper mantissa bound.
            var (fUp, ok) = eiselLemire64(mantissa + 1, exp, neg);
            if (ok && f == fUp) {
                return (f, n, error.As(null!)!);
            }

        }
    }
    decimal d = default;
    if (!d.set(s[..(int)n])) {
        return (0, n, error.As(syntaxError(fnParseFloat, s))!);
    }
    var (b, ovf) = d.floatBits(_addr_float64info);
    f = math.Float64frombits(b);
    if (ovf) {
        err = rangeError(fnParseFloat, s);
    }
    return (f, n, error.As(err)!);

}

// ParseFloat converts the string s to a floating-point number
// with the precision specified by bitSize: 32 for float32, or 64 for float64.
// When bitSize=32, the result still has type float64, but it will be
// convertible to float32 without changing its value.
//
// ParseFloat accepts decimal and hexadecimal floating-point number syntax.
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
// ParseFloat recognizes the strings "NaN", and the (possibly signed) strings "Inf" and "Infinity"
// as their respective special floating point values. It ignores case when matching.
public static (double, error) ParseFloat(@string s, nint bitSize) {
    double _p0 = default;
    error _p0 = default!;

    var (f, n, err) = parseFloatPrefix(s, bitSize);
    if (n != len(s) && (err == null || err._<ptr<NumError>>().Err != ErrSyntax)) {
        return (0, error.As(syntaxError(fnParseFloat, s))!);
    }
    return (f, error.As(err)!);

}

private static (double, nint, error) parseFloatPrefix(@string s, nint bitSize) {
    double _p0 = default;
    nint _p0 = default;
    error _p0 = default!;

    if (bitSize == 32) {
        var (f, n, err) = atof32(s);
        return (float64(f), n, error.As(err)!);
    }
    return atof64(s);

}

} // end strconv_package
