// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file provides Go implementations of elementary multi-precision
// arithmetic operations on word vectors. These have the suffix _g.
// These are needed for platforms without assembly implementations of these routines.
// This file also contains elementary operations that can be implemented
// sufficiently efficiently in Go.

// package big -- go2cs converted at 2022 March 13 05:31:44 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\arith.go
namespace go.math;

using bits = math.bits_package;

public static partial class big_package {

// A Word represents a single digit of a multi-precision unsigned integer.
public partial struct Word { // : nuint
}

private static readonly var _S = _W / 8; // word size in bytes

private static readonly var _W = bits.UintSize; // word size in bits
private static readonly nint _B = 1 << (int)(_W); // digit base
private static readonly var _M = _B - 1; // digit mask

// Many of the loops in this file are of the form
//   for i := 0; i < len(z) && i < len(x) && i < len(y); i++
// i < len(z) is the real condition.
// However, checking i < len(x) && i < len(y) as well is faster than
// having the compiler do a bounds check in the body of the loop;
// remarkably it is even faster than hoisting the bounds check
// out of the loop, by doing something like
//   _, _ = x[len(z)-1], y[len(z)-1]
// There are other ways to hoist the bounds check out of the loop,
// but the compiler's BCE isn't powerful enough for them (yet?).
// See the discussion in CL 164966.

// ----------------------------------------------------------------------------
// Elementary operations on words
//
// These operations are used by the vector operations below.

// z1<<_W + z0 = x*y
private static (Word, Word) mulWW_g(Word x, Word y) {
    Word z1 = default;
    Word z0 = default;

    var (hi, lo) = bits.Mul(uint(x), uint(y));
    return (Word(hi), Word(lo));
}

// z1<<_W + z0 = x*y + c
private static (Word, Word) mulAddWWW_g(Word x, Word y, Word c) {
    Word z1 = default;
    Word z0 = default;

    var (hi, lo) = bits.Mul(uint(x), uint(y));
    nuint cc = default;
    lo, cc = bits.Add(lo, uint(c), 0);
    return (Word(hi + cc), Word(lo));
}

// nlz returns the number of leading zeros in x.
// Wraps bits.LeadingZeros call for convenience.
private static nuint nlz(Word x) {
    return uint(bits.LeadingZeros(uint(x)));
}

// The resulting carry c is either 0 or 1.
private static Word addVV_g(slice<Word> z, slice<Word> x, slice<Word> y) {
    Word c = default;
 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x) && i < len(y); i++) {
        var (zi, cc) = bits.Add(uint(x[i]), uint(y[i]), uint(c));
        z[i] = Word(zi);
        c = Word(cc);
    }
    return ;
}

// The resulting carry c is either 0 or 1.
private static Word subVV_g(slice<Word> z, slice<Word> x, slice<Word> y) {
    Word c = default;
 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x) && i < len(y); i++) {
        var (zi, cc) = bits.Sub(uint(x[i]), uint(y[i]), uint(c));
        z[i] = Word(zi);
        c = Word(cc);
    }
    return ;
}

// The resulting carry c is either 0 or 1.
private static Word addVW_g(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;

    c = y; 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        var (zi, cc) = bits.Add(uint(x[i]), uint(c), 0);
        z[i] = Word(zi);
        c = Word(cc);
    }
    return ;
}

// addVWlarge is addVW, but intended for large z.
// The only difference is that we check on every iteration
// whether we are done with carries,
// and if so, switch to a much faster copy instead.
// This is only a good idea for large z,
// because the overhead of the check and the function call
// outweigh the benefits when z is small.
private static Word addVWlarge(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;

    c = y; 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        if (c == 0) {
            copy(z[(int)i..], x[(int)i..]);
            return ;
        }
        var (zi, cc) = bits.Add(uint(x[i]), uint(c), 0);
        z[i] = Word(zi);
        c = Word(cc);
    }
    return ;
}

private static Word subVW_g(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;

    c = y; 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        var (zi, cc) = bits.Sub(uint(x[i]), uint(c), 0);
        z[i] = Word(zi);
        c = Word(cc);
    }
    return ;
}

// subVWlarge is to subVW as addVWlarge is to addVW.
private static Word subVWlarge(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;

    c = y; 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        if (c == 0) {
            copy(z[(int)i..], x[(int)i..]);
            return ;
        }
        var (zi, cc) = bits.Sub(uint(x[i]), uint(c), 0);
        z[i] = Word(zi);
        c = Word(cc);
    }
    return ;
}

private static Word shlVU_g(slice<Word> z, slice<Word> x, nuint s) {
    Word c = default;

    if (s == 0) {
        copy(z, x);
        return ;
    }
    if (len(z) == 0) {
        return ;
    }
    s &= _W - 1; // hint to the compiler that shifts by s don't need guard code
    var ŝ = _W - s;
    ŝ &= _W - 1; // ditto
    c = x[len(z) - 1] >> (int)(ŝ);
    for (var i = len(z) - 1; i > 0; i--) {
        z[i] = x[i] << (int)(s) | x[i - 1] >> (int)(ŝ);
    }
    z[0] = x[0] << (int)(s);
    return ;
}

