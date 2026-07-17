// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (amd64 || arm64) && !purego
namespace go.crypto.@internal;

using errors = errors_package;

partial class nistec_package {

// Montgomery multiplication modulo org(G). Sets res = in1 * in2 * R⁻¹.
//
//go:noescape
internal static partial void p256OrdMul(ж<p256OrdElement> res, ж<p256OrdElement> in1, ж<p256OrdElement> in2);

// Montgomery square modulo org(G), repeated n times (n >= 1).
//
//go:noescape
internal static partial void p256OrdSqr(ж<p256OrdElement> res, ж<p256OrdElement> @in, nint n);

public static (slice<byte>, error) P256OrdInverse(slice<byte> k) {
    if (len(k) != 32) {
        return (default!, errors.New("invalid scalar length"u8));
    }
    var x = @new<p256OrdElement>();
    p256OrdBigToLittle(x, Ꮡ(new array<byte>(k, 32)));
    p256OrdReduce(x);
    // Inversion is implemented as exponentiation by n - 2, per Fermat's little theorem.
    //
    // The sequence of 38 multiplications and 254 squarings is derived from
    // https://briansmith.org/ecc-inversion-addition-chains-01#p256_scalar_inversion
    var _1 = @new<p256OrdElement>();
    var _11 = @new<p256OrdElement>();
    var _101 = @new<p256OrdElement>();
    var _111 = @new<p256OrdElement>();
    var _1111 = @new<p256OrdElement>();
    var _10101 = @new<p256OrdElement>();
    var _101111 = @new<p256OrdElement>();
    var t = @new<p256OrdElement>();
    // This code operates in the Montgomery domain where R = 2²⁵⁶ mod n and n is
    // the order of the scalar field. Elements in the Montgomery domain take the
    // form a×R and p256OrdMul calculates (a × b × R⁻¹) mod n. RR is R in the
    // domain, or R×R mod n, thus p256OrdMul(x, RR) gives x×R, i.e. converts x
    // into the Montgomery domain.
    var RR = Ꮡ(new p256OrdElement(new uint64[]{0x83244c95be79eea2UL, 0x4699799c49bd6fa6UL,
        0x2845b2392b6bec59UL, 0x66e12d94f3d95620UL}.array()));
    p256OrdMul(_1, x, RR);
    // _1
    p256OrdSqr(x, _1, 1);
    // _10
    p256OrdMul(_11, x, _1);
    // _11
    p256OrdMul(_101, x, _11);
    // _101
    p256OrdMul(_111, x, _101);
    // _111
    p256OrdSqr(x, _101, 1);
    // _1010
    p256OrdMul(_1111, _101, x);
    // _1111
    p256OrdSqr(t, x, 1);
    // _10100
    p256OrdMul(_10101, t, _1);
    // _10101
    p256OrdSqr(x, _10101, 1);
    // _101010
    p256OrdMul(_101111, _101, x);
    // _101111
    p256OrdMul(x, _10101, x);
    // _111111 = x6
    p256OrdSqr(t, x, 2);
    // _11111100
    p256OrdMul(t, t, _11);
    // _11111111 = x8
    p256OrdSqr(x, t, 8);
    // _ff00
    p256OrdMul(x, x, t);
    // _ffff = x16
    p256OrdSqr(t, x, 16);
    // _ffff0000
    p256OrdMul(t, t, x);
    // _ffffffff = x32
    p256OrdSqr(x, t, 64);
    p256OrdMul(x, x, t);
    p256OrdSqr(x, x, 32);
    p256OrdMul(x, x, t);
    var sqrs = new nint[]{
        6, 5, 4, 5, 5,
        4, 3, 3, 5, 9,
        6, 2, 5, 6, 5,
        4, 5, 5, 3, 10,
        2, 5, 5, 3, 7, 6}.slice();
    var muls = new ж<p256OrdElement>[]{
        _101111, _111, _11, _1111, _10101,
        _101, _101, _101, _111, _101111,
        _1111, _1, _1, _1111, _111,
        _111, _111, _101, _11, _101111,
        _11, _11, _11, _1, _10101, _1111}.slice();
    foreach (var (i, s) in sqrs) {
        p256OrdSqr(x, x, s);
        p256OrdMul(x, x, muls[i]);
    }
    // Montgomery multiplication by R⁻¹, or 1 outside the domain as R⁻¹×R = 1,
    // converts a Montgomery value out of the domain.
    var one = Ꮡ(new p256OrdElement(new uint64[]{1}.array()));
    p256OrdMul(x, x, one);
    ref var xOut = ref heap(new array<byte>(32), out var ᏑxOut);
    p256OrdLittleToBig(ᏑxOut, x);
    return (xOut[..], default!);
}

} // end nistec_package
