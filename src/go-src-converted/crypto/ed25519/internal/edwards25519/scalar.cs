// Copyright (c) 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package edwards25519 -- go2cs converted at 2022 March 06 22:17:34 UTC
// import "crypto/ed25519/internal/edwards25519" ==> using edwards25519 = go.crypto.ed25519.@internal.edwards25519_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\scalar.go
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;

namespace go.crypto.ed25519.@internal;

public static partial class edwards25519_package {

    // A Scalar is an integer modulo
    //
    //     l = 2^252 + 27742317777372353535851937790883648493
    //
    // which is the prime order of the edwards25519 group.
    //
    // This type works similarly to math/big.Int, and all arguments and
    // receivers are allowed to alias.
    //
    // The zero value is a valid zero element.
public partial struct Scalar {
    public array<byte> s;
}

private static Scalar scZero = new Scalar([32]byte{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});private static Scalar scOne = new Scalar([32]byte{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});private static Scalar scMinusOne = new Scalar([32]byte{236,211,245,92,26,99,18,88,214,156,247,162,222,249,222,20,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,16});

// NewScalar returns a new zero Scalar.
public static ptr<Scalar> NewScalar() {
    return addr(new Scalar());
}

// MultiplyAdd sets s = x * y + z mod l, and returns s.
private static ptr<Scalar> MultiplyAdd(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_x, ptr<Scalar> _addr_y, ptr<Scalar> _addr_z) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar x = ref _addr_x.val;
    ref Scalar y = ref _addr_y.val;
    ref Scalar z = ref _addr_z.val;

    scMulAdd(_addr_s.s, _addr_x.s, _addr_y.s, _addr_z.s);
    return _addr_s!;
}

// Add sets s = x + y mod l, and returns s.
private static ptr<Scalar> Add(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_x, ptr<Scalar> _addr_y) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar x = ref _addr_x.val;
    ref Scalar y = ref _addr_y.val;
 
    // s = 1 * x + y mod l
    scMulAdd(_addr_s.s, _addr_scOne.s, _addr_x.s, _addr_y.s);
    return _addr_s!;

}

// Subtract sets s = x - y mod l, and returns s.
private static ptr<Scalar> Subtract(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_x, ptr<Scalar> _addr_y) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar x = ref _addr_x.val;
    ref Scalar y = ref _addr_y.val;
 
    // s = -1 * y + x mod l
    scMulAdd(_addr_s.s, _addr_scMinusOne.s, _addr_y.s, _addr_x.s);
    return _addr_s!;

}

// Negate sets s = -x mod l, and returns s.
private static ptr<Scalar> Negate(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_x) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar x = ref _addr_x.val;
 
    // s = -1 * x + 0 mod l
    scMulAdd(_addr_s.s, _addr_scMinusOne.s, _addr_x.s, _addr_scZero.s);
    return _addr_s!;

}

// Multiply sets s = x * y mod l, and returns s.
private static ptr<Scalar> Multiply(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_x, ptr<Scalar> _addr_y) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar x = ref _addr_x.val;
    ref Scalar y = ref _addr_y.val;
 
    // s = x * y + 0 mod l
    scMulAdd(_addr_s.s, _addr_x.s, _addr_y.s, _addr_scZero.s);
    return _addr_s!;

}

// Set sets s = x, and returns s.
private static ptr<Scalar> Set(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_x) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar x = ref _addr_x.val;

    s.val = x;
    return _addr_s!;
}

// SetUniformBytes sets s to an uniformly distributed value given 64 uniformly
// distributed random bytes.
private static ptr<Scalar> SetUniformBytes(this ptr<Scalar> _addr_s, slice<byte> x) => func((_, panic, _) => {
    ref Scalar s = ref _addr_s.val;

    if (len(x) != 64) {
        panic("edwards25519: invalid SetUniformBytes input length");
    }
    ref array<byte> wideBytes = ref heap(new array<byte>(64), out ptr<array<byte>> _addr_wideBytes);
    copy(wideBytes[..], x[..]);
    scReduce(_addr_s.s, _addr_wideBytes);
    return _addr_s!;

});

