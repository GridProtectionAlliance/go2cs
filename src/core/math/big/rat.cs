// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements multi-precision rational numbers.
namespace go.math;

using fmt = fmt_package;
using math = math_package;

partial class big_package {

// A Rat represents a quotient a/b of arbitrary precision.
// The zero value for a Rat represents the value 0.
//
// Operations always take pointer arguments (*Rat) rather
// than Rat values, and each unique Rat value requires
// its own unique *Rat pointer. To "copy" a Rat value,
// an existing (or newly allocated) Rat must be set to
// a new value using the [Rat.Set] method; shallow copies
// of Rats are not supported and may lead to errors.
[GoType] partial struct ΔRat {
    // To make zero values for Rat work w/o initialization,
    // a zero value of b (len(b) == 0) acts like b == 1. At
    // the earliest opportunity (when an assignment to the Rat
    // is made), such uninitialized denominators are set to 1.
    // a.neg determines the sign of the Rat, b.neg is ignored.
    internal ΔInt a;
    internal ΔInt b;
}

// NewRat creates a new [Rat] with numerator a and denominator b.
public static ж<ΔRat> NewRat(int64 a, int64 b) {
    return @new<ΔRat>().SetFrac64(a, b);
}

// SetFloat64 sets z to exactly f and returns z.
// If f is not finite, SetFloat returns nil.
[GoRecv] public static ж<ΔRat> SetFloat64(this ref ΔRat z, float64 f) {
    static readonly UntypedInt expMask = /* 1<<11 - 1 */ 2047;
    var bits = math.Float64bits(f);
    var mantissa = (uint64)(bits & (1 << (int)(52) - 1));
    nint exp = ((nint)((uint64)((bits >> (int)(52)) & expMask)));
    switch (exp) {
    case expMask: {
        return default!;
    }
    case 0: {
        exp -= 1022;
        break;
    }
    default: {
        mantissa |= (uint64)(1 << (int)(52));
        exp -= 1023;
        break;
    }}

    // non-finite
    // denormal
    // normal
    nint shift = 52 - exp;
    // Optimization (?): partially pre-normalise.
    while ((uint64)(mantissa & 1) == 0 && shift > 0) {
        mantissa >>= (UntypedInt)(1);
        shift--;
    }
    z.a.SetUint64(mantissa);
    z.a.neg = f < 0;
    z.b.Set(intOne);
    if (shift > 0){
        z.b.Lsh(Ꮡ(z.b), ((nuint)shift));
    } else {
        z.a.Lsh(Ꮡ(z.a), ((nuint)(-shift)));
    }
    return z.norm();
}

// quotToFloat32 returns the non-negative float32 value
// nearest to the quotient a/b, using round-to-even in
// halfway cases. It does not mutate its arguments.
// Preconditions: b is non-zero; a and b have no common factors.
internal static (float32 f, bool exact) quotToFloat32(nat a, nat b) {
    float32 f = default!;
    bool exact = default!;

    static readonly UntypedInt Fsize = 32;
    static readonly UntypedInt Msize = 23;
    static readonly UntypedInt Msize1 = /* Msize + 1 */ 24; // incl. implicit 1
    static readonly UntypedInt Msize2 = /* Msize1 + 1 */ 25;
    static readonly UntypedInt Esize = /* Fsize - Msize1 */ 8;
    static readonly UntypedInt Ebias = /* 1<<(Esize-1) - 1 */ 127;
    static readonly UntypedInt Emin = /* 1 - Ebias */ -126;
    static readonly UntypedInt Emax = /* Ebias */ 127;
    // TODO(adonovan): specialize common degenerate cases: 1.0, integers.
    nint alen = a.bitLen();
    if (alen == 0) {
        return (0, true);
    }
    nint blen = b.bitLen();
    if (blen == 0) {
        throw panic("division by zero");
    }
    // 1. Left-shift A or B such that quotient A/B is in [1<<Msize1, 1<<(Msize2+1)
    // (Msize2 bits if A < B when they are left-aligned, Msize2+1 bits if A >= B).
    // This is 2 or 3 more than the float32 mantissa field width of Msize:
    // - the optional extra bit is shifted away in step 3 below.
    // - the high-order 1 is omitted in "normal" representation;
    // - the low-order 1 will be used during rounding then discarded.
    nint exp = alen - blen;
    nat a2 = default!;
    nat b2 = default!;
    a2 = a2.set(a);
    b2 = b2.set(b);
    {
        nint shift = Msize2 - exp; if (shift > 0){
            a2 = a2.shl(a2, ((nuint)shift));
        } else 
        if (shift < 0) {
            b2 = b2.shl(b2, ((nuint)(-shift)));
        }
    }
    // 2. Compute quotient and remainder (q, r).  NB: due to the
    // extra shift, the low-order bit of q is logically the
    // high-order bit of r.
    nat q = default!;
    (q, r) = q.div(a2, a2, b2);
    // (recycle a2)
    var mantissa = low32(q);
    var haveRem = len(r) > 0;
    // mantissa&1 && !haveRem => remainder is exactly half
    // 3. If quotient didn't fit in Msize2 bits, redo division by b2<<1
    // (in effect---we accomplish this incrementally).
    if (mantissa >> (int)(Msize2) == 1) {
        if ((uint32)(mantissa & 1) == 1) {
            haveRem = true;
        }
        mantissa >>= (UntypedInt)(1);
        exp++;
    }
    if (mantissa >> (int)(Msize1) != 1) {
        throw panic(fmt.Sprintf("expected exactly %d bits of result"u8, Msize2));
    }
    // 4. Rounding.
    if (Emin - Msize <= exp && exp <= Emin) {
        // Denormal case; lose 'shift' bits of precision.
        nuint shift = ((nuint)(Emin - (exp - 1)));
        // [1..Esize1)
        var lostbits = (uint32)(mantissa & (1 << (int)(shift) - 1));
        haveRem = haveRem || lostbits != 0;
        mantissa >>= (nuint)(shift);
        exp = 2 - Ebias;
    }
    // == exp + shift
    // Round q using round-half-to-even.
    exact = !haveRem;
    if ((uint32)(mantissa & 1) != 0) {
        exact = false;
        if (haveRem || (uint32)(mantissa & 2) != 0) {
            {
                mantissa++; if (mantissa >= 1 << (int)(Msize2)) {
                    // Complete rollover 11...1 => 100...0, so shift is safe
                    mantissa >>= (UntypedInt)(1);
                    exp++;
                }
            }
        }
    }
    mantissa >>= (UntypedInt)(1);
    // discard rounding bit.  Mantissa now scaled by 1<<Msize1.
    f = ((float32)math.Ldexp(((float64)mantissa), exp - Msize1));
    if (math.IsInf(((float64)f), 0)) {
        exact = false;
    }
    return (f, exact);
}

// quotToFloat64 returns the non-negative float64 value
// nearest to the quotient a/b, using round-to-even in
// halfway cases. It does not mutate its arguments.
// Preconditions: b is non-zero; a and b have no common factors.
internal static (float64 f, bool exact) quotToFloat64(nat a, nat b) {
    float64 f = default!;
    bool exact = default!;

    static readonly UntypedInt Fsize = 64;
    static readonly UntypedInt Msize = 52;
    static readonly UntypedInt Msize1 = /* Msize + 1 */ 53; // incl. implicit 1
    static readonly UntypedInt Msize2 = /* Msize1 + 1 */ 54;
    static readonly UntypedInt Esize = /* Fsize - Msize1 */ 11;
    static readonly UntypedInt Ebias = /* 1<<(Esize-1) - 1 */ 1023;
    static readonly UntypedInt Emin = /* 1 - Ebias */ -1022;
    static readonly UntypedInt Emax = /* Ebias */ 1023;
    // TODO(adonovan): specialize common degenerate cases: 1.0, integers.
    nint alen = a.bitLen();
    if (alen == 0) {
        return (0, true);
    }
    nint blen = b.bitLen();
    if (blen == 0) {
        throw panic("division by zero");
    }
    // 1. Left-shift A or B such that quotient A/B is in [1<<Msize1, 1<<(Msize2+1)
    // (Msize2 bits if A < B when they are left-aligned, Msize2+1 bits if A >= B).
    // This is 2 or 3 more than the float64 mantissa field width of Msize:
    // - the optional extra bit is shifted away in step 3 below.
    // - the high-order 1 is omitted in "normal" representation;
    // - the low-order 1 will be used during rounding then discarded.
    nint exp = alen - blen;
    nat a2 = default!;
    nat b2 = default!;
    a2 = a2.set(a);
    b2 = b2.set(b);
    {
        nint shift = Msize2 - exp; if (shift > 0){
            a2 = a2.shl(a2, ((nuint)shift));
        } else 
        if (shift < 0) {
            b2 = b2.shl(b2, ((nuint)(-shift)));
        }
    }
    // 2. Compute quotient and remainder (q, r).  NB: due to the
    // extra shift, the low-order bit of q is logically the
    // high-order bit of r.
    nat q = default!;
    (q, r) = q.div(a2, a2, b2);
    // (recycle a2)
    var mantissa = low64(q);
    var haveRem = len(r) > 0;
    // mantissa&1 && !haveRem => remainder is exactly half
    // 3. If quotient didn't fit in Msize2 bits, redo division by b2<<1
    // (in effect---we accomplish this incrementally).
    if (mantissa >> (int)(Msize2) == 1) {
        if ((uint64)(mantissa & 1) == 1) {
            haveRem = true;
        }
        mantissa >>= (UntypedInt)(1);
        exp++;
    }
    if (mantissa >> (int)(Msize1) != 1) {
        throw panic(fmt.Sprintf("expected exactly %d bits of result"u8, Msize2));
    }
    // 4. Rounding.
    if (Emin - Msize <= exp && exp <= Emin) {
        // Denormal case; lose 'shift' bits of precision.
        nuint shift = ((nuint)(Emin - (exp - 1)));
        // [1..Esize1)
        var lostbits = (uint64)(mantissa & (1 << (int)(shift) - 1));
        haveRem = haveRem || lostbits != 0;
        mantissa >>= (nuint)(shift);
        exp = 2 - Ebias;
    }
    // == exp + shift
    // Round q using round-half-to-even.
    exact = !haveRem;
    if ((uint64)(mantissa & 1) != 0) {
        exact = false;
        if (haveRem || (uint64)(mantissa & 2) != 0) {
            {
                mantissa++; if (mantissa >= 1 << (int)(Msize2)) {
                    // Complete rollover 11...1 => 100...0, so shift is safe
                    mantissa >>= (UntypedInt)(1);
                    exp++;
                }
            }
        }
    }
    mantissa >>= (UntypedInt)(1);
    // discard rounding bit.  Mantissa now scaled by 1<<Msize1.
    f = math.Ldexp(((float64)mantissa), exp - Msize1);
    if (math.IsInf(f, 0)) {
        exact = false;
    }
    return (f, exact);
}

// Float32 returns the nearest float32 value for x and a bool indicating
// whether f represents x exactly. If the magnitude of x is too large to
// be represented by a float32, f is an infinity and exact is false.
// The sign of f always matches the sign of x, even if f == 0.
[GoRecv] public static (float32 f, bool exact) Float32(this ref ΔRat x) {
    float32 f = default!;
    bool exact = default!;

    var b = x.b.abs;
    if (len(b) == 0) {
        b = natOne;
    }
    (f, exact) = quotToFloat32(x.a.abs, b);
    if (x.a.neg) {
        f = -f;
    }
    return (f, exact);
}

// Float64 returns the nearest float64 value for x and a bool indicating
// whether f represents x exactly. If the magnitude of x is too large to
// be represented by a float64, f is an infinity and exact is false.
// The sign of f always matches the sign of x, even if f == 0.
[GoRecv] public static (float64 f, bool exact) Float64(this ref ΔRat x) {
    float64 f = default!;
    bool exact = default!;

    var b = x.b.abs;
    if (len(b) == 0) {
        b = natOne;
    }
    (f, exact) = quotToFloat64(x.a.abs, b);
    if (x.a.neg) {
        f = -f;
    }
    return (f, exact);
}

// SetFrac sets z to a/b and returns z.
// If b == 0, SetFrac panics.
[GoRecv] public static ж<ΔRat> SetFrac(this ref ΔRat z, ж<ΔInt> Ꮡa, ж<ΔInt> Ꮡb) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;