private static Word shrVU_g(slice<Word> z, slice<Word> x, nuint s) => func((_, panic, _) => {
    Word c = default;

    if (s == 0) {
        copy(z, x);
        return ;
    }
    if (len(z) == 0) {
        return ;
    }
    if (len(x) != len(z)) { 
        // This is an invariant guaranteed by the caller.
        panic("len(x) != len(z)");
    }
    s &= _W - 1; // hint to the compiler that shifts by s don't need guard code
    var ŝ = _W - s;
    ŝ &= _W - 1; // ditto
    c = x[0] << (int)(ŝ);
    for (nint i = 1; i < len(z); i++) {
        z[i - 1] = x[i - 1] >> (int)(s) | x[i] << (int)(ŝ);
    }
    z[len(z) - 1] = x[len(z) - 1] >> (int)(s);
    return ;
});

private static Word mulAddVWW_g(slice<Word> z, slice<Word> x, Word y, Word r) {
    Word c = default;

    c = r; 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        c, z[i] = mulAddWWW_g(x[i], y, c);
    }
    return ;
}

private static Word addMulVVW_g(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;
 
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        var (z1, z0) = mulAddWWW_g(x[i], y, z[i]);
        var (lo, cc) = bits.Add(uint(z0), uint(c), 0);
        (c, z[i]) = (Word(cc), Word(lo));        c += z1;
    }
    return ;
}

// q = ( x1 << _W + x0 - r)/y. m = floor(( _B^2 - 1 ) / d - _B). Requiring x1<y.
// An approximate reciprocal with a reference to "Improved Division by Invariant Integers
// (IEEE Transactions on Computers, 11 Jun. 2010)"
private static (Word, Word) divWW(Word x1, Word x0, Word y, Word m) {
    Word q = default;
    Word r = default;

    var s = nlz(y);
    if (s != 0) {
        x1 = x1 << (int)(s) | x0 >> (int)((_W - s));
        x0<<=s;
        y<<=s;
    }
    var d = uint(y); 
    // We know that
    //   m = ⎣(B^2-1)/d⎦-B
    //   ⎣(B^2-1)/d⎦ = m+B
    //   (B^2-1)/d = m+B+delta1    0 <= delta1 <= (d-1)/d
    //   B^2/d = m+B+delta2        0 <= delta2 <= 1
    // The quotient we're trying to compute is
    //   quotient = ⎣(x1*B+x0)/d⎦
    //            = ⎣(x1*B*(B^2/d)+x0*(B^2/d))/B^2⎦
    //            = ⎣(x1*B*(m+B+delta2)+x0*(m+B+delta2))/B^2⎦
    //            = ⎣(x1*m+x1*B+x0)/B + x0*m/B^2 + delta2*(x1*B+x0)/B^2⎦
    // The latter two terms of this three-term sum are between 0 and 1.
    // So we can compute just the first term, and we will be low by at most 2.
    var (t1, t0) = bits.Mul(uint(m), uint(x1));
    var (_, c) = bits.Add(t0, uint(x0), 0);
    t1, _ = bits.Add(t1, uint(x1), c); 
    // The quotient is either t1, t1+1, or t1+2.
    // We'll try t1 and adjust if needed.
    var qq = t1; 
    // compute remainder r=x-d*q.
    var (dq1, dq0) = bits.Mul(d, qq);
    var (r0, b) = bits.Sub(uint(x0), dq0, 0);
    var (r1, _) = bits.Sub(uint(x1), dq1, b); 
    // The remainder we just computed is bounded above by B+d:
    // r = x1*B + x0 - d*q.
    //   = x1*B + x0 - d*⎣(x1*m+x1*B+x0)/B⎦
    //   = x1*B + x0 - d*((x1*m+x1*B+x0)/B-alpha)                                   0 <= alpha < 1
    //   = x1*B + x0 - x1*d/B*m                         - x1*d - x0*d/B + d*alpha
    //   = x1*B + x0 - x1*d/B*⎣(B^2-1)/d-B⎦             - x1*d - x0*d/B + d*alpha
    //   = x1*B + x0 - x1*d/B*⎣(B^2-1)/d-B⎦             - x1*d - x0*d/B + d*alpha
    //   = x1*B + x0 - x1*d/B*((B^2-1)/d-B-beta)        - x1*d - x0*d/B + d*alpha   0 <= beta < 1
    //   = x1*B + x0 - x1*B + x1/B + x1*d + x1*d/B*beta - x1*d - x0*d/B + d*alpha
    //   =        x0        + x1/B        + x1*d/B*beta        - x0*d/B + d*alpha
    //   = x0*(1-d/B) + x1*(1+d*beta)/B + d*alpha
    //   <  B*(1-d/B) +  d*B/B          + d          because x0<B (and 1-d/B>0), x1<d, 1+d*beta<=B, alpha<1
    //   =  B - d     +  d              + d
    //   = B+d
    // So r1 can only be 0 or 1. If r1 is 1, then we know q was too small.
    // Add 1 to q and subtract d from r. That guarantees that r is <B, so
    // we no longer need to keep track of r1.
    if (r1 != 0) {
        qq++;
        r0 -= d;
    }
    if (r0 >= d) {
        qq++;
        r0 -= d;
    }
    return (Word(qq), Word(r0 >> (int)(s)));
}

// reciprocalWord return the reciprocal of the divisor. rec = floor(( _B^2 - 1 ) / u - _B). u = d1 << nlz(d1).
private static Word reciprocalWord(Word d1) {
    var u = uint(d1 << (int)(nlz(d1)));
    var x1 = ~u;
    var x0 = uint(_M);
    var (rec, _) = bits.Div(x1, x0, u); // (_B^2-1)/U-_B = (_B*(_M-C)+_M)/U
    return Word(rec);
}

} // end big_package