// SetCanonicalBytes sets s = x, where x is a 32-byte little-endian encoding of
// s, and returns s. If x is not a canonical encoding of s, SetCanonicalBytes
// returns nil and an error, and the receiver is unchanged.
private static (ptr<Scalar>, error) SetCanonicalBytes(this ptr<Scalar> _addr_s, slice<byte> x) {
    ptr<Scalar> _p0 = default!;
    error _p0 = default!;
    ref Scalar s = ref _addr_s.val;

    if (len(x) != 32) {
        return (_addr_null!, error.As(errors.New("invalid scalar length"))!);
    }
    ptr<Scalar> ss = addr(new Scalar());
    copy(ss.s[..], x);
    if (!isReduced(ss)) {
        return (_addr_null!, error.As(errors.New("invalid scalar encoding"))!);
    }
    s.s = ss.s;
    return (_addr_s!, error.As(null!)!);

}

// isReduced returns whether the given scalar is reduced modulo l.
private static bool isReduced(ptr<Scalar> _addr_s) {
    ref Scalar s = ref _addr_s.val;

    for (var i = len(s.s) - 1; i >= 0; i--) {

        if (s.s[i] > scMinusOne.s[i]) 
            return false;
        else if (s.s[i] < scMinusOne.s[i]) 
            return true;
        
    }
    return true;

}

// SetBytesWithClamping applies the buffer pruning described in RFC 8032,
// Section 5.1.5 (also known as clamping) and sets s to the result. The input
// must be 32 bytes, and it is not modified.
//
// Note that since Scalar values are always reduced modulo the prime order of
// the curve, the resulting value will not preserve any of the cofactor-clearing
// properties that clamping is meant to provide. It will however work as
// expected as long as it is applied to points on the prime order subgroup, like
// in Ed25519. In fact, it is lost to history why RFC 8032 adopted the
// irrelevant RFC 7748 clamping, but it is now required for compatibility.
private static ptr<Scalar> SetBytesWithClamping(this ptr<Scalar> _addr_s, slice<byte> x) => func((_, panic, _) => {
    ref Scalar s = ref _addr_s.val;
 
    // The description above omits the purpose of the high bits of the clamping
    // for brevity, but those are also lost to reductions, and are also
    // irrelevant to edwards25519 as they protect against a specific
    // implementation bug that was once observed in a generic Montgomery ladder.
    if (len(x) != 32) {
        panic("edwards25519: invalid SetBytesWithClamping input length");
    }
    ref array<byte> wideBytes = ref heap(new array<byte>(64), out ptr<array<byte>> _addr_wideBytes);
    copy(wideBytes[..], x[..]);
    wideBytes[0] &= 248;
    wideBytes[31] &= 63;
    wideBytes[31] |= 64;
    scReduce(_addr_s.s, _addr_wideBytes);
    return _addr_s!;

});

// Bytes returns the canonical 32-byte little-endian encoding of s.
private static slice<byte> Bytes(this ptr<Scalar> _addr_s) {
    ref Scalar s = ref _addr_s.val;

    var buf = make_slice<byte>(32);
    copy(buf, s.s[..]);
    return buf;
}

// Equal returns 1 if s and t are equal, and 0 otherwise.
private static nint Equal(this ptr<Scalar> _addr_s, ptr<Scalar> _addr_t) {
    ref Scalar s = ref _addr_s.val;
    ref Scalar t = ref _addr_t.val;

    return subtle.ConstantTimeCompare(s.s[..], t.s[..]);
}

// scMulAdd and scReduce are ported from the public domain, “ref10”
// implementation of ed25519 from SUPERCOP.

private static long load3(slice<byte> @in) {
    var r = int64(in[0]);
    r |= int64(in[1]) << 8;
    r |= int64(in[2]) << 16;
    return r;
}

private static long load4(slice<byte> @in) {
    var r = int64(in[0]);
    r |= int64(in[1]) << 8;
    r |= int64(in[2]) << 16;
    r |= int64(in[3]) << 24;
    return r;
}

