// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements multi-precision rational numbers.

// package big -- go2cs converted at 2022 March 13 05:32:18 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\rat.go
namespace go.math;

using fmt = fmt_package;
using math = math_package;


// A Rat represents a quotient a/b of arbitrary precision.
// The zero value for a Rat represents the value 0.
//
// Operations always take pointer arguments (*Rat) rather
// than Rat values, and each unique Rat value requires
// its own unique *Rat pointer. To "copy" a Rat value,
// an existing (or newly allocated) Rat must be set to
// a new value using the Rat.Set method; shallow copies
// of Rats are not supported and may lead to errors.

public static partial class big_package {

public partial struct Rat {
    public Int a;
    public Int b;
}

// NewRat creates a new Rat with numerator a and denominator b.
public static ptr<Rat> NewRat(long a, long b) {
    return @new<Rat>().SetFrac64(a, b);
}

// SetFloat64 sets z to exactly f and returns z.
// If f is not finite, SetFloat returns nil.
private static ptr<Rat> SetFloat64(this ptr<Rat> _addr_z, double f) {
    ref Rat z = ref _addr_z.val;

    const nint expMask = 1 << 11 - 1;

    var bits = math.Float64bits(f);
    var mantissa = bits & (1 << 52 - 1);
    var exp = int((bits >> 52) & expMask);

    if (exp == expMask) // non-finite
        return _addr_null!;
    else if (exp == 0) // denormal
        exp -= 1022;
    else // normal
        mantissa |= 1 << 52;
        exp -= 1023;
        nint shift = 52 - exp; 

    // Optimization (?): partially pre-normalise.
    while (mantissa & 1 == 0 && shift > 0) {
        mantissa>>=1;
        shift--;
    }

    z.a.SetUint64(mantissa);
    z.a.neg = f < 0;
    z.b.Set(intOne);
    if (shift > 0) {
        z.b.Lsh(_addr_z.b, uint(shift));
    }
    else
 {
        z.a.Lsh(_addr_z.a, uint(-shift));
    }
    return _addr_z.norm()!;
}

// quotToFloat32 returns the non-negative float32 value
// nearest to the quotient a/b, using round-to-even in
// halfway cases. It does not mutate its arguments.
// Preconditions: b is non-zero; a and b have no common factors.
private static (float, bool) quotToFloat32(nat a, nat b) => func((_, panic, _) => {
    float f = default;
    bool exact = default;

 
    // float size in bits
    const nint Fsize = 32; 

    // mantissa
    const nint Msize = 23;
    const var Msize1 = Msize + 1; // incl. implicit 1
    const var Msize2 = Msize1 + 1; 

    // exponent
    const var Esize = Fsize - Msize1;
    const nint Ebias = 1 << (int)((Esize - 1)) - 1;
    const nint Emin = 1 - Ebias;
    const var Emax = Ebias; 

    // TODO(adonovan): specialize common degenerate cases: 1.0, integers.
    var alen = a.bitLen();
    if (alen == 0) {
        return (0, true);
    }
    var blen = b.bitLen();
    if (blen == 0) {
        panic("division by zero");
    }
    var exp = alen - blen;
    nat a2 = default;    nat b2 = default;

    a2 = a2.set(a);
    b2 = b2.set(b);
    {
        var shift__prev1 = shift;

        var shift = Msize2 - exp;

        if (shift > 0) {
            a2 = a2.shl(a2, uint(shift));
        }
        else if (shift < 0) {
            b2 = b2.shl(b2, uint(-shift));
        }

        shift = shift__prev1;

    } 

    // 2. Compute quotient and remainder (q, r).  NB: due to the
    // extra shift, the low-order bit of q is logically the
    // high-order bit of r.
    nat q = default;
    var (q, r) = q.div(a2, a2, b2); // (recycle a2)
    var mantissa = low32(q);
    var haveRem = len(r) > 0; // mantissa&1 && !haveRem => remainder is exactly half

    // 3. If quotient didn't fit in Msize2 bits, redo division by b2<<1
    // (in effect---we accomplish this incrementally).
    if (mantissa >> (int)(Msize2) == 1) {
        if (mantissa & 1 == 1) {
            haveRem = true;
        }
        mantissa>>=1;
        exp++;
    }
    if (mantissa >> (int)(Msize1) != 1) {
        panic(fmt.Sprintf("expected exactly %d bits of result", Msize2));
    }
    if (Emin - Msize <= exp && exp <= Emin) { 
        // Denormal case; lose 'shift' bits of precision.
        shift = uint(Emin - (exp - 1)); // [1..Esize1)
        var lostbits = mantissa & (1 << (int)(shift) - 1);
        haveRem = haveRem || lostbits != 0;
        mantissa>>=shift;
        exp = 2 - Ebias; // == exp + shift
    }
    exact = !haveRem;
    if (mantissa & 1 != 0) {
        exact = false;
        if (haveRem || mantissa & 2 != 0) {
            mantissa++;

            if (mantissa >= 1 << (int)(Msize2)) { 
                // Complete rollover 11...1 => 100...0, so shift is safe
                mantissa>>=1;
                exp++;
            }
        }
    }
    mantissa>>=1; // discard rounding bit.  Mantissa now scaled by 1<<Msize1.

    f = float32(math.Ldexp(float64(mantissa), exp - Msize1));
    if (math.IsInf(float64(f), 0)) {
        exact = false;
    }
    return ;
});

// quotToFloat64 returns the non-negative float64 value
// nearest to the quotient a/b, using round-to-even in
// halfway cases. It does not mutate its arguments.
// Preconditions: b is non-zero; a and b have no common factors.
private static (double, bool) quotToFloat64(nat a, nat b) => func((_, panic, _) => {
    double f = default;
    bool exact = default;

 
    // float size in bits
    const nint Fsize = 64; 

    // mantissa
    const nint Msize = 52;
    const var Msize1 = Msize + 1; // incl. implicit 1
    const var Msize2 = Msize1 + 1; 

    // exponent
    const var Esize = Fsize - Msize1;
    const nint Ebias = 1 << (int)((Esize - 1)) - 1;
    const nint Emin = 1 - Ebias;
    const var Emax = Ebias; 

    // TODO(adonovan): specialize common degenerate cases: 1.0, integers.
    var alen = a.bitLen();
    if (alen == 0) {
        return (0, true);
    }
    var blen = b.bitLen();
    if (blen == 0) {
        panic("division by zero");
    }
    var exp = alen - blen;
    nat a2 = default;    nat b2 = default;

    a2 = a2.set(a);
    b2 = b2.set(b);
    {
        var shift__prev1 = shift;

        var shift = Msize2 - exp;

        if (shift > 0) {
            a2 = a2.shl(a2, uint(shift));
        }
        else if (shift < 0) {
            b2 = b2.shl(b2, uint(-shift));
        }

        shift = shift__prev1;

    } 

    // 2. Compute quotient and remainder (q, r).  NB: due to the
    // extra shift, the low-order bit of q is logically the
    // high-order bit of r.
    nat q = default;
    var (q, r) = q.div(a2, a2, b2); // (recycle a2)
    var mantissa = low64(q);
    var haveRem = len(r) > 0; // mantissa&1 && !haveRem => remainder is exactly half

    // 3. If quotient didn't fit in Msize2 bits, redo division by b2<<1
    // (in effect---we accomplish this incrementally).
    if (mantissa >> (int)(Msize2) == 1) {
        if (mantissa & 1 == 1) {
            haveRem = true;
        }
        mantissa>>=1;
        exp++;
    }
    if (mantissa >> (int)(Msize1) != 1) {
        panic(fmt.Sprintf("expected exactly %d bits of result", Msize2));
    }
    if (Emin - Msize <= exp && exp <= Emin) { 
        // Denormal case; lose 'shift' bits of precision.
        shift = uint(Emin - (exp - 1)); // [1..Esize1)
        var lostbits = mantissa & (1 << (int)(shift) - 1);
        haveRem = haveRem || lostbits != 0;
        mantissa>>=shift;
        exp = 2 - Ebias; // == exp + shift
    }
    exact = !haveRem;
    if (mantissa & 1 != 0) {
        exact = false;
        if (haveRem || mantissa & 2 != 0) {
            mantissa++;

            if (mantissa >= 1 << (int)(Msize2)) { 
                // Complete rollover 11...1 => 100...0, so shift is safe
                mantissa>>=1;
                exp++;
            }
        }
    }
    mantissa>>=1; // discard rounding bit.  Mantissa now scaled by 1<<Msize1.

    f = math.Ldexp(float64(mantissa), exp - Msize1);
    if (math.IsInf(f, 0)) {
        exact = false;
    }
    return ;
});

// Float32 returns the nearest float32 value for x and a bool indicating
// whether f represents x exactly. If the magnitude of x is too large to
// be represented by a float32, f is an infinity and exact is false.
// The sign of f always matches the sign of x, even if f == 0.
private static (float, bool) Float32(this ptr<Rat> _addr_x) {
    float f = default;
    bool exact = default;
    ref Rat x = ref _addr_x.val;

    var b = x.b.abs;
    if (len(b) == 0) {
        b = natOne;
    }
    f, exact = quotToFloat32(x.a.abs, b);
    if (x.a.neg) {
        f = -f;
    }
    return ;
}

// Float64 returns the nearest float64 value for x and a bool indicating
// whether f represents x exactly. If the magnitude of x is too large to
// be represented by a float64, f is an infinity and exact is false.
// The sign of f always matches the sign of x, even if f == 0.
private static (double, bool) Float64(this ptr<Rat> _addr_x) {
    double f = default;
    bool exact = default;
    ref Rat x = ref _addr_x.val;

    var b = x.b.abs;
    if (len(b) == 0) {
        b = natOne;
    }
    f, exact = quotToFloat64(x.a.abs, b);
    if (x.a.neg) {
        f = -f;
    }
    return ;
}

// SetFrac sets z to a/b and returns z.
// If b == 0, SetFrac panics.
private static ptr<Rat> SetFrac(this ptr<Rat> _addr_z, ptr<Int> _addr_a, ptr<Int> _addr_b) => func((_, panic, _) => {
    ref Rat z = ref _addr_z.val;
    ref Int a = ref _addr_a.val;
    ref Int b = ref _addr_b.val;

    z.a.neg = a.neg != b.neg;
    var babs = b.abs;
    if (len(babs) == 0) {
        panic("division by zero");
    }
    if (_addr_z.a == b || alias(z.a.abs, babs)) {
        babs = nat(null).set(babs); // make a copy
    }
    z.a.abs = z.a.abs.set(a.abs);
    z.b.abs = z.b.abs.set(babs);
    return _addr_z.norm()!;
});

// SetFrac64 sets z to a/b and returns z.
// If b == 0, SetFrac64 panics.
private static ptr<Rat> SetFrac64(this ptr<Rat> _addr_z, long a, long b) => func((_, panic, _) => {
    ref Rat z = ref _addr_z.val;

    if (b == 0) {
        panic("division by zero");
    }
    z.a.SetInt64(a);
    if (b < 0) {
        b = -b;
        z.a.neg = !z.a.neg;
    }
    z.b.abs = z.b.abs.setUint64(uint64(b));
    return _addr_z.norm()!;
});

// SetInt sets z to x (by making a copy of x) and returns z.
private static ptr<Rat> SetInt(this ptr<Rat> _addr_z, ptr<Int> _addr_x) {
    ref Rat z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    z.a.Set(x);
    z.b.abs = z.b.abs.setWord(1);
    return _addr_z!;
}

// SetInt64 sets z to x and returns z.
private static ptr<Rat> SetInt64(this ptr<Rat> _addr_z, long x) {
    ref Rat z = ref _addr_z.val;

    z.a.SetInt64(x);
    z.b.abs = z.b.abs.setWord(1);
    return _addr_z!;
}

// SetUint64 sets z to x and returns z.
private static ptr<Rat> SetUint64(this ptr<Rat> _addr_z, ulong x) {
    ref Rat z = ref _addr_z.val;

    z.a.SetUint64(x);
    z.b.abs = z.b.abs.setWord(1);
    return _addr_z!;
}

// Set sets z to x (by making a copy of x) and returns z.
private static ptr<Rat> Set(this ptr<Rat> _addr_z, ptr<Rat> _addr_x) {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;

    if (z != x) {
        z.a.Set(_addr_x.a);
        z.b.Set(_addr_x.b);
    }
    if (len(z.b.abs) == 0) {
        z.b.abs = z.b.abs.setWord(1);
    }
    return _addr_z!;
}

// Abs sets z to |x| (the absolute value of x) and returns z.
private static ptr<Rat> Abs(this ptr<Rat> _addr_z, ptr<Rat> _addr_x) {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;

    z.Set(x);
    z.a.neg = false;
    return _addr_z!;
}

// Neg sets z to -x and returns z.
private static ptr<Rat> Neg(this ptr<Rat> _addr_z, ptr<Rat> _addr_x) {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;

    z.Set(x);
    z.a.neg = len(z.a.abs) > 0 && !z.a.neg; // 0 has no sign
    return _addr_z!;
}

// Inv sets z to 1/x and returns z.
// If x == 0, Inv panics.
private static ptr<Rat> Inv(this ptr<Rat> _addr_z, ptr<Rat> _addr_x) => func((_, panic, _) => {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;

    if (len(x.a.abs) == 0) {
        panic("division by zero");
    }
    z.Set(x);
    (z.a.abs, z.b.abs) = (z.b.abs, z.a.abs);    return _addr_z!;
});

// Sign returns:
//
//    -1 if x <  0
//     0 if x == 0
//    +1 if x >  0
//
private static nint Sign(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    return x.a.Sign();
}

// IsInt reports whether the denominator of x is 1.
private static bool IsInt(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    return len(x.b.abs) == 0 || x.b.abs.cmp(natOne) == 0;
}

// Num returns the numerator of x; it may be <= 0.
// The result is a reference to x's numerator; it
// may change if a new value is assigned to x, and vice versa.
// The sign of the numerator corresponds to the sign of x.
private static ptr<Int> Num(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    return _addr__addr_x.a!;
}

// Denom returns the denominator of x; it is always > 0.
// The result is a reference to x's denominator, unless
// x is an uninitialized (zero value) Rat, in which case
// the result is a new Int of value 1. (To initialize x,
// any operation that sets x will do, including x.Set(x).)
// If the result is a reference to x's denominator it
// may change if a new value is assigned to x, and vice versa.
private static ptr<Int> Denom(this ptr<Rat> _addr_x) {
    ref Rat x = ref _addr_x.val;

    x.b.neg = false; // the result is always >= 0
    if (len(x.b.abs) == 0) { 
        // Note: If this proves problematic, we could
        //       panic instead and require the Rat to
        //       be explicitly initialized.
        return addr(new Int(abs:nat{1}));
    }
    return _addr__addr_x.b!;
}

private static ptr<Rat> norm(this ptr<Rat> _addr_z) {
    ref Rat z = ref _addr_z.val;


    if (len(z.a.abs) == 0) 
    {
        // z == 0; normalize sign and denominator
        z.a.neg = false;
        fallthrough = true;
    }
    if (fallthrough || len(z.b.abs) == 0) 
    {
        // z is integer; normalize denominator
        z.b.abs = z.b.abs.setWord(1);
        goto __switch_break0;
    }
    // default: 
        // z is fraction; normalize numerator and denominator
        var neg = z.a.neg;
        z.a.neg = false;
        z.b.neg = false;
        {
            var f = NewInt(0).lehmerGCD(null, null, _addr_z.a, _addr_z.b);

            if (f.Cmp(intOne) != 0) {
                z.a.abs, _ = z.a.abs.div(null, z.a.abs, f.abs);
                z.b.abs, _ = z.b.abs.div(null, z.b.abs, f.abs);
            }

        }
        z.a.neg = neg;

    __switch_break0:;
    return _addr_z!;
}

// mulDenom sets z to the denominator product x*y (by taking into
// account that 0 values for x or y must be interpreted as 1) and
// returns z.
private static nat mulDenom(nat z, nat x, nat y) {

    if (len(x) == 0 && len(y) == 0) 
        return z.setWord(1);
    else if (len(x) == 0) 
        return z.set(y);
    else if (len(y) == 0) 
        return z.set(x);
        return z.mul(x, y);
}

// scaleDenom sets z to the product x*f.
// If f == 0 (zero value of denominator), z is set to (a copy of) x.
private static void scaleDenom(this ptr<Int> _addr_z, ptr<Int> _addr_x, nat f) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    if (len(f) == 0) {
        z.Set(x);
        return ;
    }
    z.abs = z.abs.mul(x.abs, f);
    z.neg = x.neg;
}

