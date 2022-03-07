// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements rat-to-string conversion functions.

// package big -- go2cs converted at 2022 March 06 22:18:06 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\ratconv.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

namespace go.math;

public static partial class big_package {

private static bool ratTok(int ch) {
    return strings.ContainsRune("+-/0123456789.eE", ch);
}

private static Rat ratZero = default;
private static fmt.Scanner _ = _addr_ratZero; // *Rat must implement fmt.Scanner

// Scan is a support routine for fmt.Scanner. It accepts the formats
// 'e', 'E', 'f', 'F', 'g', 'G', and 'v'. All formats are equivalent.
private static error Scan(this ptr<Rat> _addr_z, fmt.ScanState s, int ch) {
    ref Rat z = ref _addr_z.val;

    var (tok, err) = s.Token(true, ratTok);
    if (err != null) {
        return error.As(err)!;
    }
    if (!strings.ContainsRune("efgEFGv", ch)) {
        return error.As(errors.New("Rat.Scan: invalid verb"))!;
    }
    {
        var (_, ok) = z.SetString(string(tok));

        if (!ok) {
            return error.As(errors.New("Rat.Scan: invalid syntax"))!;
        }
    }

    return error.As(null!)!;

}

// SetString sets z to the value of s and returns z and a boolean indicating
// success. s can be given as a (possibly signed) fraction "a/b", or as a
// floating-point number optionally followed by an exponent.
// If a fraction is provided, both the dividend and the divisor may be a
// decimal integer or independently use a prefix of ``0b'', ``0'' or ``0o'',
// or ``0x'' (or their upper-case variants) to denote a binary, octal, or
// hexadecimal integer, respectively. The divisor may not be signed.
// If a floating-point number is provided, it may be in decimal form or
// use any of the same prefixes as above but for ``0'' to denote a non-decimal
// mantissa. A leading ``0'' is considered a decimal leading 0; it does not
// indicate octal representation in this case.
// An optional base-10 ``e'' or base-2 ``p'' (or their upper-case variants)
// exponent may be provided as well, except for hexadecimal floats which
// only accept an (optional) ``p'' exponent (because an ``e'' or ``E'' cannot
// be distinguished from a mantissa digit). If the exponent's absolute value
// is too large, the operation may fail.
// The entire string, not just a prefix, must be valid for success. If the
// operation failed, the value of z is undefined but the returned value is nil.
private static (ptr<Rat>, bool) SetString(this ptr<Rat> _addr_z, @string s) => func((_, panic, _) => {
    ptr<Rat> _p0 = default!;
    bool _p0 = default;
    ref Rat z = ref _addr_z.val;

    if (len(s) == 0) {
        return (_addr_null!, false);
    }
    {
        var sep = strings.Index(s, "/");

        if (sep >= 0) {
            {
                var (_, ok) = z.a.SetString(s[..(int)sep], 0);

                if (!ok) {
                    return (_addr_null!, false);
                }

            }

            var r = strings.NewReader(s[(int)sep + 1..]);
            error err = default!;
            z.b.abs, _, _, err = z.b.abs.scan(r, 0, false);

            if (err != null) {
                return (_addr_null!, false);
            } 
            // entire string must have been consumed
            _, err = r.ReadByte();

            if (err != io.EOF) {
                return (_addr_null!, false);
            }

            if (len(z.b.abs) == 0) {
                return (_addr_null!, false);
            }

            return (_addr_z.norm()!, true);

        }
    } 

    // parse floating-point number
    r = strings.NewReader(s); 

    // sign
    var (neg, err) = scanSign(r);
    if (err != null) {
        return (_addr_null!, false);
    }
    nint @base = default;
    nint fcount = default; // fractional digit count; valid if <= 0
    z.a.abs, base, fcount, err = z.a.abs.scan(r, 0, true);
    if (err != null) {
        return (_addr_null!, false);
    }
    long exp = default;
    nint ebase = default;
    exp, ebase, err = scanExponent(r, true, true);
    if (err != null) {
        return (_addr_null!, false);
    }
    _, err = r.ReadByte();

    if (err != io.EOF) {
        return (_addr_null!, false);
    }
    if (len(z.a.abs) == 0) {
        return (_addr_z!, true);
    }
    long exp2 = default;    long exp5 = default;

    if (fcount < 0) { 
        // The mantissa has a radix point ddd.dddd; and
        // -fcount is the number of digits to the right
        // of '.'. Adjust relevant exponent accordingly.
        var d = int64(fcount);

        if (base == 10)
        {
            exp5 = d;
            fallthrough = true; // 10**e == 5**e * 2**e
        }
        if (fallthrough || base == 2)
        {
            exp2 = d;
            goto __switch_break0;
        }
        if (base == 8)
        {
            exp2 = d * 3; // octal digits are 3 bits each
            goto __switch_break0;
        }
        if (base == 16)
        {
            exp2 = d * 4; // hexadecimal digits are 4 bits each
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

    // apply exp5 contributions
    // (start with exp5 so the numbers to multiply are smaller)
    if (exp5 != 0) {
        var n = exp5;
        if (n < 0) {
            n = -n;
        }
        if (n > 1e6F) {
            return (_addr_null!, false); // avoid excessively large exponents
        }
        var pow5 = z.b.abs.expNN(natFive, nat(null).setWord(Word(n)), null); // use underlying array of z.b.abs
        if (exp5 > 0) {
            z.a.abs = z.a.abs.mul(z.a.abs, pow5);
            z.b.abs = z.b.abs.setWord(1);
        }
        else
 {
            z.b.abs = pow5;
        }
    }
    else
 {
        z.b.abs = z.b.abs.setWord(1);
    }
    if (exp2 < -1e7F || exp2 > 1e7F) {
        return (_addr_null!, false); // avoid excessively large exponents
    }
    if (exp2 > 0) {
        z.a.abs = z.a.abs.shl(z.a.abs, uint(exp2));
    }
    else if (exp2 < 0) {
        z.b.abs = z.b.abs.shl(z.b.abs, uint(-exp2));
    }
    z.a.neg = neg && len(z.a.abs) > 0; // 0 has no sign

    return (_addr_z.norm()!, true);

});

// scanExponent scans the longest possible prefix of r representing a base 10
// (``e'', ``E'') or a base 2 (``p'', ``P'') exponent, if any. It returns the
// exponent, the exponent base (10 or 2), or a read or syntax error, if any.
//
// If sepOk is set, an underscore character ``_'' may appear between successive
// exponent digits; such underscores do not change the value of the exponent.
// Incorrect placement of underscores is reported as an error if there are no
// other errors. If sepOk is not set, underscores are not recognized and thus
// terminate scanning like any other character that is not a valid digit.
//
//    exponent = ( "e" | "E" | "p" | "P" ) [ sign ] digits .
//    sign     = "+" | "-" .
//    digits   = digit { [ '_' ] digit } .
//    digit    = "0" ... "9" .
//
// A base 2 exponent is only permitted if base2ok is set.
private static (long, nint, error) scanExponent(io.ByteScanner r, bool base2ok, bool sepOk) {
    long exp = default;
    nint @base = default;
    error err = default!;
 
    // one char look-ahead
    var (ch, err) = r.ReadByte();
    if (err != null) {
        if (err == io.EOF) {
            err = null;
        }
        return (0, 10, error.As(err)!);

    }

    if (ch == 'e' || ch == 'E')
    {
        base = 10;
        goto __switch_break2;
    }
    if (ch == 'p' || ch == 'P')
    {
        if (base2ok) {
            base = 2;
            break; // ok
        }
    }
    // default: 
        r.UnreadByte(); // ch does not belong to exponent anymore
        return (0, 10, error.As(null!)!);

    __switch_break2:; 

    // sign
    slice<byte> digits = default;
    ch, err = r.ReadByte();
    if (err == null && (ch == '+' || ch == '-')) {
        if (ch == '-') {
            digits = append(digits, '-');
        }
        ch, err = r.ReadByte();

    }
    char prev = '.';
    var invalSep = false; 

    // exponent value
    var hasDigits = false;
    while (err == null) {
        if ('0' <= ch && ch <= '9') {
            digits = append(digits, ch);
            prev = '0';
            hasDigits = true;
        }
        else if (ch == '_' && sepOk) {
            if (prev != '0') {
                invalSep = true;
            }
            prev = '_';
        }
        else
 {
            r.UnreadByte(); // ch does not belong to number anymore
            break;

        }
        ch, err = r.ReadByte();

    }

    if (err == io.EOF) {
        err = null;
    }
    if (err == null && !hasDigits) {
        err = errNoDigits;
    }
    if (err == null) {
        exp, err = strconv.ParseInt(string(digits), 10, 64);
    }
    if (err == null && (invalSep || prev == '_')) {
        err = errInvalSep;
    }
    return ;

}

// String returns a string representation of x in the form "a/b" (even if b == 1).
private static @string String(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    return string(x.marshal());
}

// marshal implements String returning a slice of bytes
private static slice<byte> marshal(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    slice<byte> buf = default;
    buf = x.a.Append(buf, 10);
    buf = append(buf, '/');
    if (len(x.b.abs) != 0) {
        buf = x.b.Append(buf, 10);
    }
    else
 {
        buf = append(buf, '1');
    }
    return buf;

}

// RatString returns a string representation of x in the form "a/b" if b != 1,
// and in the form "a" if b == 1.
private static @string RatString(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    if (x.IsInt()) {
        return x.a.String();
    }
    return x.String();

}

// FloatString returns a string representation of x in decimal form with prec
// digits of precision after the radix point. The last digit is rounded to
// nearest, with halves rounded away from zero.
private static @string FloatString(this ptr<Rat> _addr_x, nint prec) {
    ref Rat x = ref _addr_x.val;

    slice<byte> buf = default;

    if (x.IsInt()) {
        buf = x.a.Append(buf, 10);
        if (prec > 0) {
            buf = append(buf, '.');
            {
                var i__prev1 = i;

                for (var i = prec; i > 0; i--) {
                    buf = append(buf, '0');
                }


                i = i__prev1;
            }

        }
        return string(buf);

    }
    var (q, r) = nat(null).div(nat(null), x.a.abs, x.b.abs);

    var p = natOne;
    if (prec > 0) {
        p = nat(null).expNN(natTen, nat(null).setUint64(uint64(prec)), null);
    }
    r = r.mul(r, p);
    var (r, r2) = r.div(nat(null), r, x.b.abs); 

    // see if we need to round up
    r2 = r2.add(r2, r2);
    if (x.b.abs.cmp(r2) <= 0) {
        r = r.add(r, natOne);
        if (r.cmp(p) >= 0) {
            q = nat(null).add(q, natOne);
            r = nat(null).sub(r, p);
        }
    }
    if (x.a.neg) {
        buf = append(buf, '-');
    }
    buf = append(buf, q.utoa(10)); // itoa ignores sign if q == 0

    if (prec > 0) {
        buf = append(buf, '.');
        var rs = r.utoa(10);
        {
            var i__prev1 = i;

            for (i = prec - len(rs); i > 0; i--) {
                buf = append(buf, '0');
            }


            i = i__prev1;
        }
        buf = append(buf, rs);

    }
    return string(buf);

}

} // end big_package