// Input:
//   a[0]+256*a[1]+...+256^31*a[31] = a
//   b[0]+256*b[1]+...+256^31*b[31] = b
//   c[0]+256*c[1]+...+256^31*c[31] = c
//
// Output:
//   s[0]+256*s[1]+...+256^31*s[31] = (ab+c) mod l
//   where l = 2^252 + 27742317777372353535851937790883648493.
private static void scMulAdd(ptr<array<byte>> _addr_s, ptr<array<byte>> _addr_a, ptr<array<byte>> _addr_b, ptr<array<byte>> _addr_c) {
    ref array<byte> s = ref _addr_s.val;
    ref array<byte> a = ref _addr_a.val;
    ref array<byte> b = ref _addr_b.val;
    ref array<byte> c = ref _addr_c.val;

    nint a0 = 2097151 & load3(a[..]);
    nint a1 = 2097151 & (load4(a[(int)2..]) >> 5);
    nint a2 = 2097151 & (load3(a[(int)5..]) >> 2);
    nint a3 = 2097151 & (load4(a[(int)7..]) >> 7);
    nint a4 = 2097151 & (load4(a[(int)10..]) >> 4);
    nint a5 = 2097151 & (load3(a[(int)13..]) >> 1);
    nint a6 = 2097151 & (load4(a[(int)15..]) >> 6);
    nint a7 = 2097151 & (load3(a[(int)18..]) >> 3);
    nint a8 = 2097151 & load3(a[(int)21..]);
    nint a9 = 2097151 & (load4(a[(int)23..]) >> 5);
    nint a10 = 2097151 & (load3(a[(int)26..]) >> 2);
    var a11 = (load4(a[(int)28..]) >> 7);
    nint b0 = 2097151 & load3(b[..]);
    nint b1 = 2097151 & (load4(b[(int)2..]) >> 5);
    nint b2 = 2097151 & (load3(b[(int)5..]) >> 2);
    nint b3 = 2097151 & (load4(b[(int)7..]) >> 7);
    nint b4 = 2097151 & (load4(b[(int)10..]) >> 4);
    nint b5 = 2097151 & (load3(b[(int)13..]) >> 1);
    nint b6 = 2097151 & (load4(b[(int)15..]) >> 6);
    nint b7 = 2097151 & (load3(b[(int)18..]) >> 3);
    nint b8 = 2097151 & load3(b[(int)21..]);
    nint b9 = 2097151 & (load4(b[(int)23..]) >> 5);
    nint b10 = 2097151 & (load3(b[(int)26..]) >> 2);
    var b11 = (load4(b[(int)28..]) >> 7);
    nint c0 = 2097151 & load3(c[..]);
    nint c1 = 2097151 & (load4(c[(int)2..]) >> 5);
    nint c2 = 2097151 & (load3(c[(int)5..]) >> 2);
    nint c3 = 2097151 & (load4(c[(int)7..]) >> 7);
    nint c4 = 2097151 & (load4(c[(int)10..]) >> 4);
    nint c5 = 2097151 & (load3(c[(int)13..]) >> 1);
    nint c6 = 2097151 & (load4(c[(int)15..]) >> 6);
    nint c7 = 2097151 & (load3(c[(int)18..]) >> 3);
    nint c8 = 2097151 & load3(c[(int)21..]);
    nint c9 = 2097151 & (load4(c[(int)23..]) >> 5);
    nint c10 = 2097151 & (load3(c[(int)26..]) >> 2);
    var c11 = (load4(c[(int)28..]) >> 7);
    array<long> carry = new array<long>(23);

    var s0 = c0 + a0 * b0;
    var s1 = c1 + a0 * b1 + a1 * b0;
    var s2 = c2 + a0 * b2 + a1 * b1 + a2 * b0;
    var s3 = c3 + a0 * b3 + a1 * b2 + a2 * b1 + a3 * b0;
    var s4 = c4 + a0 * b4 + a1 * b3 + a2 * b2 + a3 * b1 + a4 * b0;
    var s5 = c5 + a0 * b5 + a1 * b4 + a2 * b3 + a3 * b2 + a4 * b1 + a5 * b0;
    var s6 = c6 + a0 * b6 + a1 * b5 + a2 * b4 + a3 * b3 + a4 * b2 + a5 * b1 + a6 * b0;
    var s7 = c7 + a0 * b7 + a1 * b6 + a2 * b5 + a3 * b4 + a4 * b3 + a5 * b2 + a6 * b1 + a7 * b0;
    var s8 = c8 + a0 * b8 + a1 * b7 + a2 * b6 + a3 * b5 + a4 * b4 + a5 * b3 + a6 * b2 + a7 * b1 + a8 * b0;
    var s9 = c9 + a0 * b9 + a1 * b8 + a2 * b7 + a3 * b6 + a4 * b5 + a5 * b4 + a6 * b3 + a7 * b2 + a8 * b1 + a9 * b0;
    var s10 = c10 + a0 * b10 + a1 * b9 + a2 * b8 + a3 * b7 + a4 * b6 + a5 * b5 + a6 * b4 + a7 * b3 + a8 * b2 + a9 * b1 + a10 * b0;
    var s11 = c11 + a0 * b11 + a1 * b10 + a2 * b9 + a3 * b8 + a4 * b7 + a5 * b6 + a6 * b5 + a7 * b4 + a8 * b3 + a9 * b2 + a10 * b1 + a11 * b0;
    var s12 = a1 * b11 + a2 * b10 + a3 * b9 + a4 * b8 + a5 * b7 + a6 * b6 + a7 * b5 + a8 * b4 + a9 * b3 + a10 * b2 + a11 * b1;
    var s13 = a2 * b11 + a3 * b10 + a4 * b9 + a5 * b8 + a6 * b7 + a7 * b6 + a8 * b5 + a9 * b4 + a10 * b3 + a11 * b2;
    var s14 = a3 * b11 + a4 * b10 + a5 * b9 + a6 * b8 + a7 * b7 + a8 * b6 + a9 * b5 + a10 * b4 + a11 * b3;
    var s15 = a4 * b11 + a5 * b10 + a6 * b9 + a7 * b8 + a8 * b7 + a9 * b6 + a10 * b5 + a11 * b4;
    var s16 = a5 * b11 + a6 * b10 + a7 * b9 + a8 * b8 + a9 * b7 + a10 * b6 + a11 * b5;
    var s17 = a6 * b11 + a7 * b10 + a8 * b9 + a9 * b8 + a10 * b7 + a11 * b6;
    var s18 = a7 * b11 + a8 * b10 + a9 * b9 + a10 * b8 + a11 * b7;
    var s19 = a8 * b11 + a9 * b10 + a10 * b9 + a11 * b8;
    var s20 = a9 * b11 + a10 * b10 + a11 * b9;
    var s21 = a10 * b11 + a11 * b10;
    var s22 = a11 * b11;
    var s23 = int64(0);

    carry[0] = (s0 + (1 << 20)) >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[2] = (s2 + (1 << 20)) >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[4] = (s4 + (1 << 20)) >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[12] = (s12 + (1 << 20)) >> 21;
    s13 += carry[12];
    s12 -= carry[12] << 21;
    carry[14] = (s14 + (1 << 20)) >> 21;
    s15 += carry[14];
    s14 -= carry[14] << 21;
    carry[16] = (s16 + (1 << 20)) >> 21;
    s17 += carry[16];
    s16 -= carry[16] << 21;
    carry[18] = (s18 + (1 << 20)) >> 21;
    s19 += carry[18];
    s18 -= carry[18] << 21;
    carry[20] = (s20 + (1 << 20)) >> 21;
    s21 += carry[20];
    s20 -= carry[20] << 21;
    carry[22] = (s22 + (1 << 20)) >> 21;
    s23 += carry[22];
    s22 -= carry[22] << 21;

    carry[1] = (s1 + (1 << 20)) >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[3] = (s3 + (1 << 20)) >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[5] = (s5 + (1 << 20)) >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;
    carry[13] = (s13 + (1 << 20)) >> 21;
    s14 += carry[13];
    s13 -= carry[13] << 21;
    carry[15] = (s15 + (1 << 20)) >> 21;
    s16 += carry[15];
    s15 -= carry[15] << 21;
    carry[17] = (s17 + (1 << 20)) >> 21;
    s18 += carry[17];
    s17 -= carry[17] << 21;
    carry[19] = (s19 + (1 << 20)) >> 21;
    s20 += carry[19];
    s19 -= carry[19] << 21;
    carry[21] = (s21 + (1 << 20)) >> 21;
    s22 += carry[21];
    s21 -= carry[21] << 21;

    s11 += s23 * 666643;
    s12 += s23 * 470296;
    s13 += s23 * 654183;
    s14 -= s23 * 997805;
    s15 += s23 * 136657;
    s16 -= s23 * 683901;
    s23 = 0;

    s10 += s22 * 666643;
    s11 += s22 * 470296;
    s12 += s22 * 654183;
    s13 -= s22 * 997805;
    s14 += s22 * 136657;
    s15 -= s22 * 683901;
    s22 = 0;

    s9 += s21 * 666643;
    s10 += s21 * 470296;
    s11 += s21 * 654183;
    s12 -= s21 * 997805;
    s13 += s21 * 136657;
    s14 -= s21 * 683901;
    s21 = 0;

    s8 += s20 * 666643;
    s9 += s20 * 470296;
    s10 += s20 * 654183;
    s11 -= s20 * 997805;
    s12 += s20 * 136657;
    s13 -= s20 * 683901;
    s20 = 0;

    s7 += s19 * 666643;
    s8 += s19 * 470296;
    s9 += s19 * 654183;
    s10 -= s19 * 997805;
    s11 += s19 * 136657;
    s12 -= s19 * 683901;
    s19 = 0;

    s6 += s18 * 666643;
    s7 += s18 * 470296;
    s8 += s18 * 654183;
    s9 -= s18 * 997805;
    s10 += s18 * 136657;
    s11 -= s18 * 683901;
    s18 = 0;

    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[12] = (s12 + (1 << 20)) >> 21;
    s13 += carry[12];
    s12 -= carry[12] << 21;
    carry[14] = (s14 + (1 << 20)) >> 21;
    s15 += carry[14];
    s14 -= carry[14] << 21;
    carry[16] = (s16 + (1 << 20)) >> 21;
    s17 += carry[16];
    s16 -= carry[16] << 21;

    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;
    carry[13] = (s13 + (1 << 20)) >> 21;
    s14 += carry[13];
    s13 -= carry[13] << 21;
    carry[15] = (s15 + (1 << 20)) >> 21;
    s16 += carry[15];
    s15 -= carry[15] << 21;

    s5 += s17 * 666643;
    s6 += s17 * 470296;
    s7 += s17 * 654183;
    s8 -= s17 * 997805;
    s9 += s17 * 136657;
    s10 -= s17 * 683901;
    s17 = 0;

    s4 += s16 * 666643;
    s5 += s16 * 470296;
    s6 += s16 * 654183;
    s7 -= s16 * 997805;
    s8 += s16 * 136657;
    s9 -= s16 * 683901;
    s16 = 0;

    s3 += s15 * 666643;
    s4 += s15 * 470296;
    s5 += s15 * 654183;
    s6 -= s15 * 997805;
    s7 += s15 * 136657;
    s8 -= s15 * 683901;
    s15 = 0;

    s2 += s14 * 666643;
    s3 += s14 * 470296;
    s4 += s14 * 654183;
    s5 -= s14 * 997805;
    s6 += s14 * 136657;
    s7 -= s14 * 683901;
    s14 = 0;

    s1 += s13 * 666643;
    s2 += s13 * 470296;
    s3 += s13 * 654183;
    s4 -= s13 * 997805;
    s5 += s13 * 136657;
    s6 -= s13 * 683901;
    s13 = 0;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = (s0 + (1 << 20)) >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[2] = (s2 + (1 << 20)) >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[4] = (s4 + (1 << 20)) >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    carry[1] = (s1 + (1 << 20)) >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[3] = (s3 + (1 << 20)) >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[5] = (s5 + (1 << 20)) >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[11] = s11 >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    s[0] = byte(s0 >> 0);
    s[1] = byte(s0 >> 8);
    s[2] = byte((s0 >> 16) | (s1 << 5));
    s[3] = byte(s1 >> 3);
    s[4] = byte(s1 >> 11);
    s[5] = byte((s1 >> 19) | (s2 << 2));
    s[6] = byte(s2 >> 6);
    s[7] = byte((s2 >> 14) | (s3 << 7));
    s[8] = byte(s3 >> 1);
    s[9] = byte(s3 >> 9);
    s[10] = byte((s3 >> 17) | (s4 << 4));
    s[11] = byte(s4 >> 4);
    s[12] = byte(s4 >> 12);
    s[13] = byte((s4 >> 20) | (s5 << 1));
    s[14] = byte(s5 >> 7);
    s[15] = byte((s5 >> 15) | (s6 << 6));
    s[16] = byte(s6 >> 2);
    s[17] = byte(s6 >> 10);
    s[18] = byte((s6 >> 18) | (s7 << 3));
    s[19] = byte(s7 >> 5);
    s[20] = byte(s7 >> 13);
    s[21] = byte(s8 >> 0);
    s[22] = byte(s8 >> 8);
    s[23] = byte((s8 >> 16) | (s9 << 5));
    s[24] = byte(s9 >> 3);
    s[25] = byte(s9 >> 11);
    s[26] = byte((s9 >> 19) | (s10 << 2));
    s[27] = byte(s10 >> 6);
    s[28] = byte((s10 >> 14) | (s11 << 7));
    s[29] = byte(s11 >> 1);
    s[30] = byte(s11 >> 9);
    s[31] = byte(s11 >> 17);
}