    z.a.neg = a.neg != b.neg;
    var babs = b.abs;
    if (len(babs) == 0) {
        throw panic("division by zero");
    }
    if (Ꮡ(z.a) == Ꮡb || alias(z.a.abs, babs)) {
        babs = ((nat)default!).set(babs);
    }
    // make a copy
    z.a.abs = z.a.abs.set(a.abs);
    z.b.abs = z.b.abs.set(babs);
    return z.norm();
}

// SetFrac64 sets z to a/b and returns z.
// If b == 0, SetFrac64 panics.
[GoRecv] public static ж<ΔRat> SetFrac64(this ref ΔRat z, int64 a, int64 b) {
    if (b == 0) {
        throw panic("division by zero");
    }
    z.a.SetInt64(a);
    if (b < 0) {
        b = -b;
        z.a.neg = !z.a.neg;
    }
    z.b.abs = z.b.abs.setUint64(((uint64)b));
    return z.norm();
}

// SetInt sets z to x (by making a copy of x) and returns z.
[GoRecv("capture")] public static ж<ΔRat> SetInt(this ref ΔRat z, ж<ΔInt> Ꮡx) {
    ref var x = ref Ꮡx.val;

    z.a.Set(Ꮡx);
    z.b.abs = z.b.abs.setWord(1);
    return SetIntꓸᏑz;
}