// Cmp compares x and y and returns:
//
//   -1 if x <  y
//    0 if x == y
//   +1 if x >  y
//
private static nint Cmp(this ptr<Rat> _addr_x, ptr<Rat> _addr_y) {
    ref Rat x = ref _addr_x.val;
    ref Rat y = ref _addr_y.val;

    Int a = default;    ref Int b = ref heap(out ptr<Int> _addr_b);

    a.scaleDenom(_addr_x.a, y.b.abs);
    b.scaleDenom(_addr_y.a, x.b.abs);
    return a.Cmp(_addr_b);
}

// Add sets z to the sum x+y and returns z.
private static ptr<Rat> Add(this ptr<Rat> _addr_z, ptr<Rat> _addr_x, ptr<Rat> _addr_y) {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;
    ref Rat y = ref _addr_y.val;

    ref Int a1 = ref heap(out ptr<Int> _addr_a1);    ref Int a2 = ref heap(out ptr<Int> _addr_a2);

    a1.scaleDenom(_addr_x.a, y.b.abs);
    a2.scaleDenom(_addr_y.a, x.b.abs);
    z.a.Add(_addr_a1, _addr_a2);
    z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
    return _addr_z.norm()!;
}

// Sub sets z to the difference x-y and returns z.
private static ptr<Rat> Sub(this ptr<Rat> _addr_z, ptr<Rat> _addr_x, ptr<Rat> _addr_y) {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;
    ref Rat y = ref _addr_y.val;

    ref Int a1 = ref heap(out ptr<Int> _addr_a1);    ref Int a2 = ref heap(out ptr<Int> _addr_a2);

    a1.scaleDenom(_addr_x.a, y.b.abs);
    a2.scaleDenom(_addr_y.a, x.b.abs);
    z.a.Sub(_addr_a1, _addr_a2);
    z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
    return _addr_z.norm()!;
}