// Input:
//   s[0]+256*s[1]+...+256^63*s[63] = s
//
// Output:
//   s[0]+256*s[1]+...+256^31*s[31] = s mod l
//   where l = 2^252 + 27742317777372353535851937790883648493.
private static void scReduce(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_s) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<byte> s = ref _addr_s.val;

    nint s0 = 2097151 & load3(s[..]);
    nint s1 = 2097151 & (load4(s[(int)2..]) >> 5);
    nint s2 = 2097151 & (load3(s[(int)5..]) >> 2);
    nint s3 = 2097151 & (load4(s[(int)7..]) >> 7);
    nint s4 = 2097151 & (load4(s[(int)10..]) >> 4);
    nint s5 = 2097151 & (load3(s[(int)13..]) >> 1);
    nint s6 = 2097151 & (load4(s[(int)15..]) >> 6);
    nint s7 = 2097151 & (load3(s[(int)18..]) >> 3);
    nint s8 = 2097151 & load3(s[(int)21..]);
    nint s9 = 2097151 & (load4(s[(int)23..]) >> 5);
    nint s10 = 2097151 & (load3(s[(int)26..]) >> 2);
    nint s11 = 2097151 & (load4(s[(int)28..]) >> 7);
    nint s12 = 2097151 & (load4(s[(int)31..]) >> 4);
    nint s13 = 2097151 & (load3(s[(int)34..]) >> 1);
    nint s14 = 2097151 & (load4(s[(int)36..]) >> 6);
    nint s15 = 2097151 & (load3(s[(int)39..]) >> 3);
    nint s16 = 2097151 & load3(s[(int)42..]);
    nint s17 = 2097151 & (load4(s[(int)44..]) >> 5);
    nint s18 = 2097151 & (load3(s[(int)47..]) >> 2);
    nint s19 = 2097151 & (load4(s[(int)49..]) >> 7);
    nint s20 = 2097151 & (load4(s[(int)52..]) >> 4);
    nint s21 = 2097151 & (load3(s[(int)55..]) >> 1);
    nint s22 = 2097151 & (load4(s[(int)57..]) >> 6);
    var s23 = (load4(s[(int)60..]) >> 3);

    s11 += s23 * 666643;
    s12 += s23 * 470296;
    s13 += s23 * 654183;
    s14 -= s23 * 997805;
    s15 += s23 * 136657;
    s16 -= s23 * 683901;
    s23 = 0;

    s10 += s22 * 666643;
    s11 += s22 * 470296;
    s12 += s22 * 654183;
    s13 -= s22 * 997805;
    s14 += s22 * 136657;
    s15 -= s22 * 683901;
    s22 = 0;

    s9 += s21 * 666643;
    s10 += s21 * 470296;
    s11 += s21 * 654183;
    s12 -= s21 * 997805;
    s13 += s21 * 136657;
    s14 -= s21 * 683901;
    s21 = 0;

    s8 += s20 * 666643;
    s9 += s20 * 470296;
    s10 += s20 * 654183;
    s11 -= s20 * 997805;
    s12 += s20 * 136657;
    s13 -= s20 * 683901;
    s20 = 0;

    s7 += s19 * 666643;
    s8 += s19 * 470296;
    s9 += s19 * 654183;
    s10 -= s19 * 997805;
    s11 += s19 * 136657;
    s12 -= s19 * 683901;
    s19 = 0;

    s6 += s18 * 666643;
    s7 += s18 * 470296;
    s8 += s18 * 654183;
    s9 -= s18 * 997805;
    s10 += s18 * 136657;
    s11 -= s18 * 683901;
    s18 = 0;

    array<long> carry = new array<long>(17);

    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[12] = (s12 + (1 << 20)) >> 21;
    s13 += carry[12];
    s12 -= carry[12] << 21;
    carry[14] = (s14 + (1 << 20)) >> 21;
    s15 += carry[14];
    s14 -= carry[14] << 21;
    carry[16] = (s16 + (1 << 20)) >> 21;
    s17 += carry[16];
    s16 -= carry[16] << 21;

    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;
    carry[13] = (s13 + (1 << 20)) >> 21;
    s14 += carry[13];
    s13 -= carry[13] << 21;
    carry[15] = (s15 + (1 << 20)) >> 21;
    s16 += carry[15];
    s15 -= carry[15] << 21;

    s5 += s17 * 666643;
    s6 += s17 * 470296;
    s7 += s17 * 654183;
    s8 -= s17 * 997805;
    s9 += s17 * 136657;
    s10 -= s17 * 683901;
    s17 = 0;

    s4 += s16 * 666643;
    s5 += s16 * 470296;
    s6 += s16 * 654183;
    s7 -= s16 * 997805;
    s8 += s16 * 136657;
    s9 -= s16 * 683901;
    s16 = 0;

    s3 += s15 * 666643;
    s4 += s15 * 470296;
    s5 += s15 * 654183;
    s6 -= s15 * 997805;
    s7 += s15 * 136657;
    s8 -= s15 * 683901;
    s15 = 0;

    s2 += s14 * 666643;
    s3 += s14 * 470296;
    s4 += s14 * 654183;
    s5 -= s14 * 997805;
    s6 += s14 * 136657;
    s7 -= s14 * 683901;
    s14 = 0;

    s1 += s13 * 666643;
    s2 += s13 * 470296;
    s3 += s13 * 654183;
    s4 -= s13 * 997805;
    s5 += s13 * 136657;
    s6 -= s13 * 683901;
    s13 = 0;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = (s0 + (1 << 20)) >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[2] = (s2 + (1 << 20)) >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[4] = (s4 + (1 << 20)) >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    carry[1] = (s1 + (1 << 20)) >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[3] = (s3 + (1 << 20)) >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[5] = (s5 + (1 << 20)) >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[11] = s11 >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    out[0] = byte(s0 >> 0);
    out[1] = byte(s0 >> 8);
    out[2] = byte((s0 >> 16) | (s1 << 5));
    out[3] = byte(s1 >> 3);
    out[4] = byte(s1 >> 11);
    out[5] = byte((s1 >> 19) | (s2 << 2));
    out[6] = byte(s2 >> 6);
    out[7] = byte((s2 >> 14) | (s3 << 7));
    out[8] = byte(s3 >> 1);
    out[9] = byte(s3 >> 9);
    out[10] = byte((s3 >> 17) | (s4 << 4));
    out[11] = byte(s4 >> 4);
    out[12] = byte(s4 >> 12);
    out[13] = byte((s4 >> 20) | (s5 << 1));
    out[14] = byte(s5 >> 7);
    out[15] = byte((s5 >> 15) | (s6 << 6));
    out[16] = byte(s6 >> 2);
    out[17] = byte(s6 >> 10);
    out[18] = byte((s6 >> 18) | (s7 << 3));
    out[19] = byte(s7 >> 5);
    out[20] = byte(s7 >> 13);
    out[21] = byte(s8 >> 0);
    out[22] = byte(s8 >> 8);
    out[23] = byte((s8 >> 16) | (s9 << 5));
    out[24] = byte(s9 >> 3);
    out[25] = byte(s9 >> 11);
    out[26] = byte((s9 >> 19) | (s10 << 2));
    out[27] = byte(s10 >> 6);
    out[28] = byte((s10 >> 14) | (s11 << 7));
    out[29] = byte(s11 >> 1);
    out[30] = byte(s11 >> 9);
    out[31] = byte(s11 >> 17);
}