// SetInt64 sets z to x and returns z.
[GoRecv("capture")] public static ж<ΔRat> SetInt64(this ref ΔRat z, int64 x) {
    z.a.SetInt64(x);
    z.b.abs = z.b.abs.setWord(1);
    return SetInt64ꓸᏑz;
}

// SetUint64 sets z to x and returns z.
[GoRecv("capture")] public static ж<ΔRat> SetUint64(this ref ΔRat z, uint64 x) {
    z.a.SetUint64(x);
    z.b.abs = z.b.abs.setWord(1);
    return SetUint64ꓸᏑz;
}

// Set sets z to x (by making a copy of x) and returns z.
[GoRecv("capture")] public static ж<ΔRat> Set(this ref ΔRat z, ж<ΔRat> Ꮡx) {
    ref var x = ref Ꮡx.val;

    if (z != Ꮡx) {
        z.a.Set(Ꮡ(x.a));
        z.b.Set(Ꮡ(x.b));
    }
    if (len(z.b.abs) == 0) {
        z.b.abs = z.b.abs.setWord(1);
    }
    return SetꓸᏑz;
}

// Abs sets z to |x| (the absolute value of x) and returns z.
[GoRecv("capture")] public static ж<ΔRat> Abs(this ref ΔRat z, ж<ΔRat> Ꮡx) {
    ref var x = ref Ꮡx.val;

    z.Set(Ꮡx);
    z.a.neg = false;
    return AbsꓸᏑz;
}

