// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements string-to-Float conversion functions.

// package big -- go2cs converted at 2022 March 06 22:17:44 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\floatconv.go
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;

namespace go.math;

public static partial class big_package {

private static Float floatZero = default;

// SetString sets z to the value of s and returns z and a boolean indicating
// success. s must be a floating-point number of the same format as accepted
// by Parse, with base argument 0. The entire string (not just a prefix) must
// be valid for success. If the operation failed, the value of z is undefined
// but the returned value is nil.
private static (ptr<Float>, bool) SetString(this ptr<Float> _addr_z, @string s) {
    ptr<Float> _p0 = default!;
    bool _p0 = default;
    ref Float z = ref _addr_z.val;

    {
        var (f, _, err) = z.Parse(s, 0);

        if (err == null) {
            return (_addr_f!, true);
        }
    }

    return (_addr_null!, false);

}

// scan is like Parse but reads the longest possible prefix representing a valid
// floating point number from an io.ByteScanner rather than a string. It serves
// as the implementation of Parse. It does not recognize ±Inf and does not expect
// EOF at the end.
private static (ptr<Float>, nint, error) scan(this ptr<Float> _addr_z, io.ByteScanner r, nint @base) => func((_, panic, _) => {
    ptr<Float> f = default!;
    nint b = default;
    error err = default!;
    ref Float z = ref _addr_z.val;

    var prec = z.prec;
    if (prec == 0) {
        prec = 64;
    }
    z.form = zero; 

    // sign
    z.neg, err = scanSign(r);
    if (err != null) {
        return ;
    }
    nint fcount = default; // fractional digit count; valid if <= 0
    z.mant, b, fcount, err = z.mant.scan(r, base, true);
    if (err != null) {
        return ;
    }
    long exp = default;
    nint ebase = default;
    exp, ebase, err = scanExponent(r, true, base == 0);
    if (err != null) {
        return ;
    }
    if (len(z.mant) == 0) {
        z.prec = prec;
        z.acc = Exact;
        z.form = zero;
        f = z;
        return ;
    }
    var exp2 = int64(len(z.mant)) * _W - fnorm(z.mant);
    var exp5 = int64(0); 

    // determine binary or decimal exponent contribution of radix point
    if (fcount < 0) { 
        // The mantissa has a radix point ddd.dddd; and
        // -fcount is the number of digits to the right
        // of '.'. Adjust relevant exponent accordingly.
        var d = int64(fcount);

        if (b == 10)
        {
            exp5 = d;
            fallthrough = true; // 10**e == 5**e * 2**e
        }
        if (fallthrough || b == 2)
        {
            exp2 += d;
            goto __switch_break0;
        }
        if (b == 8)
        {
            exp2 += d * 3; // octal digits are 3 bits each
            goto __switch_break0;
        }
        if (b == 16)
        {
            exp2 += d * 4; // hexadecimal digits are 4 bits each
            goto __switch_break0;
        }
        // default: 
            panic("unexpected mantissa base");

        __switch_break0:; 
        // fcount consumed - not needed anymore
    }

    if (ebase == 10)
    {
        exp5 += exp;
        fallthrough = true; // see fallthrough above
    }
    if (fallthrough || ebase == 2)
    {
        exp2 += exp;
        goto __switch_break1;
    }
    // default: 
        panic("unexpected exponent base");

    __switch_break1:; 
    // exp consumed - not needed anymore

    // apply 2**exp2
    if (MinExp <= exp2 && exp2 <= MaxExp) {
        z.prec = prec;
        z.form = finite;
        z.exp = int32(exp2);
        f = z;
    }
    else
 {
        err = fmt.Errorf("exponent overflow");
        return ;
    }
    if (exp5 == 0) { 
        // no decimal exponent contribution
        z.round(0);
        return ;

    }
    ptr<Float> p = @new<Float>().SetPrec(z.Prec() + 64); // use more bits for p -- TODO(gri) what is the right number?
    if (exp5 < 0) {
        z.Quo(z, p.pow5(uint64(-exp5)));
    }
    else
 {
        z.Mul(z, p.pow5(uint64(exp5)));
    }
    return ;

});

// These powers of 5 fit into a uint64.
//
//    for p, q := uint64(0), uint64(1); p < q; p, q = q, q*5 {
//        fmt.Println(q)
//    }
//
private static array<ulong> pow5tab = new array<ulong>(new ulong[] { 1, 5, 25, 125, 625, 3125, 15625, 78125, 390625, 1953125, 9765625, 48828125, 244140625, 1220703125, 6103515625, 30517578125, 152587890625, 762939453125, 3814697265625, 19073486328125, 95367431640625, 476837158203125, 2384185791015625, 11920928955078125, 59604644775390625, 298023223876953125, 1490116119384765625, 7450580596923828125 });

// pow5 sets z to 5**n and returns z.
// n must not be negative.
private static ptr<Float> pow5(this ptr<Float> _addr_z, ulong n) {
    ref Float z = ref _addr_z.val;

    const var m = uint64(len(pow5tab) - 1);

    if (n <= m) {
        return _addr_z.SetUint64(pow5tab[n])!;
    }
    z.SetUint64(pow5tab[m]);
    n -= m; 

    // use more bits for f than for z
    // TODO(gri) what is the right number?
    ptr<Float> f = @new<Float>().SetPrec(z.Prec() + 64).SetUint64(5);

    while (n > 0) {
        if (n & 1 != 0) {
            z.Mul(z, f);
        }
        f.Mul(f, f);
        n>>=1;

    }

    return _addr_z!;

}

// Parse parses s which must contain a text representation of a floating-
// point number with a mantissa in the given conversion base (the exponent
// is always a decimal number), or a string representing an infinite value.
//
// For base 0, an underscore character ``_'' may appear between a base
// prefix and an adjacent digit, and between successive digits; such
// underscores do not change the value of the number, or the returned
// digit count. Incorrect placement of underscores is reported as an
// error if there are no other errors. If base != 0, underscores are
// not recognized and thus terminate scanning like any other character
// that is not a valid radix point or digit.
//
// It sets z to the (possibly rounded) value of the corresponding floating-
// point value, and returns z, the actual base b, and an error err, if any.
// The entire string (not just a prefix) must be consumed for success.
// If z's precision is 0, it is changed to 64 before rounding takes effect.
// The number must be of the form:
//
//     number    = [ sign ] ( float | "inf" | "Inf" ) .
//     sign      = "+" | "-" .
//     float     = ( mantissa | prefix pmantissa ) [ exponent ] .
//     prefix    = "0" [ "b" | "B" | "o" | "O" | "x" | "X" ] .
//     mantissa  = digits "." [ digits ] | digits | "." digits .
//     pmantissa = [ "_" ] digits "." [ digits ] | [ "_" ] digits | "." digits .
//     exponent  = ( "e" | "E" | "p" | "P" ) [ sign ] digits .
//     digits    = digit { [ "_" ] digit } .
//     digit     = "0" ... "9" | "a" ... "z" | "A" ... "Z" .
//
// The base argument must be 0, 2, 8, 10, or 16. Providing an invalid base
// argument will lead to a run-time panic.
//
// For base 0, the number prefix determines the actual base: A prefix of
// ``0b'' or ``0B'' selects base 2, ``0o'' or ``0O'' selects base 8, and
// ``0x'' or ``0X'' selects base 16. Otherwise, the actual base is 10 and
// no prefix is accepted. The octal prefix "0" is not supported (a leading
// "0" is simply considered a "0").
//
// A "p" or "P" exponent indicates a base 2 (rather then base 10) exponent;
// for instance, "0x1.fffffffffffffp1023" (using base 0) represents the
// maximum float64 value. For hexadecimal mantissae, the exponent character
// must be one of 'p' or 'P', if present (an "e" or "E" exponent indicator
// cannot be distinguished from a mantissa digit).
//
// The returned *Float f is nil and the value of z is valid but not
// defined if an error is reported.
//
private static (ptr<Float>, nint, error) Parse(this ptr<Float> _addr_z, @string s, nint @base) {
    ptr<Float> f = default!;
    nint b = default;
    error err = default!;
    ref Float z = ref _addr_z.val;
 
    // scan doesn't handle ±Inf
    if (len(s) == 3 && (s == "Inf" || s == "inf")) {
        f = z.SetInf(false);
        return ;
    }
    if (len(s) == 4 && (s[0] == '+' || s[0] == '-') && (s[(int)1..] == "Inf" || s[(int)1..] == "inf")) {
        f = z.SetInf(s[0] == '-');
        return ;
    }
    var r = strings.NewReader(s);
    f, b, err = z.scan(r, base);

    if (err != null) {
        return ;
    }
    {
        var (ch, err2) = r.ReadByte();

        if (err2 == null) {
            err = fmt.Errorf("expected end of string, found %q", ch);
        }
        else if (err2 != io.EOF) {
            err = err2;
        }

    }


    return ;

}

// ParseFloat is like f.Parse(s, base) with f set to the given precision
// and rounding mode.
public static (ptr<Float>, nint, error) ParseFloat(@string s, nint @base, nuint prec, RoundingMode mode) {
    ptr<Float> f = default!;
    nint b = default;
    error err = default!;

    return @new<Float>().SetPrec(prec).SetMode(mode).Parse(s, base);
}

private static fmt.Scanner _ = (Float.val)(null); // *Float must implement fmt.Scanner

// Scan is a support routine for fmt.Scanner; it sets z to the value of
// the scanned number. It accepts formats whose verbs are supported by
// fmt.Scan for floating point values, which are:
// 'b' (binary), 'e', 'E', 'f', 'F', 'g' and 'G'.
// Scan doesn't handle ±Inf.
private static error Scan(this ptr<Float> _addr_z, fmt.ScanState s, int ch) {
    ref Float z = ref _addr_z.val;

    s.SkipSpace();
    var (_, _, err) = z.scan(new byteReader(s), 0);
    return error.As(err)!;
}

} // end big_package