// nonAdjacentForm computes a width-w non-adjacent form for this scalar.
//
// w must be between 2 and 8, or nonAdjacentForm will panic.
private static array<sbyte> nonAdjacentForm(this ptr<Scalar> _addr_s, nuint w) => func((_, panic, _) => {
    ref Scalar s = ref _addr_s.val;
 
    // This implementation is adapted from the one
    // in curve25519-dalek and is documented there:
    // https://github.com/dalek-cryptography/curve25519-dalek/blob/f630041af28e9a405255f98a8a93adca18e4315b/src/scalar.rs#L800-L871
    if (s.s[31] > 127) {
        panic("scalar has high bit set illegally");
    }
    if (w < 2) {
        panic("w must be at least 2 by the definition of NAF");
    }
    else if (w > 8) {
        panic("NAF digits must fit in int8");
    }
    array<sbyte> naf = new array<sbyte>(256);
    array<ulong> digits = new array<ulong>(5);

    for (nint i = 0; i < 4; i++) {
        digits[i] = binary.LittleEndian.Uint64(s.s[(int)i * 8..]);
    }

    var width = uint64(1 << (int)(w));
    var windowMask = uint64(width - 1);

    var pos = uint(0);
    var carry = uint64(0);
    while (pos < 256) {
        var indexU64 = pos / 64;
        var indexBit = pos % 64;
        ulong bitBuf = default;
        if (indexBit < 64 - w) { 
            // This window's bits are contained in a single u64
            bitBuf = digits[indexU64] >> (int)(indexBit);

        }
        else
 { 
            // Combine the current 64 bits with bits from the next 64
            bitBuf = (digits[indexU64] >> (int)(indexBit)) | (digits[1 + indexU64] << (int)((64 - indexBit)));

        }
        var window = carry + (bitBuf & windowMask);

        if (window & 1 == 0) { 
            // If the window value is even, preserve the carry and continue.
            // Why is the carry preserved?
            // If carry == 0 and window & 1 == 0,
            //    then the next carry should be 0
            // If carry == 1 and window & 1 == 0,
            //    then bit_buf & 1 == 1 so the next carry should be 1
            pos += 1;
            continue;

        }
        if (window < width / 2) {
            carry = 0;
            naf[pos] = int8(window);
        }
        else
 {
            carry = 1;
            naf[pos] = int8(window) - int8(width);
        }
        pos += w;

    }
    return naf;

});

private static array<sbyte> signedRadix16(this ptr<Scalar> _addr_s) => func((_, panic, _) => {
    ref Scalar s = ref _addr_s.val;

    if (s.s[31] > 127) {
        panic("scalar has high bit set illegally");
    }
    array<sbyte> digits = new array<sbyte>(64); 

    // Compute unsigned radix-16 digits:
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 32; i++) {
            digits[2 * i] = int8(s.s[i] & 15);
            digits[2 * i + 1] = int8((s.s[i] >> 4) & 15);
        }

        i = i__prev1;
    } 

    // Recenter coefficients:
    {
        nint i__prev1 = i;

        for (i = 0; i < 63; i++) {
            var carry = (digits[i] + 8) >> 4;
            digits[i] -= carry << 4;
            digits[i + 1] += carry;
        }

        i = i__prev1;
    }

    return digits;

});

} // end edwards25519_package
