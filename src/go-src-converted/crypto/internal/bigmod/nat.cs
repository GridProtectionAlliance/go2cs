// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using big = math.big_package;
using bits = math.bits_package;
using go.@internal;
using math;

partial class bigmod_package {

internal static readonly UntypedInt _W = /* bits.UintSize */ 64;
internal static readonly UntypedInt _S = /* _W / 8 */ 8;

[GoType("num:nuint")] public partial struct choice;

internal static choice not(choice c) {
    return (choice)(1 ^ c);
}

internal static readonly choice yes = /* choice(1) */ 1;

internal static readonly choice no = /* choice(0) */ 0;

// ctMask is all 1s if on is yes, and all 0s otherwise.
internal static nuint ctMask(choice on) {
    return ((nuint)0 - (nuint)on);
}

// ctEq returns 1 if x == y, and 0 otherwise. The execution time of this
// function does not depend on its inputs.
internal static choice ctEq(nuint x, nuint y) {
    // If x != y, then either x - y or y - x will generate a carry.
    var (_, c1) = bits.Sub(x, y, 0);
    var (_, c2) = bits.Sub(y, x, 0);
    return not(((choice)((nuint)(c1 | c2))));
}

// Nat represents an arbitrary natural number
//
// Each Nat has an announced length, which is the number of limbs it has stored.
// Operations on this number are allowed to leak this length, but will not leak
// any information about the values contained in those limbs.
[GoType] partial struct ΔNat {
    // limbs is little-endian in base 2^W with W = bits.UintSize.
    internal slice<nuint> limbs;
}

// preallocTarget is the size in bits of the numbers used to implement the most
// common and most performant RSA key size. It's also enough to cover some of
// the operations of key sizes up to 4096.
internal static readonly UntypedInt preallocTarget = 2048;

internal static readonly UntypedInt preallocLimbs = /* (preallocTarget + _W - 1) / _W */ 32;

// NewNat returns a new nat with a size of zero, just like new(Nat), but with
// the preallocated capacity to hold a number of up to preallocTarget bits.
// NewNat inlines, so the allocation can live on the stack.
public static ж<ΔNat> NewNat() {
    var limbs = new slice<nuint>(0, preallocLimbs);
    return Ꮡ(new ΔNat(limbs));
}

// expand expands x to n limbs, leaving its value unchanged.
internal static ж<ΔNat> expand(this ж<ΔNat> Ꮡx, nint n) {
    ref var x = ref Ꮡx.Value;

    if (len(x.limbs) > n) {
        throw panic("bigmod: internal error: shrinking nat");
    }
    if (cap(x.limbs) < n) {
        var newLimbs = new slice<nuint>(n);
        copy(newLimbs, x.limbs);
        x.limbs = newLimbs;
        return Ꮡx;
    }
    var extraLimbs = x.limbs[(int)(len(x.limbs))..(int)(n)];
    clear(extraLimbs);
    x.limbs = x.limbs[..(int)(n)];
    return Ꮡx;
}

// reset returns a zero nat of n limbs, reusing x's storage if n <= cap(x.limbs).
internal static ж<ΔNat> reset(this ж<ΔNat> Ꮡx, nint n) {
    ref var x = ref Ꮡx.Value;

    if (cap(x.limbs) < n) {
        x.limbs = new slice<nuint>(n);
        return Ꮡx;
    }
    clear(x.limbs);
    x.limbs = x.limbs[..(int)(n)];
    return Ꮡx;
}

// set assigns x = y, optionally resizing x to the appropriate size.
internal static ж<ΔNat> set(this ж<ΔNat> Ꮡx, ж<ΔNat> Ꮡy) {
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    Ꮡx.reset(len(y.limbs));
    copy(x.limbs, y.limbs);
    return Ꮡx;
}

// setBig assigns x = n, optionally resizing n to the appropriate size.
//
// The announced length of x is set based on the actual bit size of the input,
// ignoring leading zeroes.
internal static ж<ΔNat> setBig(this ж<ΔNat> Ꮡx, ж<bigꓸInt> Ꮡn) {
    ref var x = ref Ꮡx.Value;
    ref var n = ref Ꮡn.Value;

    var limbs = n.Bits();
    Ꮡx.reset(len(limbs));
    foreach (var (i, _) in limbs) {
        x.limbs[i] = (nuint)limbs[i];
    }
    return Ꮡx;
}

// Bytes returns x as a zero-extended big-endian byte slice. The size of the
// slice will match the size of m.
//
// x must have the same size as m and it must be reduced modulo m.
[GoRecv] public static slice<byte> Bytes(this ref ΔNat x, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    nint i = m.Size();
    var bytes = new slice<byte>(i);
    foreach (var (_, vᴛ1) in x.limbs) {
        var limb = vᴛ1;

        for (nint j = 0; j < _S; j++) {
            i--;
            if (i < 0) {
                if (limb == 0) {
                    break;
                }
                throw panic("bigmod: modulus is smaller than nat");
            }
            bytes[i] = (byte)limb;
            limb >>= (int)(8);
        }
    }
    return bytes;
}

// SetBytes assigns x = b, where b is a slice of big-endian bytes.
// SetBytes returns an error if b >= m.
//
// The output will be resized to the size of m and overwritten.
public static (ж<ΔNat>, error) SetBytes(this ж<ΔNat> Ꮡx, slice<byte> b, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    {
        var err = Ꮡx.setBytes(b, Ꮡm); if (err != default!) {
            return (default!, err);
        }
    }
    if (x.cmpGeq(m.nat) == yes) {
        return (default!, errors.New("input overflows the modulus"u8));
    }
    return (Ꮡx, default!);
}

// SetOverflowingBytes assigns x = b, where b is a slice of big-endian bytes.
// SetOverflowingBytes returns an error if b has a longer bit length than m, but
// reduces overflowing values up to 2^⌈log2(m)⌉ - 1.
//
// The output will be resized to the size of m and overwritten.
public static (ж<ΔNat>, error) SetOverflowingBytes(this ж<ΔNat> Ꮡx, slice<byte> b, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    {
        var err = Ꮡx.setBytes(b, Ꮡm); if (err != default!) {
            return (default!, err);
        }
    }
    nint leading = (nint)_W - bitLen(x.limbs[len(x.limbs) - 1]);
    if (leading < m.leading) {
        return (default!, errors.New("input overflows the modulus size"u8));
    }
    Ꮡx.maybeSubtractModulus(no, Ꮡm);
    return (Ꮡx, default!);
}

// bigEndianUint returns the contents of buf interpreted as a
// big-endian encoded uint value.
internal static nuint bigEndianUint(slice<byte> buf) {
    if (_W == 64) {
        return (nuint)byteorder.BeUint64(buf);
    }
    return (nuint)byteorder.BeUint32(buf);
}

internal static error setBytes(this ж<ΔNat> Ꮡx, slice<byte> b, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    Ꮡx.resetFor(Ꮡm);
    nint i = len(b);
    nint k = 0;
    while (k < len(x.limbs) && i >= _S) {
        x.limbs[k] = bigEndianUint(b[(int)(i - (nint)_S)..(int)(i)]);
        i -= _S;
        k++;
    }
    for (nint s = 0; s < _W && k < len(x.limbs) && i > 0; s += 8) {
        x.limbs[k] |= (nuint)(((nuint)b[i - 1] << (int)(s)));
        i--;
    }
    if (i > 0) {
        return errors.New("input overflows the modulus size"u8);
    }
    return default!;
}

// Equal returns 1 if x == y, and 0 otherwise.
//
// Both operands must have the same announced length.
[GoRecv] public static choice Equal(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    ref var y = ref Ꮡy.Value;

    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var yLimbs = y.limbs[..(int)(size)];
    choice equal = yes;
    for (nint i = 0; i < size; i++) {
        equal &= (choice)(ctEq(xLimbs[i], yLimbs[i]));
    }
    return equal;
}

// IsZero returns 1 if x == 0, and 0 otherwise.
[GoRecv] public static choice IsZero(this ref ΔNat x) {
    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    choice zero = yes;
    for (nint i = 0; i < size; i++) {
        zero &= (choice)(ctEq(xLimbs[i], 0));
    }
    return zero;
}

// cmpGeq returns 1 if x >= y, and 0 otherwise.
//
// Both operands must have the same announced length.
[GoRecv] internal static choice cmpGeq(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    ref var y = ref Ꮡy.Value;

    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var yLimbs = y.limbs[..(int)(size)];
    nuint c = default!;
    for (nint i = 0; i < size; i++) {
        (_, c) = bits.Sub(xLimbs[i], yLimbs[i], c);
    }
    // If there was a carry, then subtracting y underflowed, so
    // x is not greater than or equal to y.
    return not(((choice)c));
}

// assign sets x <- y if on == 1, and does nothing otherwise.
//
// Both operands must have the same announced length.
internal static ж<ΔNat> assign(this ж<ΔNat> Ꮡx, choice on, ж<ΔNat> Ꮡy) {
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var yLimbs = y.limbs[..(int)(size)];
    nuint mask = ctMask(on);
    for (nint i = 0; i < size; i++) {
        xLimbs[i] ^= (nuint)((nuint)(mask & ((nuint)(xLimbs[i] ^ yLimbs[i]))));
    }
    return Ꮡx;
}

// add computes x += y and returns the carry.
//
// Both operands must have the same announced length.
[GoRecv] internal static nuint /*c*/ add(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    nuint c = default!;

    ref var y = ref Ꮡy.Value;
    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var yLimbs = y.limbs[..(int)(size)];
    for (nint i = 0; i < size; i++) {
        (xLimbs[i], c) = bits.Add(xLimbs[i], yLimbs[i], c);
    }
    return c;
}

// sub computes x -= y. It returns the borrow of the subtraction.
//
// Both operands must have the same announced length.
[GoRecv] internal static nuint /*c*/ sub(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    nuint c = default!;

    ref var y = ref Ꮡy.Value;
    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var yLimbs = y.limbs[..(int)(size)];
    for (nint i = 0; i < size; i++) {
        (xLimbs[i], c) = bits.Sub(xLimbs[i], yLimbs[i], c);
    }
    return c;
}

// Modulus is used for modular arithmetic, precomputing relevant constants.
//
// Moduli are assumed to be odd numbers. Moduli can also leak the exact
// number of bits needed to store their value, and are stored without padding.
//
// Their actual value is still kept secret.
[GoType] partial struct Modulus {
    // The underlying natural number for this modulus.
    //
    // This will be stored without any padding, and shouldn't alias with any
    // other natural number being used.
    internal ж<ΔNat> nat;
    internal nint leading; // number of leading zeros in the modulus
    internal nuint m0inv; // -nat.limbs[0]⁻¹ mod _W
    internal ж<ΔNat> rr; // R*R for montgomeryRepresentation
}

// rr returns R*R with R = 2^(_W * n) and n = len(m.nat.limbs).
internal static ж<ΔNat> rr(ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    var rr = NewNat().ExpandFor(Ꮡm);
    nuint n = (nuint)len((~rr).limbs);
    nuint mLen = (nuint)m.BitLen();
    nuint logR = (nuint)_W * n;
    // We start by computing R = 2^(_W * n) mod m. We can get pretty close, to
    // 2^⌊log₂m⌋, by setting the highest bit we can without having to reduce.
    rr.Value.limbs[(nint)(n - 1)] = ((nuint)1 << (int)(((mLen - 1) % (nuint)_W)));
    // Then we double until we reach 2^(_W * n).
    for (nuint iΔ1 = mLen - 1; iΔ1 < logR; iΔ1++) {
        rr.Add(rr, Ꮡm);
    }
    // Next we need to get from R to 2^(_W * n) R mod m (aka from one to R in
    // the Montgomery domain, meaning we can use Montgomery multiplication now).
    // We could do that by doubling _W * n times, or with a square-and-double
    // chain log2(_W * n) long. Turns out the fastest thing is to start out with
    // doublings, and switch to square-and-double once the exponent is large
    // enough to justify the cost of the multiplications.
    // The threshold is selected experimentally as a linear function of n.
    nuint threshold = n / 4;
    // We calculate how many of the most-significant bits of the exponent we can
    // compute before crossing the threshold, and we do it with doublings.
    nint i = bits.UintSize;
    while ((logR >> (int)(i)) <= threshold) {
        i--;
    }
    for (nuint k = (nuint)0; k < (logR >> (int)(i)); k++) {
        rr.Add(rr, Ꮡm);
    }
    // Then we process the remaining bits of the exponent with a
    // square-and-double chain.
    while (i > 0) {
        rr.montgomeryMul(rr, rr, Ꮡm);
        i--;
        if ((nuint)((logR >> (int)(i)) & 1) != 0) {
            rr.Add(rr, Ꮡm);
        }
    }
    return rr;
}

// minusInverseModW computes -x⁻¹ mod _W with x odd.
//
// This operation is used to precompute a constant involved in Montgomery
// multiplication.
internal static nuint minusInverseModW(nuint x) {
    // Every iteration of this loop doubles the least-significant bits of
    // correct inverse in y. The first three bits are already correct (1⁻¹ = 1,
    // 3⁻¹ = 3, 5⁻¹ = 5, and 7⁻¹ = 7 mod 8), so doubling five times is enough
    // for 64 bits (and wastes only one iteration for 32 bits).
    //
    // See https://crypto.stackexchange.com/a/47496.
    nuint y = x;
    for (nint i = 0; i < 5; i++) {
        y = y * (2 - x * y);
    }
    return ((nuint)0 - y);
}

// NewModulusFromBig creates a new Modulus from a [big.Int].
//
// The Int must be odd. The number of significant bits (and nothing else) is
// leaked through timing side-channels.
public static (ж<Modulus>, error) NewModulusFromBig(ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.Value;

    {
        var b = n.Bits(); if (len(b) == 0){
            return (default!, errors.New("modulus must be >= 0"u8));
        } else 
        if ((big.Word)(b[0] & 1) != 1) {
            return (default!, errors.New("modulus must be odd"u8));
        }
    }
    var m = Ꮡ(new Modulus(nil));
    m.Value.nat = NewNat().setBig(Ꮡn);
    m.Value.leading = (nint)_W - bitLen((~(~m).nat).limbs[len((~(~m).nat).limbs) - 1]);
    m.Value.m0inv = minusInverseModW((~(~m).nat).limbs[0]);
    m.Value.rr = rr(m);
    return (m, default!);
}

// bitLen is a version of bits.Len that only leaks the bit length of n, but not
// its value. bits.Len and bits.LeadingZeros use a lookup table for the
// low-order bits on some architectures.
internal static nint bitLen(nuint n) {
    nint len = default!;
    // We assume, here and elsewhere, that comparison to zero is constant time
    // with respect to different non-zero values.
    while (n != 0) {
        len++;
        n >>= (int)(1);
    }
    return len;
}

// Size returns the size of m in bytes.
[GoRecv] public static nint Size(this ref Modulus m) {
    return (m.BitLen() + 7) / 8;
}

// BitLen returns the size of m in bits.
[GoRecv] public static nint BitLen(this ref Modulus m) {
    return len((~m.nat).limbs) * (nint)_W - (nint)m.leading;
}

// Nat returns m as a Nat. The return value must not be written to.
[GoRecv] public static ж<ΔNat> Nat(this ref Modulus m) {
    return m.nat;
}

// shiftIn calculates x = x << _W + y mod m.
//
// This assumes that x is already reduced mod m.
internal static ж<ΔNat> shiftIn(this ж<ΔNat> Ꮡx, nuint y, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    var d = NewNat().resetFor(Ꮡm);
    // Eliminate bounds checks in the loop.
    nint size = len((~m.nat).limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var dLimbs = (~d).limbs[..(int)(size)];
    var mLimbs = (~m.nat).limbs[..(int)(size)];
    // Each iteration of this loop computes x = 2x + b mod m, where b is a bit
    // from y. Effectively, it left-shifts x and adds y one bit at a time,
    // reducing it every time.
    //
    // To do the reduction, each iteration computes both 2x + b and 2x + b - m.
    // The next iteration (and finally the return line) will use either result
    // based on whether 2x + b overflows m.
    choice needSubtraction = no;
    for (nint i = _W - 1; i >= 0; i--) {
        nuint carry = (nuint)(((y >> (int)(i))) & 1);
        nuint borrow = default!;
        nuint mask = ctMask(needSubtraction);
        for (nint iΔ1 = 0; iΔ1 < size; iΔ1++) {
            nuint l = (nuint)(xLimbs[iΔ1] ^ ((nuint)(mask & ((nuint)(xLimbs[iΔ1] ^ dLimbs[iΔ1])))));
            (xLimbs[iΔ1], carry) = bits.Add(l, l, carry);
            (dLimbs[iΔ1], borrow) = bits.Sub(xLimbs[iΔ1], mLimbs[iΔ1], borrow);
        }
        // Like in maybeSubtractModulus, we need the subtraction if either it
        // didn't underflow (meaning 2x + b > m) or if computing 2x + b
        // overflowed (meaning 2x + b > 2^_W*n > m).
        needSubtraction = (choice)(not(((choice)borrow)) | ((choice)carry));
    }
    return Ꮡx.assign(needSubtraction, d);
}

// Mod calculates out = x mod m.
//
// This works regardless how large the value of x is.
//
// The output will be resized to the size of m and overwritten.
public static ж<ΔNat> Mod(this ж<ΔNat> Ꮡout, ж<ΔNat> Ꮡx, ж<Modulus> Ꮡm) {
    ref var @out = ref Ꮡout.Value;
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    Ꮡout.resetFor(Ꮡm);
    // Working our way from the most significant to the least significant limb,
    // we can insert each limb at the least significant position, shifting all
    // previous limbs left by _W. This way each limb will get shifted by the
    // correct number of bits. We can insert at least N - 1 limbs without
    // overflowing m. After that, we need to reduce every time we shift.
    nint i = len(x.limbs) - 1;
    // For the first N - 1 limbs we can skip the actual shifting and position
    // them at the shifted position, which starts at min(N - 2, i).
    nint start = len((~m.nat).limbs) - 2;
    if (i < start) {
        start = i;
    }
    for (nint j = start; j >= 0; j--) {
        @out.limbs[j] = x.limbs[i];
        i--;
    }
    // We shift in the remaining limbs, reducing modulo m each time.
    while (i >= 0) {
        Ꮡout.shiftIn(x.limbs[i], Ꮡm);
        i--;
    }
    return Ꮡout;
}

// ExpandFor ensures x has the right size to work with operations modulo m.
//
// The announced size of x must be smaller than or equal to that of m.
public static ж<ΔNat> ExpandFor(this ж<ΔNat> Ꮡx, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    return Ꮡx.expand(len((~m.nat).limbs));
}

// resetFor ensures out has the right size to work with operations modulo m.
//
// out is zeroed and may start at any size.
internal static ж<ΔNat> resetFor(this ж<ΔNat> Ꮡout, ж<Modulus> Ꮡm) {
    ref var @out = ref Ꮡout.Value;
    ref var m = ref Ꮡm.Value;

    return Ꮡout.reset(len((~m.nat).limbs));
}

// maybeSubtractModulus computes x -= m if and only if x >= m or if "always" is yes.
//
// It can be used to reduce modulo m a value up to 2m - 1, which is a common
// range for results computed by higher level operations.
//
// always is usually a carry that indicates that the operation that produced x
// overflowed its size, meaning abstractly x > 2^_W*n > m even if x < m.
//
// x and m operands must have the same announced length.
internal static void maybeSubtractModulus(this ж<ΔNat> Ꮡx, choice always, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    var t = NewNat().set(Ꮡx);
    nuint underflow = t.sub(m.nat);
    // We keep the result if x - m didn't underflow (meaning x >= m)
    // or if always was set.
    choice keep = (choice)(not(((choice)underflow)) | always);
    Ꮡx.assign(keep, t);
}

// Sub computes x = x - y mod m.
//
// The length of both operands must be the same as the modulus. Both operands
// must already be reduced modulo m.
public static ж<ΔNat> Sub(this ж<ΔNat> Ꮡx, ж<ΔNat> Ꮡy, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;
    ref var m = ref Ꮡm.Value;

    nuint underflow = x.sub(Ꮡy);
    // If the subtraction underflowed, add m.
    var t = NewNat().set(Ꮡx);
    t.add(m.nat);
    Ꮡx.assign(((choice)underflow), t);
    return Ꮡx;
}

// Add computes x = x + y mod m.
//
// The length of both operands must be the same as the modulus. Both operands
// must already be reduced modulo m.
public static ж<ΔNat> Add(this ж<ΔNat> Ꮡx, ж<ΔNat> Ꮡy, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;
    ref var m = ref Ꮡm.Value;

    nuint overflow = x.add(Ꮡy);
    Ꮡx.maybeSubtractModulus(((choice)overflow), Ꮡm);
    return Ꮡx;
}

// montgomeryRepresentation calculates x = x * R mod m, with R = 2^(_W * n) and
// n = len(m.nat.limbs).
//
// Faster Montgomery multiplication replaces standard modular multiplication for
// numbers in this representation.
//
// This assumes that x is already reduced mod m.
internal static ж<ΔNat> montgomeryRepresentation(this ж<ΔNat> Ꮡx, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    // A Montgomery multiplication (which computes a * b / R) by R * R works out
    // to a multiplication by R, which takes the value out of the Montgomery domain.
    return Ꮡx.montgomeryMul(Ꮡx, m.rr, Ꮡm);
}

// montgomeryReduction calculates x = x / R mod m, with R = 2^(_W * n) and
// n = len(m.nat.limbs).
//
// This assumes that x is already reduced mod m.
internal static ж<ΔNat> montgomeryReduction(this ж<ΔNat> Ꮡx, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    // By Montgomery multiplying with 1 not in Montgomery representation, we
    // convert out back from Montgomery representation, because it works out to
    // dividing by R.
    var one = NewNat().ExpandFor(Ꮡm);
    one.Value.limbs[0] = 1;
    return Ꮡx.montgomeryMul(Ꮡx, one, Ꮡm);
}

// montgomeryMul calculates x = a * b / R mod m, with R = 2^(_W * n) and
// n = len(m.nat.limbs), also known as a Montgomery multiplication.
//
// All inputs should be the same length and already reduced modulo m.
// x will be resized to the size of m and overwritten.
internal static ж<ΔNat> montgomeryMul(this ж<ΔNat> Ꮡx, ж<ΔNat> Ꮡa, ж<ΔNat> Ꮡb, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var a = ref Ꮡa.Value;
    ref var b = ref Ꮡb.Value;
    ref var m = ref Ꮡm.Value;

    nint n = len((~m.nat).limbs);
    var mLimbs = (~m.nat).limbs[..(int)(n)];
    var aLimbs = a.limbs[..(int)(n)];
    var bLimbs = b.limbs[..(int)(n)];
    var exprᴛ1 = n;
    if (exprᴛ1 == 1024 / _W) {
// Attempt to use a stack-allocated backing array.
// bounds check elimination hint
// Step 1 (T = a × b) is computed as a large pen-and-paper column
// multiplication of two numbers with n base-2^_W digits. If we just
// wanted to produce 2n-wide T, we would do
//
//   for i := 0; i < n; i++ {
//       d := bLimbs[i]
//       T[n+i] = addMulVVW(T[i:n+i], aLimbs, d)
//   }
//
// where d is a digit of the multiplier, T[i:n+i] is the shifted
// position of the product of that digit, and T[n+i] is the final carry.
// Note that T[i] isn't modified after processing the i-th digit.
//
// Instead of running two loops, one for Step 1 and one for Steps 2–6,
// the result of Step 1 is computed during the next loop. This is
// possible because each iteration only uses T[i] in Step 2 and then
// discards it in Step 6.
// Step 6 is replaced by shifting the virtual window we operate
// over: T of the algorithm is T[i:] for us. That means that T1 in
// Step 2 (T mod 2^_W) is simply T[i]. k0 in Step 3 is our m0inv.
// Step 4 and 5 add Y × m to T, which as mentioned above is stored
// at T[i:]. The two carries (from a × d and Y × m) are added up in
// the next word T[n+i], and the carry bit from that addition is
// brought forward to the next iteration.
// Finally for Step 7 we copy the final T window into x, and subtract m
// if necessary (which as explained in maybeSubtractModulus can be the
// case both if x >= m, or if x overflowed).
//
// The paper suggests in Section 4 that we can do an "Almost Montgomery
// Multiplication" by subtracting only in the overflow case, but the
// cost is very similar since the constant time subtraction tells us if
// x >= m as a side effect, and taking care of the broken invariant is
// highly undesirable (see https://go.dev/issue/13907).
// The following specialized cases follow the exact same algorithm, but
// optimized for the sizes most used in RSA. addMulVVW is implemented in
// assembly with loop unrolling depending on the architecture and bounds
// checks are removed by the compiler thanks to the constant size.
        UntypedInt nΔ4 = /* 1024 / _W */ 16; // compiler hint
        var T = new slice<nuint>(nΔ4 * 2);
        nuint c = default!;
        for (nint i = 0; i < nΔ4; i++) {
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW1024(Ꮡ(T, i), Ꮡ(aLimbs, 0), d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW1024(Ꮡ(T, i), Ꮡ(mLimbs, 0), Y);
            (T[(nint)nΔ4 + i], c) = bits.Add(c1, c2, c);
        }
        copy((~Ꮡx.reset(nΔ4)).limbs, T[(int)(nΔ4)..]);
        Ꮡx.maybeSubtractModulus(((choice)c), Ꮡm);
    }
    else if (exprᴛ1 == 1536 / _W) {
        UntypedInt nΔ5 = /* 1536 / _W */ 24; // compiler hint
        var T = new slice<nuint>(nΔ5 * 2);
        nuint c = default!;
        for (nint i = 0; i < nΔ5; i++) {
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW1536(Ꮡ(T, i), Ꮡ(aLimbs, 0), d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW1536(Ꮡ(T, i), Ꮡ(mLimbs, 0), Y);
            (T[(nint)nΔ5 + i], c) = bits.Add(c1, c2, c);
        }
        copy((~Ꮡx.reset(nΔ5)).limbs, T[(int)(nΔ5)..]);
        Ꮡx.maybeSubtractModulus(((choice)c), Ꮡm);
    }
    else if (exprᴛ1 == 2048 / _W) {
        UntypedInt nΔ6 = /* 2048 / _W */ 32; // compiler hint
        var T = new slice<nuint>(nΔ6 * 2);
        nuint c = default!;
        for (nint i = 0; i < nΔ6; i++) {
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW2048(Ꮡ(T, i), Ꮡ(aLimbs, 0), d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW2048(Ꮡ(T, i), Ꮡ(mLimbs, 0), Y);
            (T[(nint)nΔ6 + i], c) = bits.Add(c1, c2, c);
        }
        copy((~Ꮡx.reset(nΔ6)).limbs, T[(int)(nΔ6)..]);
        Ꮡx.maybeSubtractModulus(((choice)c), Ꮡm);
    }
    else { /* default: */
        var T = new slice<nuint>(0, preallocLimbs * 2);
        if (cap(T) < n * 2) {
            T = new slice<nuint>(0, n * 2);
        }
        T = T[..(int)(n * 2)];
        // This loop implements Word-by-Word Montgomery Multiplication, as
        // described in Algorithm 4 (Fig. 3) of "Efficient Software
        // Implementations of Modular Exponentiation" by Shay Gueron
        // [https://eprint.iacr.org/2011/239.pdf].
        nuint c = default!;
        for (nint i = 0; i < n; i++) {
            _ = T[n + i];
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW(T[(int)(i)..(int)(n + i)], aLimbs, d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW(T[(int)(i)..(int)(n + i)], mLimbs, Y);
            (T[n + i], c) = bits.Add(c1, c2, c);
        }
        copy((~Ꮡx.reset(n)).limbs, T[(int)(n)..]);
        Ꮡx.maybeSubtractModulus(((choice)c), Ꮡm);
    }

    return Ꮡx;
}

// addMulVVW multiplies the multi-word value x by the single-word value y,
// adding the result to the multi-word value z and returning the final carry.
// It can be thought of as one row of a pen-and-paper column multiplication.
internal static nuint /*carry*/ addMulVVW(slice<nuint> z, slice<nuint> x, nuint y) {
    nuint carry = default!;

    _ = x[len(z) - 1];
    // bounds check elimination hint
    foreach (var (i, _) in z) {
        var (hi, lo) = bits.Mul(x[i], y);
        (lo, var c) = bits.Add(lo, z[i], 0);
        // We use bits.Add with zero to get an add-with-carry instruction that
        // absorbs the carry from the previous bits.Add.
        (hi, _) = bits.Add(hi, 0, c);
        (lo, c) = bits.Add(lo, carry, 0);
        (hi, _) = bits.Add(hi, 0, c);
        carry = hi;
        z[i] = lo;
    }
    return carry;
}

// Mul calculates x = x * y mod m.
//
// The length of both operands must be the same as the modulus. Both operands
// must already be reduced modulo m.
public static ж<ΔNat> Mul(this ж<ΔNat> Ꮡx, ж<ΔNat> Ꮡy, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;
    ref var m = ref Ꮡm.Value;

    // A Montgomery multiplication by a value out of the Montgomery domain
    // takes the result out of Montgomery representation.
    var xR = NewNat().set(Ꮡx).montgomeryRepresentation(Ꮡm);
    // xR = x * R mod m
    return Ꮡx.montgomeryMul(xR, Ꮡy, Ꮡm);
}

// x = xR * y / R mod m

// Exp calculates out = x^e mod m.
//
// The exponent e is represented in big-endian order. The output will be resized
// to the size of m and overwritten. x must already be reduced modulo m.
public static ж<ΔNat> Exp(this ж<ΔNat> Ꮡout, ж<ΔNat> Ꮡx, slice<byte> e, ж<Modulus> Ꮡm) {
    ref var @out = ref Ꮡout.Value;
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    // We use a 4 bit window. For our RSA workload, 4 bit windows are faster
    // than 2 bit windows, but use an extra 12 nats worth of scratch space.
    // Using bit sizes that don't divide 8 are more complex to implement, but
    // are likely to be more efficient if necessary.
    var table = new ж<ΔNat>[]{ // table[i] = x ^ (i+1)
 // newNat calls are unrolled so they are allocated on the stack.

        NewNat(), NewNat(), NewNat(), NewNat(), NewNat(),
        NewNat(), NewNat(), NewNat(), NewNat(), NewNat(),
        NewNat(), NewNat(), NewNat(), NewNat(), NewNat()
    }.array();
    table[0].set(Ꮡx).montgomeryRepresentation(Ꮡm);
    for (nint i = 1; i < len(table); i++) {
        table[i].montgomeryMul(table[i - 1], table[0], Ꮡm);
    }
    Ꮡout.resetFor(Ꮡm);
    @out.limbs[0] = 1;
    Ꮡout.montgomeryRepresentation(Ꮡm);
    var tmp = NewNat().ExpandFor(Ꮡm);
    foreach (var (_, b) in e) {
        foreach (var (_, j) in new nint[]{4, 0}.slice()) {
            // Square four times. Optimization note: this can be implemented
            // more efficiently than with generic Montgomery multiplication.
            Ꮡout.montgomeryMul(Ꮡout, Ꮡout, Ꮡm);
            Ꮡout.montgomeryMul(Ꮡout, Ꮡout, Ꮡm);
            Ꮡout.montgomeryMul(Ꮡout, Ꮡout, Ꮡm);
            Ꮡout.montgomeryMul(Ꮡout, Ꮡout, Ꮡm);
            // Select x^k in constant time from the table.
            nuint k = (nuint)((byte)(((b >> (int)(j))) & 0b1111));
            foreach (var (i, _) in table) {
                tmp.assign(ctEq(k, (nuint)(i + 1)), table[i]);
            }
            // Multiply by x^k, discarding the result if k = 0.
            tmp.montgomeryMul(Ꮡout, tmp, Ꮡm);
            Ꮡout.assign(not(ctEq(k, 0)), tmp);
        }
    }
    return Ꮡout.montgomeryReduction(Ꮡm);
}

// ExpShortVarTime calculates out = x^e mod m.
//
// The output will be resized to the size of m and overwritten. x must already
// be reduced modulo m. This leaks the exponent through timing side-channels.
public static ж<ΔNat> ExpShortVarTime(this ж<ΔNat> Ꮡout, ж<ΔNat> Ꮡx, nuint e, ж<Modulus> Ꮡm) {
    ref var @out = ref Ꮡout.Value;
    ref var x = ref Ꮡx.Value;
    ref var m = ref Ꮡm.Value;

    // For short exponents, precomputing a table and using a window like in Exp
    // doesn't pay off. Instead, we do a simple conditional square-and-multiply
    // chain, skipping the initial run of zeroes.
    var xR = NewNat().set(Ꮡx).montgomeryRepresentation(Ꮡm);
    Ꮡout.set(xR);
    for (nint i = (nint)bits.UintSize - bitLen(e) + 1; i < bits.UintSize; i++) {
        Ꮡout.montgomeryMul(Ꮡout, Ꮡout, Ꮡm);
        {
            nuint k = (nuint)(((e >> (int)(((nint)bits.UintSize - i - 1)))) & 1); if (k != 0) {
                Ꮡout.montgomeryMul(Ꮡout, xR, Ꮡm);
            }
        }
    }
    return Ꮡout.montgomeryReduction(Ꮡm);
}

} // end bigmod_package