// Neg sets z to -x and returns z.
[GoRecv("capture")] public static ж<ΔRat> Neg(this ref ΔRat z, ж<ΔRat> Ꮡx) {
    ref var x = ref Ꮡx.val;

    z.Set(Ꮡx);
    z.a.neg = len(z.a.abs) > 0 && !z.a.neg;
    // 0 has no sign
    return NegꓸᏑz;
}

// Inv sets z to 1/x and returns z.
// If x == 0, Inv panics.
[GoRecv("capture")] public static ж<ΔRat> Inv(this ref ΔRat z, ж<ΔRat> Ꮡx) {
    ref var x = ref Ꮡx.val;

    if (len(x.a.abs) == 0) {
        throw panic("division by zero");
    }
    z.Set(Ꮡx);
    (z.a.abs, z.b.abs) = (z.b.abs, z.a.abs);
    return InvꓸᏑz;
}

// Sign returns:
//   - -1 if x < 0;
//   - 0 if x == 0;
//   - +1 if x > 0.
[GoRecv] public static nint Sign(this ref ΔRat x) {
    return x.a.Sign();
}

// IsInt reports whether the denominator of x is 1.
[GoRecv] public static bool IsInt(this ref ΔRat x) {
    return len(x.b.abs) == 0 || x.b.abs.cmp(natOne) == 0;
}

// Num returns the numerator of x; it may be <= 0.
// The result is a reference to x's numerator; it
// may change if a new value is assigned to x, and vice versa.
// The sign of the numerator corresponds to the sign of x.
[GoRecv] public static ж<ΔInt> Num(this ref ΔRat x) {
    return Ꮡ(x.a);
}

// Denom returns the denominator of x; it is always > 0.
// The result is a reference to x's denominator, unless
// x is an uninitialized (zero value) [Rat], in which case
// the result is a new [Int] of value 1. (To initialize x,
// any operation that sets x will do, including x.Set(x).)
// If the result is a reference to x's denominator it
// may change if a new value is assigned to x, and vice versa.
[GoRecv] public static ж<ΔInt> Denom(this ref ΔRat x) {
    // Note that x.b.neg is guaranteed false.
    if (len(x.b.abs) == 0) {
        // Note: If this proves problematic, we could
        //       panic instead and require the Rat to
        //       be explicitly initialized.
        return Ꮡ(new ΔInt(abs: new nat{1}));
    }
    return Ꮡ(x.b);
}

[GoRecv("capture")] internal static ж<ΔRat> norm(this ref ΔRat z) {
    var matchᴛ1 = false;
    if (len(z.a.abs) is 0) { matchᴛ1 = true;
        z.a.neg = false;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && len(z.b.abs) is 0) {
        z.b.abs = z.b.abs.setWord(1);
    }
    else { /* default: */
        var neg = z.a.neg;
        z.a.neg = false;
        z.b.neg = false;
        {
            var f = NewInt(0).lehmerGCD(nil, // z == 0; normalize sign and denominator
 // z is integer; normalize denominator
 // z is fraction; normalize numerator and denominator
 nil, Ꮡ(z.a), Ꮡ(z.b)); if (f.Cmp(intOne) != 0) {
                (z.a.abs, Δ_) = z.a.abs.div(default!, z.a.abs, (~f).abs);
                (z.b.abs, Δ_) = z.b.abs.div(default!, z.b.abs, (~f).abs);
            }
        }
        z.a.neg = neg;
    }

    return normꓸᏑz;
}

