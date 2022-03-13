// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2022 March 13 05:41:18 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Program Files\Go\src\strconv\atoi.go
namespace go;

using errors = errors_package;

public static partial class strconv_package {

// lower(c) is a lower-case letter if and only if
// c is either that lower-case letter or the equivalent upper-case letter.
// Instead of writing c == 'x' || c == 'X' one can write lower(c) == 'x'.
// Note that lower of non-letters can produce other non-letters.
private static byte lower(byte c) {
    return c | ('x' - 'X');
}

// ErrRange indicates that a value is out of range for the target type.
public static var ErrRange = errors.New("value out of range");

// ErrSyntax indicates that a value does not have the right syntax for the target type.
public static var ErrSyntax = errors.New("invalid syntax");

// A NumError records a failed conversion.
public partial struct NumError {
    public @string Func; // the failing function (ParseBool, ParseInt, ParseUint, ParseFloat, ParseComplex)
    public @string Num; // the input
    public error Err; // the reason the conversion failed (e.g. ErrRange, ErrSyntax, etc.)
}

private static @string Error(this ptr<NumError> _addr_e) {
    ref NumError e = ref _addr_e.val;

    return "strconv." + e.Func + ": " + "parsing " + Quote(e.Num) + ": " + e.Err.Error();
}

private static error Unwrap(this ptr<NumError> _addr_e) {
    ref NumError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

private static ptr<NumError> syntaxError(@string fn, @string str) {
    return addr(new NumError(fn,str,ErrSyntax));
}

private static ptr<NumError> rangeError(@string fn, @string str) {
    return addr(new NumError(fn,str,ErrRange));
}

private static ptr<NumError> baseError(@string fn, @string str, nint @base) {
    return addr(new NumError(fn,str,errors.New("invalid base "+Itoa(base))));
}

private static ptr<NumError> bitSizeError(@string fn, @string str, nint bitSize) {
    return addr(new NumError(fn,str,errors.New("invalid bit size "+Itoa(bitSize))));
}

private static readonly nint intSize = 32 << (int)((~uint(0) >> 63));

// IntSize is the size in bits of an int or uint value.


// IntSize is the size in bits of an int or uint value.
public static readonly var IntSize = intSize;



private static readonly nint maxUint64 = 1 << 64 - 1;

// ParseUint is like ParseInt but for unsigned numbers.
//
// A sign prefix is not permitted.


// ParseUint is like ParseInt but for unsigned numbers.
//
// A sign prefix is not permitted.
public static (ulong, error) ParseUint(@string s, nint @base, nint bitSize) {
    ulong _p0 = default;
    error _p0 = default!;

    const @string fnParseUint = "ParseUint";



    if (s == "") {
        return (0, error.As(syntaxError(fnParseUint, s))!);
    }
    var base0 = base == 0;

    var s0 = s;

    if (2 <= base && base <= 36)     else if (base == 0) 
        // Look for octal, hex prefix.
        base = 10;
        if (s[0] == '0') {

            if (len(s) >= 3 && lower(s[1]) == 'b') 
                base = 2;
                s = s[(int)2..];
            else if (len(s) >= 3 && lower(s[1]) == 'o') 
                base = 8;
                s = s[(int)2..];
            else if (len(s) >= 3 && lower(s[1]) == 'x') 
                base = 16;
                s = s[(int)2..];
            else 
                base = 8;
                s = s[(int)1..];
                    }
    else 
        return (0, error.As(baseError(fnParseUint, s0, base))!);
        if (bitSize == 0) {
        bitSize = IntSize;
    }
    else if (bitSize < 0 || bitSize > 64) {
        return (0, error.As(bitSizeError(fnParseUint, s0, bitSize))!);
    }
    ulong cutoff = default;
    switch (base) {
        case 10: 
            cutoff = maxUint64 / 10 + 1;
            break;
        case 16: 
            cutoff = maxUint64 / 16 + 1;
            break;
        default: 
            cutoff = maxUint64 / uint64(base) + 1;
            break;
    }

    var maxVal = uint64(1) << (int)(uint(bitSize)) - 1;

    var underscores = false;
    ulong n = default;
    foreach (var (_, c) in (slice<byte>)s) {
        byte d = default;

        if (c == '_' && base0) 
            underscores = true;
            continue;
        else if ('0' <= c && c <= '9') 
            d = c - '0';
        else if ('a' <= lower(c) && lower(c) <= 'z') 
            d = lower(c) - 'a' + 10;
        else 
            return (0, error.As(syntaxError(fnParseUint, s0))!);
                if (d >= byte(base)) {
            return (0, error.As(syntaxError(fnParseUint, s0))!);
        }
        if (n >= cutoff) { 
            // n*base overflows
            return (maxVal, error.As(rangeError(fnParseUint, s0))!);
        }
        n *= uint64(base);

        var n1 = n + uint64(d);
        if (n1 < n || n1 > maxVal) { 
            // n+d overflows
            return (maxVal, error.As(rangeError(fnParseUint, s0))!);
        }
        n = n1;
    }    if (underscores && !underscoreOK(s0)) {
        return (0, error.As(syntaxError(fnParseUint, s0))!);
    }
    return (n, error.As(null!)!);
}

// ParseInt interprets a string s in the given base (0, 2 to 36) and
// bit size (0 to 64) and returns the corresponding value i.
//
// The string may begin with a leading sign: "+" or "-".
//
// If the base argument is 0, the true base is implied by the string's
// prefix following the sign (if present): 2 for "0b", 8 for "0" or "0o",
// 16 for "0x", and 10 otherwise. Also, for argument base 0 only,
// underscore characters are permitted as defined by the Go syntax for
// integer literals.
//
// The bitSize argument specifies the integer type
// that the result must fit into. Bit sizes 0, 8, 16, 32, and 64
// correspond to int, int8, int16, int32, and int64.
// If bitSize is below 0 or above 64, an error is returned.
//
// The errors that ParseInt returns have concrete type *NumError
// and include err.Num = s. If s is empty or contains invalid
// digits, err.Err = ErrSyntax and the returned value is 0;
// if the value corresponding to s cannot be represented by a
// signed integer of the given size, err.Err = ErrRange and the
// returned value is the maximum magnitude integer of the
// appropriate bitSize and sign.
public static (long, error) ParseInt(@string s, nint @base, nint bitSize) {
    long i = default;
    error err = default!;

    const @string fnParseInt = "ParseInt";



    if (s == "") {
        return (0, error.As(syntaxError(fnParseInt, s))!);
    }
    var s0 = s;
    var neg = false;
    if (s[0] == '+') {
        s = s[(int)1..];
    }
    else if (s[0] == '-') {
        neg = true;
        s = s[(int)1..];
    }
    ulong un = default;
    un, err = ParseUint(s, base, bitSize);
    if (err != null && err._<ptr<NumError>>().Err != ErrRange) {
        err._<ptr<NumError>>().Func = fnParseInt;
        err._<ptr<NumError>>().Num = s0;
        return (0, error.As(err)!);
    }
    if (bitSize == 0) {
        bitSize = IntSize;
    }
    var cutoff = uint64(1 << (int)(uint(bitSize - 1)));
    if (!neg && un >= cutoff) {
        return (int64(cutoff - 1), error.As(rangeError(fnParseInt, s0))!);
    }
    if (neg && un > cutoff) {
        return (-int64(cutoff), error.As(rangeError(fnParseInt, s0))!);
    }
    var n = int64(un);
    if (neg) {
        n = -n;
    }
    return (n, error.As(null!)!);
}

// Atoi is equivalent to ParseInt(s, 10, 0), converted to type int.
public static (nint, error) Atoi(@string s) {
    nint _p0 = default;
    error _p0 = default!;

    const @string fnAtoi = "Atoi";



    var sLen = len(s);
    if (intSize == 32 && (0 < sLen && sLen < 10) || intSize == 64 && (0 < sLen && sLen < 19)) { 
        // Fast path for small integers that fit int type.
        var s0 = s;
        if (s[0] == '-' || s[0] == '+') {
            s = s[(int)1..];
            if (len(s) < 1) {
                return (0, error.As(addr(new NumError(fnAtoi,s0,ErrSyntax))!)!);
            }
        }
        nint n = 0;
        foreach (var (_, ch) in (slice<byte>)s) {
            ch -= '0';
            if (ch > 9) {
                return (0, error.As(addr(new NumError(fnAtoi,s0,ErrSyntax))!)!);
            }
            n = n * 10 + int(ch);
        }        if (s0[0] == '-') {
            n = -n;
        }
        return (n, error.As(null!)!);
    }
    var (i64, err) = ParseInt(s, 10, 0);
    {
        ptr<NumError> (nerr, ok) = err._<ptr<NumError>>();

        if (ok) {
            nerr.Func = fnAtoi;
        }
    }
    return (int(i64), error.As(err)!);
}

// underscoreOK reports whether the underscores in s are allowed.
// Checking them in this one function lets all the parsers skip over them simply.
// Underscore must appear only between digits or between a base prefix and a digit.
private static bool underscoreOK(@string s) { 
    // saw tracks the last character (class) we saw:
    // ^ for beginning of number,
    // 0 for a digit or base prefix,
    // _ for an underscore,
    // ! for none of the above.
    char saw = '^';
    nint i = 0; 

    // Optional sign.
    if (len(s) >= 1 && (s[0] == '-' || s[0] == '+')) {
        s = s[(int)1..];
    }
    var hex = false;
    if (len(s) >= 2 && s[0] == '0' && (lower(s[1]) == 'b' || lower(s[1]) == 'o' || lower(s[1]) == 'x')) {
        i = 2;
        saw = '0'; // base prefix counts as a digit for "underscore as digit separator"
        hex = lower(s[1]) == 'x';
    }
    while (i < len(s)) { 
        // Digits are always okay.
        if ('0' <= s[i] && s[i] <= '9' || hex && 'a' <= lower(s[i]) && lower(s[i]) <= 'f') {
            saw = '0';
            continue;
        i++;
        }
        if (s[i] == '_') {
            if (saw != '0') {
                return false;
            }
            saw = '_';
            continue;
        }
        if (saw == '_') {
            return false;
        }
        saw = '!';
    }
    return saw != '_';
}

} // end strconv_package
