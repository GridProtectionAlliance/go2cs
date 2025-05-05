// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using errors = errors_package;
using byteorder = @internal.byteorder_package;
using big = math.big_package;
using bits = math.bits_package;
using @internal;
using math;

partial class bigmod_package {

internal static readonly UntypedInt _W = /* bits.UintSize */ 64;
internal static readonly UntypedInt _S = /* _W / 8 */ 8;

[GoType("num:nuint")] partial struct choice;

internal static choice not(choice c) {
    return (choice)(1 ^ c);
}

internal static readonly choice yes = /* choice(1) */ 1;

internal static readonly choice no = /* choice(0) */ 0;

// ctMask is all 1s if on is yes, and all 0s otherwise.
internal static nuint ctMask(choice on) {
    return -((nuint)on);
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
[GoRecv("capture")] internal static ж<ΔNat> expand(this ref ΔNat x, nint n) {
    if (len(x.limbs) > n) {
        throw panic("bigmod: internal error: shrinking nat");
    }
    if (cap(x.limbs) < n) {
        var newLimbs = new slice<nuint>(n);
        copy(newLimbs, x.limbs);
        x.limbs = newLimbs;
        return expandꓸᏑx;
    }
    var extraLimbs = x.limbs[(int)(len(x.limbs))..(int)(n)];
    clear(extraLimbs);
    x.limbs = x.limbs[..(int)(n)];
    return expandꓸᏑx;
}

// reset returns a zero nat of n limbs, reusing x's storage if n <= cap(x.limbs).
[GoRecv("capture")] internal static ж<ΔNat> reset(this ref ΔNat x, nint n) {
    if (cap(x.limbs) < n) {
        x.limbs = new slice<nuint>(n);
        return resetꓸᏑx;
    }
    clear(x.limbs);
    x.limbs = x.limbs[..(int)(n)];
    return resetꓸᏑx;
}

// set assigns x = y, optionally resizing x to the appropriate size.
[GoRecv("capture")] public static ж<ΔNat> set(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    ref var y = ref Ꮡy.val;

    x.reset(len(y.limbs));
    copy(x.limbs, y.limbs);
    return setꓸᏑx;
}

// setBig assigns x = n, optionally resizing n to the appropriate size.
//
// The announced length of x is set based on the actual bit size of the input,
// ignoring leading zeroes.
[GoRecv("capture")] public static ж<ΔNat> setBig(this ref ΔNat x, ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.val;

    var limbs = n.Bits();
    x.reset(len(limbs));
    foreach (var (i, _) in limbs) {
        x.limbs[i] = ((nuint)limbs[i]);
    }
    return setBigꓸᏑx;
}

// Bytes returns x as a zero-extended big-endian byte slice. The size of the
// slice will match the size of m.
//
// x must have the same size as m and it must be reduced modulo m.
[GoRecv] public static slice<byte> Bytes(this ref ΔNat x, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    nint i = m.Size();
    var bytes = new slice<byte>(i);
    foreach (var (_, limb) in x.limbs) {
        for (nint j = 0; j < _S; j++) {
            i--;
            if (i < 0) {
                if (limb == 0) {
                    break;
                }
                throw panic("bigmod: modulus is smaller than nat");
            }
            bytes[i] = ((byte)limb);
            limb >>= (UntypedInt)(8);
        }
    }
    return bytes;
}

// SetBytes assigns x = b, where b is a slice of big-endian bytes.
// SetBytes returns an error if b >= m.
//
// The output will be resized to the size of m and overwritten.
[GoRecv("capture")] public static (ж<ΔNat>, error) SetBytes(this ref ΔNat x, slice<byte> b, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    {
        var err = x.setBytes(b, Ꮡm); if (err != default!) {
            return (default!, err);
        }
    }
    if (x.cmpGeq(m.nat) == yes) {
        return (default!, errors.New("input overflows the modulus"u8));
    }
    return (SetBytesꓸᏑx, default!);
}

// SetOverflowingBytes assigns x = b, where b is a slice of big-endian bytes.
// SetOverflowingBytes returns an error if b has a longer bit length than m, but
// reduces overflowing values up to 2^⌈log2(m)⌉ - 1.
//
// The output will be resized to the size of m and overwritten.
[GoRecv("capture")] public static (ж<ΔNat>, error) SetOverflowingBytes(this ref ΔNat x, slice<byte> b, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    {
        var err = x.setBytes(b, Ꮡm); if (err != default!) {
            return (default!, err);
        }
    }
    nint leading = _W - bitLen(x.limbs[len(x.limbs) - 1]);
    if (leading < m.leading) {
        return (default!, errors.New("input overflows the modulus size"u8));
    }
    x.maybeSubtractModulus(no, Ꮡm);
    return (SetOverflowingBytesꓸᏑx, default!);
}

// bigEndianUint returns the contents of buf interpreted as a
// big-endian encoded uint value.
internal static nuint bigEndianUint(slice<byte> buf) {
    if (_W == 64) {
        return ((nuint)byteorder.BeUint64(buf));
    }
    return ((nuint)byteorder.BeUint32(buf));
}

[GoRecv] public static error setBytes(this ref ΔNat x, slice<byte> b, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    x.resetFor(Ꮡm);
    nint i = len(b);
    nint k = 0;
    while (k < len(x.limbs) && i >= _S) {
        x.limbs[k] = bigEndianUint(b[(int)(i - _S)..(int)(i)]);
        i -= _S;
        k++;
    }
    for (nint s = 0; s < _W && k < len(x.limbs) && i > 0; s += 8) {
        x.limbs[k] |= (nuint)(((nuint)b[i - 1]) << (int)(s));
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
    ref var y = ref Ꮡy.val;

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
[GoRecv] public static choice cmpGeq(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    ref var y = ref Ꮡy.val;

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
[GoRecv("capture")] public static ж<ΔNat> assign(this ref ΔNat x, choice on, ж<ΔNat> Ꮡy) {
    ref var y = ref Ꮡy.val;

    // Eliminate bounds checks in the loop.
    nint size = len(x.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var yLimbs = y.limbs[..(int)(size)];
    nuint mask = ctMask(on);
    for (nint i = 0; i < size; i++) {
        xLimbs[i] ^= (nuint)((nuint)(mask & ((nuint)(xLimbs[i] ^ yLimbs[i]))));
    }
    return assignꓸᏑx;
}

// add computes x += y and returns the carry.
//
// Both operands must have the same announced length.
[GoRecv] public static nuint /*c*/ add(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    nuint c = default!;

    ref var y = ref Ꮡy.val;
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
[GoRecv] public static nuint /*c*/ sub(this ref ΔNat x, ж<ΔNat> Ꮡy) {
    nuint c = default!;

    ref var y = ref Ꮡy.val;
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
    ref var m = ref Ꮡm.val;

    var rr = NewNat().ExpandFor(Ꮡm);
    nuint n = ((nuint)len((~rr).limbs));
    nuint mLen = ((nuint)m.BitLen());
    nuint logR = _W * n;
    // We start by computing R = 2^(_W * n) mod m. We can get pretty close, to
    // 2^⌊log₂m⌋, by setting the highest bit we can without having to reduce.
    (~rr).limbs[n - 1] = 1 << (int)(((mLen - 1) % _W));
    // Then we double until we reach 2^(_W * n).
    for (nuint i = mLen - 1; i < logR; i++) {
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
    while (logR >> (int)(i) <= threshold) {
        i--;
    }
    for (nuint k = ((nuint)0); k < logR >> (int)(i); k++) {
        rr.Add(rr, Ꮡm);
    }
    // Then we process the remaining bits of the exponent with a
    // square-and-double chain.
    while (i > 0) {
        rr.montgomeryMul(rr, rr, Ꮡm);
        i--;
        if ((nuint)(logR >> (int)(i) & 1) != 0) {
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
    return -y;
}

// NewModulusFromBig creates a new Modulus from a [big.Int].
//
// The Int must be odd. The number of significant bits (and nothing else) is
// leaked through timing side-channels.
public static (ж<Modulus>, error) NewModulusFromBig(ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.val;

    {
        var b = n.Bits(); if (len(b) == 0){
            return (default!, errors.New("modulus must be >= 0"u8));
        } else 
        if ((big.Word)(b[0] & 1) != 1) {
            return (default!, errors.New("modulus must be odd"u8));
        }
    }
    var m = Ꮡ(new Modulus(nil));
    m.val.nat = NewNat().setBig(Ꮡn);
    m.val.leading = _W - bitLen((~(~m).nat).limbs[len((~(~m).nat).limbs) - 1]);
    m.val.m0inv = minusInverseModW((~(~m).nat).limbs[0]);
    m.val.rr = rr(m);
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
        n >>= (UntypedInt)(1);
    }
    return len;
}

// Size returns the size of m in bytes.
[GoRecv] public static nint Size(this ref Modulus m) {
    return (m.BitLen() + 7) / 8;
}

// BitLen returns the size of m in bits.
[GoRecv] public static nint BitLen(this ref Modulus m) {
    return len(m.nat.limbs) * _W - ((nint)m.leading);
}

// Nat returns m as a Nat. The return value must not be written to.
[GoRecv] public static ж<ΔNat> Nat(this ref Modulus m) {
    return m.nat;
}

// shiftIn calculates x = x << _W + y mod m.
//
// This assumes that x is already reduced mod m.
[GoRecv] public static ж<ΔNat> shiftIn(this ref ΔNat x, nuint y, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    var d = NewNat().resetFor(Ꮡm);
    // Eliminate bounds checks in the loop.
    nint size = len(m.nat.limbs);
    var xLimbs = x.limbs[..(int)(size)];
    var dLimbs = (~d).limbs[..(int)(size)];
    var mLimbs = m.nat.limbs[..(int)(size)];
    // Each iteration of this loop computes x = 2x + b mod m, where b is a bit
    // from y. Effectively, it left-shifts x and adds y one bit at a time,
    // reducing it every time.
    //
    // To do the reduction, each iteration computes both 2x + b and 2x + b - m.
    // The next iteration (and finally the return line) will use either result
    // based on whether 2x + b overflows m.
    choice needSubtraction = no;
    for (nint i = _W - 1; i >= 0; i--) {
        nuint carry = (nuint)((y >> (int)(i)) & 1);
        nuint borrow = default!;
        nuint mask = ctMask(needSubtraction);
        for (nint iΔ1 = 0; iΔ1 < size; iΔ1++) {
            nuint l = (nuint)(xLimbs[iΔ1] ^ ((nuint)(mask & ((nuint)(xLimbs[iΔ1] ^ dLimbs[iΔ1])))));
            (xLimbs[i], carry) = bits.Add(l, l, carry);
            (dLimbs[i], borrow) = bits.Sub(xLimbs[iΔ1], mLimbs[iΔ1], borrow);
        }
        // Like in maybeSubtractModulus, we need the subtraction if either it
        // didn't underflow (meaning 2x + b > m) or if computing 2x + b
        // overflowed (meaning 2x + b > 2^_W*n > m).
        needSubtraction = (choice)(not(((choice)borrow)) | ((choice)carry));
    }
    return x.assign(needSubtraction, d);
}

// Mod calculates out = x mod m.
//
// This works regardless how large the value of x is.
//
// The output will be resized to the size of m and overwritten.
[GoRecv("capture")] public static ж<ΔNat> Mod(this ref ΔNat @out, ж<ΔNat> Ꮡx, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.val;
    ref var m = ref Ꮡm.val;

    @out.resetFor(Ꮡm);
    // Working our way from the most significant to the least significant limb,
    // we can insert each limb at the least significant position, shifting all
    // previous limbs left by _W. This way each limb will get shifted by the
    // correct number of bits. We can insert at least N - 1 limbs without
    // overflowing m. After that, we need to reduce every time we shift.
    nint i = len(x.limbs) - 1;
    // For the first N - 1 limbs we can skip the actual shifting and position
    // them at the shifted position, which starts at min(N - 2, i).
    nint start = len(m.nat.limbs) - 2;
    if (i < start) {
        start = i;
    }
    for (nint j = start; j >= 0; j--) {
        @out.limbs[j] = x.limbs[i];
        i--;
    }
    // We shift in the remaining limbs, reducing modulo m each time.
    while (i >= 0) {
        @out.shiftIn(x.limbs[i], Ꮡm);
        i--;
    }
    return ModꓸᏑout;
}

// ExpandFor ensures x has the right size to work with operations modulo m.
//
// The announced size of x must be smaller than or equal to that of m.
[GoRecv] public static ж<ΔNat> ExpandFor(this ref ΔNat x, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    return x.expand(len(m.nat.limbs));
}

// resetFor ensures out has the right size to work with operations modulo m.
//
// out is zeroed and may start at any size.
[GoRecv] public static ж<ΔNat> resetFor(this ref ΔNat @out, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    return @out.reset(len(m.nat.limbs));
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
[GoRecv] public static void maybeSubtractModulus(this ref ΔNat x, choice always, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    var t = NewNat().set(x);
    nuint underflow = t.sub(m.nat);
    // We keep the result if x - m didn't underflow (meaning x >= m)
    // or if always was set.
    choice keep = (choice)(not(((choice)underflow)) | ((choice)always));
    x.assign(keep, t);
}

// Sub computes x = x - y mod m.
//
// The length of both operands must be the same as the modulus. Both operands
// must already be reduced modulo m.
[GoRecv("capture")] public static ж<ΔNat> Sub(this ref ΔNat x, ж<ΔNat> Ꮡy, ж<Modulus> Ꮡm) {
    ref var y = ref Ꮡy.val;
    ref var m = ref Ꮡm.val;

    nuint underflow = x.sub(Ꮡy);
    // If the subtraction underflowed, add m.
    var t = NewNat().set(x);
    t.add(m.nat);
    x.assign(((choice)underflow), t);
    return SubꓸᏑx;
}

// Add computes x = x + y mod m.
//
// The length of both operands must be the same as the modulus. Both operands
// must already be reduced modulo m.
[GoRecv("capture")] public static ж<ΔNat> Add(this ref ΔNat x, ж<ΔNat> Ꮡy, ж<Modulus> Ꮡm) {
    ref var y = ref Ꮡy.val;
    ref var m = ref Ꮡm.val;

    nuint overflow = x.add(Ꮡy);
    x.maybeSubtractModulus(((choice)overflow), Ꮡm);
    return AddꓸᏑx;
}

// montgomeryRepresentation calculates x = x * R mod m, with R = 2^(_W * n) and
// n = len(m.nat.limbs).
//
// Faster Montgomery multiplication replaces standard modular multiplication for
// numbers in this representation.
//
// This assumes that x is already reduced mod m.
[GoRecv] public static ж<ΔNat> montgomeryRepresentation(this ref ΔNat x, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    // A Montgomery multiplication (which computes a * b / R) by R * R works out
    // to a multiplication by R, which takes the value out of the Montgomery domain.
    return x.montgomeryMul(x, m.rr, Ꮡm);
}

// montgomeryReduction calculates x = x / R mod m, with R = 2^(_W * n) and
// n = len(m.nat.limbs).
//
// This assumes that x is already reduced mod m.
[GoRecv] public static ж<ΔNat> montgomeryReduction(this ref ΔNat x, ж<Modulus> Ꮡm) {
    ref var m = ref Ꮡm.val;

    // By Montgomery multiplying with 1 not in Montgomery representation, we
    // convert out back from Montgomery representation, because it works out to
    // dividing by R.
    var one = NewNat().ExpandFor(Ꮡm);
    (~one).limbs[0] = 1;
    return x.montgomeryMul(x, one, Ꮡm);
}

// montgomeryMul calculates x = a * b / R mod m, with R = 2^(_W * n) and
// n = len(m.nat.limbs), also known as a Montgomery multiplication.
//
// All inputs should be the same length and already reduced modulo m.
// x will be resized to the size of m and overwritten.
[GoRecv("capture")] public static ж<ΔNat> montgomeryMul(this ref ΔNat x, ж<ΔNat> Ꮡa, ж<ΔNat> Ꮡb, ж<Modulus> Ꮡm) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;
    ref var m = ref Ꮡm.val;

    nint n = len(m.nat.limbs);
    var mLimbs = m.nat.limbs[..(int)(n)];
    var aLimbs = a.limbs[..(int)(n)];
    var bLimbs = b.limbs[..(int)(n)];
    switch (n) {
    default: {
        var T = new slice<nuint>(0, // Attempt to use a stack-allocated backing array.
 preallocLimbs * 2);
        if (cap(T) < n * 2) {
            T = new slice<nuint>(0, n * 2);
        }
        T = T[..(int)(n * 2)];
        // This loop implements Word-by-Word Montgomery Multiplication, as
        // described in Algorithm 4 (Fig. 3) of "Efficient Software
        // Implementations of Modular Exponentiation" by Shay Gueron
        // [https://eprint.iacr.org/2011/239.pdf].
        nuint cΔ4 = default!;
        for (nint i = 0; i < n; i++) {
            _ = T[n + i];
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
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW(T[(int)(i)..(int)(n + i)], aLimbs, d);
            // Step 6 is replaced by shifting the virtual window we operate
            // over: T of the algorithm is T[i:] for us. That means that T1 in
            // Step 2 (T mod 2^_W) is simply T[i]. k0 in Step 3 is our m0inv.
            nuint Y = T[i] * m.m0inv;
            // Step 4 and 5 add Y × m to T, which as mentioned above is stored
            // at T[i:]. The two carries (from a × d and Y × m) are added up in
            // the next word T[n+i], and the carry bit from that addition is
            // brought forward to the next iteration.
            nuint c2 = addMulVVW(T[(int)(i)..(int)(n + i)], mLimbs, Y);
            (T[n + i], c) = bits.Add(c1, c2, cΔ4);
        }
        copy((~x.reset(n)).limbs, // Finally for Step 7 we copy the final T window into x, and subtract m
 // if necessary (which as explained in maybeSubtractModulus can be the
 // case both if x >= m, or if x overflowed).
 //
 // The paper suggests in Section 4 that we can do an "Almost Montgomery
 // Multiplication" by subtracting only in the overflow case, but the
 // cost is very similar since the constant time subtraction tells us if
 // x >= m as a side effect, and taking care of the broken invariant is
 // highly undesirable (see https://go.dev/issue/13907).
 T[(int)(n)..]);
        x.maybeSubtractModulus(((choice)cΔ4), Ꮡm);
        break;
    }
    case 1024 / _W: {
// The following specialized cases follow the exact same algorithm, but
// optimized for the sizes most used in RSA. addMulVVW is implemented in
// assembly with loop unrolling depending on the architecture and bounds
// checks are removed by the compiler thanks to the constant size.
        static readonly UntypedInt n = /* 1024 / _W */ 16; // compiler hint
        var T = new slice<nuint>(n * 2);
        nuint cΔ5 = default!;
        ref var i = ref heap<nint>(out var Ꮡi);
        for (i = 0; i < n; i++) {
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW1024(Ꮡ(T, i), Ꮡ(aLimbs, 0), d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW1024(Ꮡ(T, i), Ꮡ(mLimbs, 0), Y);
            (T[n + i], c) = bits.Add(c1, c2, cΔ5);
        }
        copy((~x.reset(n)).limbs, T[(int)(n)..]);
        x.maybeSubtractModulus(((choice)cΔ5), Ꮡm);
        break;
    }
    case 1536 / _W: {
        static readonly UntypedInt n = /* 1536 / _W */ 24; // compiler hint
        var T = new slice<nuint>(n * 2);
        nuint cΔ6 = default!;
        ref var i = ref heap<nint>(out var Ꮡi);
        for (i = 0; i < n; i++) {
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW1536(Ꮡ(T, i), Ꮡ(aLimbs, 0), d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW1536(Ꮡ(T, i), Ꮡ(mLimbs, 0), Y);
            (T[n + i], c) = bits.Add(c1, c2, cΔ6);
        }
        copy((~x.reset(n)).limbs, T[(int)(n)..]);
        x.maybeSubtractModulus(((choice)cΔ6), Ꮡm);
        break;
    }
    case 2048 / _W: {
        static readonly UntypedInt n = /* 2048 / _W */ 32; // compiler hint
        var T = new slice<nuint>(n * 2);
        nuint c = default!;
        ref var i = ref heap<nint>(out var Ꮡi);
        for (i = 0; i < n; i++) {
            nuint d = bLimbs[i];
            nuint c1 = addMulVVW2048(Ꮡ(T, i), Ꮡ(aLimbs, 0), d);
            nuint Y = T[i] * m.m0inv;
            nuint c2 = addMulVVW2048(Ꮡ(T, i), Ꮡ(mLimbs, 0), Y);
            (T[n + i], c) = bits.Add(c1, c2, c);
        }
        copy((~x.reset(n)).limbs, T[(int)(n)..]);
        x.maybeSubtractModulus(((choice)c), Ꮡm);
        break;
    }}

    return montgomeryMulꓸᏑx;
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
        var (lo, c) = bits.Add(lo, z[i], 0);
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
[GoRecv] public static ж<ΔNat> Mul(this ref ΔNat x, ж<ΔNat> Ꮡy, ж<Modulus> Ꮡm) {
    ref var y = ref Ꮡy.val;
    ref var m = ref Ꮡm.val;

    // A Montgomery multiplication by a value out of the Montgomery domain
    // takes the result out of Montgomery representation.
    var xR = NewNat().set(x).montgomeryRepresentation(Ꮡm);
    // xR = x * R mod m
    return x.montgomeryMul(xR, Ꮡy, Ꮡm);
}

// x = xR * y / R mod m

// Exp calculates out = x^e mod m.
//
// The exponent e is represented in big-endian order. The output will be resized
// to the size of m and overwritten. x must already be reduced modulo m.
[GoRecv] public static ж<ΔNat> Exp(this ref ΔNat @out, ж<ΔNat> Ꮡx, slice<byte> e, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.val;
    ref var m = ref Ꮡm.val;

    // We use a 4 bit window. For our RSA workload, 4 bit windows are faster
    // than 2 bit windows, but use an extra 12 nats worth of scratch space.
    // Using bit sizes that don't divide 8 are more complex to implement, but
    // are likely to be more efficient if necessary.
    ref var table = ref heap<array<ж<ΔNat>>>(out var Ꮡtable);
    table = new ж<ΔNat>[]{ // table[i] = x ^ (i+1)
 // newNat calls are unrolled so they are allocated on the stack.

        NewNat(), NewNat(), NewNat(), NewNat(), NewNat(),
        NewNat(), NewNat(), NewNat(), NewNat(), NewNat(),
        NewNat(), NewNat(), NewNat(), NewNat(), NewNat()
    }.array();
    table[0].set(Ꮡx).montgomeryRepresentation(Ꮡm);
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 1; i < len(table); i++) {
        table[i].montgomeryMul(table[i - 1], table[0], Ꮡm);
    }
    @out.resetFor(Ꮡm);
    @out.limbs[0] = 1;
    @out.montgomeryRepresentation(Ꮡm);
    var tmp = NewNat().ExpandFor(Ꮡm);
    foreach (var (_, b) in e) {
        foreach (var (_, j) in new nint[]{4, 0}.slice()) {
            // Square four times. Optimization note: this can be implemented
            // more efficiently than with generic Montgomery multiplication.
            @out.montgomeryMul(@out, @out, Ꮡm);
            @out.montgomeryMul(@out, @out, Ꮡm);
            @out.montgomeryMul(@out, @out, Ꮡm);
            @out.montgomeryMul(@out, @out, Ꮡm);
            // Select x^k in constant time from the table.
            nuint k = ((nuint)((byte)((b >> (int)(j)) & 15)));
            ref var i = ref heap(new nint(), out var Ꮡi);

            foreach (var (i, _) in table) {
                tmp.assign(ctEq(k, ((nuint)(i + 1))), table[i]);
            }
            // Multiply by x^k, discarding the result if k = 0.
            tmp.montgomeryMul(@out, tmp, Ꮡm);
            @out.assign(not(ctEq(k, 0)), tmp);
        }
    }
    return @out.montgomeryReduction(Ꮡm);
}

// ExpShortVarTime calculates out = x^e mod m.
//
// The output will be resized to the size of m and overwritten. x must already
// be reduced modulo m. This leaks the exponent through timing side-channels.
[GoRecv] public static ж<ΔNat> ExpShortVarTime(this ref ΔNat @out, ж<ΔNat> Ꮡx, nuint e, ж<Modulus> Ꮡm) {
    ref var x = ref Ꮡx.val;
    ref var m = ref Ꮡm.val;

    // For short exponents, precomputing a table and using a window like in Exp
    // doesn't pay off. Instead, we do a simple conditional square-and-multiply
    // chain, skipping the initial run of zeroes.
    var xR = NewNat().set(Ꮡx).montgomeryRepresentation(Ꮡm);
    @out.set(xR);
    for (nint i = bits.UintSize - bitLen(e) + 1; i < bits.UintSize; i++) {
        @out.montgomeryMul(@out, @out, Ꮡm);
        {
            nuint k = (nuint)((e >> (int)((bits.UintSize - i - 1))) & 1); if (k != 0) {
                @out.montgomeryMul(@out, xR, Ꮡm);
            }
        }
    }
    return @out.montgomeryReduction(Ꮡm);
}

} // end bigmod_package