// mulDenom sets z to the denominator product x*y (by taking into
// account that 0 values for x or y must be interpreted as 1) and
// returns z.
internal static nat mulDenom(nat z, nat x, nat y) {
    switch (ᐧ) {
    case {} when len(x) == 0 && len(y) == 0: {
        return z.setWord(1);
    }
    case {} when len(x) is 0: {
        return z.set(y);
    }
    case {} when len(y) is 0: {
        return z.set(x);
    }}

    return z.mul(x, y);
}

// scaleDenom sets z to the product x*f.
// If f == 0 (zero value of denominator), z is set to (a copy of) x.
[GoRecv] public static void scaleDenom(this ref ΔInt z, ж<ΔInt> Ꮡx, nat f) {
    ref var x = ref Ꮡx.val;

    if (len(f) == 0) {
        z.Set(Ꮡx);
        return;
    }
    z.abs = z.abs.mul(x.abs, f);
    z.neg = x.neg;
}

// Cmp compares x and y and returns:
//   - -1 if x < y;
//   - 0 if x == y;
//   - +1 if x > y.
[GoRecv] public static nint Cmp(this ref ΔRat x, ж<ΔRat> Ꮡy) {
    ref var y = ref Ꮡy.val;

    ΔInt a = default!;
    ref var b = ref heap(new ΔInt(), out var Ꮡb);
    a.scaleDenom(Ꮡ(x.a), y.b.abs);
    b.scaleDenom(Ꮡ(y.a), x.b.abs);
    return a.Cmp(Ꮡb);
}

// Add sets z to the sum x+y and returns z.
[GoRecv] public static ж<ΔRat> Add(this ref ΔRat z, ж<ΔRat> Ꮡx, ж<ΔRat> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    ref var a1 = ref heap(new ΔInt(), out var Ꮡa1);
    ref var a2 = ref heap(new ΔInt(), out var Ꮡa2);
    a1.scaleDenom(Ꮡ(x.a), y.b.abs);
    a2.scaleDenom(Ꮡ(y.a), x.b.abs);
    z.a.Add(Ꮡa1, Ꮡa2);
    z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
    return z.norm();
}

// Sub sets z to the difference x-y and returns z.
[GoRecv] public static ж<ΔRat> Sub(this ref ΔRat z, ж<ΔRat> Ꮡx, ж<ΔRat> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    ref var a1 = ref heap(new ΔInt(), out var Ꮡa1);
    ref var a2 = ref heap(new ΔInt(), out var Ꮡa2);
    a1.scaleDenom(Ꮡ(x.a), y.b.abs);
    a2.scaleDenom(Ꮡ(y.a), x.b.abs);
    z.a.Sub(Ꮡa1, Ꮡa2);
    z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
    return z.norm();
}

// Mul sets z to the product x*y and returns z.
[GoRecv("capture")] public static ж<ΔRat> Mul(this ref ΔRat z, ж<ΔRat> Ꮡx, ж<ΔRat> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    if (Ꮡx == Ꮡy) {
        // a squared Rat is positive and can't be reduced (no need to call norm())
        z.a.neg = false;
        z.a.abs = z.a.abs.sqr(x.a.abs);
        if (len(x.b.abs) == 0){
            z.b.abs = z.b.abs.setWord(1);
        } else {
            z.b.abs = z.b.abs.sqr(x.b.abs);
        }
        return MulꓸᏑz;
    }
    z.a.Mul(Ꮡ(x.a), Ꮡ(y.a));
    z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
    return z.norm();
}

// Quo sets z to the quotient x/y and returns z.
// If y == 0, Quo panics.
[GoRecv] public static ж<ΔRat> Quo(this ref ΔRat z, ж<ΔRat> Ꮡx, ж<ΔRat> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    if (len(y.a.abs) == 0) {
        throw panic("division by zero");
    }
    ΔInt a = default!;
    ΔInt b = default!;
    a.scaleDenom(Ꮡ(x.a), y.b.abs);
    b.scaleDenom(Ꮡ(y.a), x.b.abs);
    z.a.abs = a.abs;
    z.b.abs = b.abs;
    z.a.neg = a.neg != b.neg;
    return z.norm();
}

} // end big_package
