// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements multi-precision decimal numbers.
// The implementation is for float to decimal conversion only;
// not general purpose use.
// The only operations are precise conversion from binary to
// decimal and rounding.
//
// The key observation and some code (shr) is borrowed from
// strconv/decimal.go: conversion of binary fractional values can be done
// precisely in multi-precision decimal because 2 divides 10 (required for
// >> of mantissa); but conversion of decimal floating-point values cannot
// be done precisely in binary representation.
//
// In contrast to strconv/decimal.go, only right shift is implemented in
// decimal format - left shift can be done precisely in binary format.

// package big -- go2cs converted at 2022 March 06 22:17:37 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\decimal.go


namespace go.math;

public static partial class big_package {

    // A decimal represents an unsigned floating-point number in decimal representation.
    // The value of a non-zero decimal d is d.mant * 10**d.exp with 0.1 <= d.mant < 1,
    // with the most-significant mantissa digit at index 0. For the zero decimal, the
    // mantissa length and exponent are 0.
    // The zero value for decimal represents a ready-to-use 0.0.
private partial struct @decimal {
    public slice<byte> mant; // mantissa ASCII digits, big-endian
    public nint exp; // exponent
}

// at returns the i'th mantissa digit, starting with the most significant digit at 0.
private static byte at(this ptr<decimal> _addr_d, nint i) {
    ref decimal d = ref _addr_d.val;

    if (0 <= i && i < len(d.mant)) {
        return d.mant[i];
    }
    return '0';

}

// Maximum shift amount that can be done in one pass without overflow.
// A Word has _W bits and (1<<maxShift - 1)*10 + 9 must fit into Word.
private static readonly var maxShift = _W - 4;

// TODO(gri) Since we know the desired decimal precision when converting
// a floating-point number, we may be able to limit the number of decimal
// digits that need to be computed by init by providing an additional
// precision argument and keeping track of when a number was truncated early
// (equivalent of "sticky bit" in binary rounding).

// TODO(gri) Along the same lines, enforce some limit to shift magnitudes
// to avoid "infinitely" long running conversions (until we run out of space).

// Init initializes x to the decimal representation of m << shift (for
// shift >= 0), or m >> -shift (for shift < 0).


// TODO(gri) Since we know the desired decimal precision when converting
// a floating-point number, we may be able to limit the number of decimal
// digits that need to be computed by init by providing an additional
// precision argument and keeping track of when a number was truncated early
// (equivalent of "sticky bit" in binary rounding).

// TODO(gri) Along the same lines, enforce some limit to shift magnitudes
// to avoid "infinitely" long running conversions (until we run out of space).

// Init initializes x to the decimal representation of m << shift (for
// shift >= 0), or m >> -shift (for shift < 0).
private static void init(this ptr<decimal> _addr_x, nat m, nint shift) {
    ref decimal x = ref _addr_x.val;
 
    // special case 0
    if (len(m) == 0) {
        x.mant = x.mant[..(int)0];
        x.exp = 0;
        return ;
    }
    if (shift < 0) {
        var ntz = m.trailingZeroBits();
        var s = uint(-shift);
        if (s >= ntz) {
            s = ntz; // shift at most ntz bits
        }
        m = nat(null).shr(m, s);
        shift += int(s);

    }
    if (shift > 0) {
        m = nat(null).shl(m, uint(shift));
        shift = 0;
    }
    s = m.utoa(10);
    var n = len(s);
    x.exp = n; 
    // Trim trailing zeros; instead the exponent is tracking
    // the decimal point independent of the number of digits.
    while (n > 0 && s[n - 1] == '0') {
        n--;
    }
    x.mant = append(x.mant[..(int)0], s[..(int)n]); 

    // Do any (remaining) shift right in decimal representation.
    if (shift < 0) {
        while (shift < -maxShift) {
            shr(_addr_x, maxShift);
            shift += maxShift;
        }
        shr(_addr_x, uint(-shift));
    }
}

// shr implements x >> s, for s <= maxShift.
private static void shr(ptr<decimal> _addr_x, nuint s) {
    ref decimal x = ref _addr_x.val;
 
    // Division by 1<<s using shift-and-subtract algorithm.

    // pick up enough leading digits to cover first shift
    nint r = 0; // read index
    Word n = default;
    while (n >> (int)(s) == 0 && r < len(x.mant)) {
        var ch = Word(x.mant[r]);
        r++;
        n = n * 10 + ch - '0';
    }
    if (n == 0) { 
        // x == 0; shouldn't get here, but handle anyway
        x.mant = x.mant[..(int)0];
        return ;

    }
    while (n >> (int)(s) == 0) {
        r++;
        n *= 10;
    }
    x.exp += 1 - r; 

    // read a digit, write a digit
    nint w = 0; // write index
    var mask = Word(1) << (int)(s) - 1;
    while (r < len(x.mant)) {
        ch = Word(x.mant[r]);
        r++;
        var d = n >> (int)(s);
        n &= mask; // n -= d << s
        x.mant[w] = byte(d + '0');
        w++;
        n = n * 10 + ch - '0';

    } 

    // write extra digits that still fit
    while (n > 0 && w < len(x.mant)) {
        d = n >> (int)(s);
        n &= mask;
        x.mant[w] = byte(d + '0');
        w++;
        n = n * 10;
    }
    x.mant = x.mant[..(int)w]; // the number may be shorter (e.g. 1024 >> 10)

    // append additional digits that didn't fit
    while (n > 0) {
        d = n >> (int)(s);
        n &= mask;
        x.mant = append(x.mant, byte(d + '0'));
        n = n * 10;
    }

    trim(_addr_x);

}

private static @string String(this ptr<decimal> _addr_x) {
    ref decimal x = ref _addr_x.val;

    if (len(x.mant) == 0) {
        return "0";
    }
    slice<byte> buf = default;

    if (x.exp <= 0) 
        // 0.00ddd
        buf = make_slice<byte>(0, 2 + (-x.exp) + len(x.mant));
        buf = append(buf, "0.");
        buf = appendZeros(buf, -x.exp);
        buf = append(buf, x.mant);
    else if (x.exp < len(x.mant)) 
        // dd.ddd
        buf = make_slice<byte>(0, 1 + len(x.mant));
        buf = append(buf, x.mant[..(int)x.exp]);
        buf = append(buf, '.');
        buf = append(buf, x.mant[(int)x.exp..]);
    else // len(x.mant) <= x.exp
        // ddd00
        buf = make_slice<byte>(0, x.exp);
        buf = append(buf, x.mant);
        buf = appendZeros(buf, x.exp - len(x.mant));
        return string(buf);

}

// appendZeros appends n 0 digits to buf and returns buf.
private static slice<byte> appendZeros(slice<byte> buf, nint n) {
    while (n > 0) {
        buf = append(buf, '0');
        n--;
    }
    return buf;

}

// shouldRoundUp reports if x should be rounded up
// if shortened to n digits. n must be a valid index
// for x.mant.
private static bool shouldRoundUp(ptr<decimal> _addr_x, nint n) {
    ref decimal x = ref _addr_x.val;

    if (x.mant[n] == '5' && n + 1 == len(x.mant)) { 
        // exactly halfway - round to even
        return n > 0 && (x.mant[n - 1] - '0') & 1 != 0;

    }
    return x.mant[n] >= '5';

}

// round sets x to (at most) n mantissa digits by rounding it
// to the nearest even value with n (or fever) mantissa digits.
// If n < 0, x remains unchanged.
private static void round(this ptr<decimal> _addr_x, nint n) {
    ref decimal x = ref _addr_x.val;

    if (n < 0 || n >= len(x.mant)) {
        return ; // nothing to do
    }
    if (shouldRoundUp(_addr_x, n)) {
        x.roundUp(n);
    }
    else
 {
        x.roundDown(n);
    }
}

private static void roundUp(this ptr<decimal> _addr_x, nint n) {
    ref decimal x = ref _addr_x.val;

    if (n < 0 || n >= len(x.mant)) {
        return ; // nothing to do
    }
    while (n > 0 && x.mant[n - 1] >= '9') {
        n--;
    }

    if (n == 0) { 
        // all digits are '9's => round up to '1' and update exponent
        x.mant[0] = '1'; // ok since len(x.mant) > n
        x.mant = x.mant[..(int)1];
        x.exp++;
        return ;

    }
    x.mant[n - 1]++;
    x.mant = x.mant[..(int)n]; 
    // x already trimmed
}

private static void roundDown(this ptr<decimal> _addr_x, nint n) {
    ref decimal x = ref _addr_x.val;

    if (n < 0 || n >= len(x.mant)) {
        return ; // nothing to do
    }
    x.mant = x.mant[..(int)n];
    trim(_addr_x);

}

// trim cuts off any trailing zeros from x's mantissa;
// they are meaningless for the value of x.
private static void trim(ptr<decimal> _addr_x) {
    ref decimal x = ref _addr_x.val;

    var i = len(x.mant);
    while (i > 0 && x.mant[i - 1] == '0') {
        i--;
    }
    x.mant = x.mant[..(int)i];
    if (i == 0) {
        x.exp = 0;
    }
}

} // end big_package
