// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2022 March 06 22:30:26 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Program Files\Go\src\strconv\atoc.go


namespace go;

public static partial class strconv_package {

private static readonly @string fnParseComplex = "ParseComplex";

// convErr splits an error returned by parseFloatPrefix
// into a syntax or range error for ParseComplex.


// convErr splits an error returned by parseFloatPrefix
// into a syntax or range error for ParseComplex.
private static (error, error) convErr(error err, @string s) {
    error syntax = default!;
    error range_ = default!;

    {
        ptr<NumError> (x, ok) = err._<ptr<NumError>>();

        if (ok) {
            x.Func = fnParseComplex;
            x.Num = s;
            if (x.Err == ErrRange) {
                return (error.As(null!)!, error.As(x)!);
            }
        }
    }

    return (error.As(err)!, error.As(null!)!);

}

// ParseComplex converts the string s to a complex number
// with the precision specified by bitSize: 64 for complex64, or 128 for complex128.
// When bitSize=64, the result still has type complex128, but it will be
// convertible to complex64 without changing its value.
//
// The number represented by s must be of the form N, Ni, or N±Ni, where N stands
// for a floating-point number as recognized by ParseFloat, and i is the imaginary
// component. If the second N is unsigned, a + sign is required between the two components
// as indicated by the ±. If the second N is NaN, only a + sign is accepted.
// The form may be parenthesized and cannot contain any spaces.
// The resulting complex number consists of the two components converted by ParseFloat.
//
// The errors that ParseComplex returns have concrete type *NumError
// and include err.Num = s.
//
// If s is not syntactically well-formed, ParseComplex returns err.Err = ErrSyntax.
//
// If s is syntactically well-formed but either component is more than 1/2 ULP
// away from the largest floating point number of the given component's size,
// ParseComplex returns err.Err = ErrRange and c = ±Inf for the respective component.
public static (System.Numerics.Complex128, error) ParseComplex(@string s, nint bitSize) {
    System.Numerics.Complex128 _p0 = default;
    error _p0 = default!;

    nint size = 64;
    if (bitSize == 64) {
        size = 32; // complex64 uses float32 parts
    }
    var orig = s; 

    // Remove parentheses, if any.
    if (len(s) >= 2 && s[0] == '(' && s[len(s) - 1] == ')') {
        s = s[(int)1..(int)len(s) - 1];
    }
    error pending = default!; // pending range error, or nil

    // Read real part (possibly imaginary part if followed by 'i').
    var (re, n, err) = parseFloatPrefix(s, size);
    if (err != null) {
        err, pending = convErr(err, orig);
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    s = s[(int)n..]; 

    // If we have nothing left, we're done.
    if (len(s) == 0) {
        return (complex(re, 0), error.As(pending)!);
    }

    if (s[0] == '+') 
    {
        // Consume the '+' to avoid an error if we have "+NaNi", but
        // do this only if we don't have a "++" (don't hide that error).
        if (len(s) > 1 && s[1] != '+') {
            s = s[(int)1..];
        }
        goto __switch_break0;
    }
    if (s[0] == '-')
    {
        goto __switch_break0;
    }
    if (s[0] == 'i') 
    {
        // If 'i' is the last character, we only have an imaginary part.
        if (len(s) == 1) {
            return (complex(0, re), error.As(pending)!);
        }
    }
    // default: 
        return (0, error.As(syntaxError(fnParseComplex, orig))!);

    __switch_break0:; 

    // Read imaginary part.
    var (im, n, err) = parseFloatPrefix(s, size);
    if (err != null) {
        err, pending = convErr(err, orig);
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    s = s[(int)n..];
    if (s != "i") {
        return (0, error.As(syntaxError(fnParseComplex, orig))!);
    }
    return (complex(re, im), error.As(pending)!);

}

} // end strconv_package