// Mul sets z to the product x*y and returns z.
private static ptr<Rat> Mul(this ptr<Rat> _addr_z, ptr<Rat> _addr_x, ptr<Rat> _addr_y) {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;
    ref Rat y = ref _addr_y.val;

    if (x == y) { 
        // a squared Rat is positive and can't be reduced (no need to call norm())
        z.a.neg = false;
        z.a.abs = z.a.abs.sqr(x.a.abs);
        if (len(x.b.abs) == 0) {
            z.b.abs = z.b.abs.setWord(1);
        }
        else
 {
            z.b.abs = z.b.abs.sqr(x.b.abs);
        }
        return _addr_z!;
    }
    z.a.Mul(_addr_x.a, _addr_y.a);
    z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
    return _addr_z.norm()!;
}

// Quo sets z to the quotient x/y and returns z.
// If y == 0, Quo panics.
private static ptr<Rat> Quo(this ptr<Rat> _addr_z, ptr<Rat> _addr_x, ptr<Rat> _addr_y) => func((_, panic, _) => {
    ref Rat z = ref _addr_z.val;
    ref Rat x = ref _addr_x.val;
    ref Rat y = ref _addr_y.val;

    if (len(y.a.abs) == 0) {
        panic("division by zero");
    }
    Int a = default;    Int b = default;

    a.scaleDenom(_addr_x.a, y.b.abs);
    b.scaleDenom(_addr_y.a, x.b.abs);
    z.a.abs = a.abs;
    z.b.abs = b.abs;
    z.a.neg = a.neg != b.neg;
    return _addr_z.norm()!;
});

} // end big_package
