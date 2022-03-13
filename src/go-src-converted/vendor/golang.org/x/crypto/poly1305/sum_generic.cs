// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file provides the generic implementation of Sum and MAC. Other files
// might provide optimized assembly implementations of some of this code.

// package poly1305 -- go2cs converted at 2022 March 13 06:45:00 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\poly1305\sum_generic.go
namespace go.vendor.golang.org.x.crypto;

using binary = encoding.binary_package;

public static partial class poly1305_package {

// Poly1305 [RFC 7539] is a relatively simple algorithm: the authentication tag
// for a 64 bytes message is approximately
//
//     s + m[0:16] * r⁴ + m[16:32] * r³ + m[32:48] * r² + m[48:64] * r  mod  2¹³⁰ - 5
//
// for some secret r and s. It can be computed sequentially like
//
//     for len(msg) > 0:
//         h += read(msg, 16)
//         h *= r
//         h %= 2¹³⁰ - 5
//     return h + s
//
// All the complexity is about doing performant constant-time math on numbers
// larger than any available numeric type.

private static void sumGeneric(ptr<array<byte>> _addr_@out, slice<byte> msg, ptr<array<byte>> _addr_key) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<byte> key = ref _addr_key.val;

    var h = newMACGeneric(_addr_key);
    h.Write(msg);
    h.Sum(out);
}

private static macGeneric newMACGeneric(ptr<array<byte>> _addr_key) {
    ref array<byte> key = ref _addr_key.val;

    macGeneric m = new macGeneric();
    initialize(_addr_key, _addr_m.macState);
    return m;
}

// macState holds numbers in saturated 64-bit little-endian limbs. That is,
// the value of [x0, x1, x2] is x[0] + x[1] * 2⁶⁴ + x[2] * 2¹²⁸.
private partial struct macState {
    public array<ulong> h; // r and s are the private key components.
    public array<ulong> r;
    public array<ulong> s;
}

private partial struct macGeneric {
    public ref macState macState => ref macState_val;
    public array<byte> buffer;
    public nint offset;
}

// Write splits the incoming message into TagSize chunks, and passes them to
// update. It buffers incomplete chunks.
private static (nint, error) Write(this ptr<macGeneric> _addr_h, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref macGeneric h = ref _addr_h.val;

    var nn = len(p);
    if (h.offset > 0) {
        var n = copy(h.buffer[(int)h.offset..], p);
        if (h.offset + n < TagSize) {
            h.offset += n;
            return (nn, error.As(null!)!);
        }
        p = p[(int)n..];
        h.offset = 0;
        updateGeneric(_addr_h.macState, h.buffer[..]);
    }
    {
        var n__prev1 = n;

        n = len(p) - (len(p) % TagSize);

        if (n > 0) {
            updateGeneric(_addr_h.macState, p[..(int)n]);
            p = p[(int)n..];
        }
        n = n__prev1;

    }
    if (len(p) > 0) {
        h.offset += copy(h.buffer[(int)h.offset..], p);
    }
    return (nn, error.As(null!)!);
}

// Sum flushes the last incomplete chunk from the buffer, if any, and generates
// the MAC output. It does not modify its state, in order to allow for multiple
// calls to Sum, even if no Write is allowed after Sum.
private static void Sum(this ptr<macGeneric> _addr_h, ptr<array<byte>> _addr_@out) {
    ref macGeneric h = ref _addr_h.val;
    ref array<byte> @out = ref _addr_@out.val;

    ref var state = ref heap(h.macState, out ptr<var> _addr_state);
    if (h.offset > 0) {
        updateGeneric(_addr_state, h.buffer[..(int)h.offset]);
    }
    finalize(_addr_out, _addr_state.h, _addr_state.s);
}

// [rMask0, rMask1] is the specified Poly1305 clamping mask in little-endian. It
// clears some bits of the secret coefficient to make it possible to implement
// multiplication more efficiently.
private static readonly nuint rMask0 = 0x0FFFFFFC0FFFFFFF;
private static readonly nuint rMask1 = 0x0FFFFFFC0FFFFFFC;

// initialize loads the 256-bit key into the two 128-bit secret values r and s.
private static void initialize(ptr<array<byte>> _addr_key, ptr<macState> _addr_m) {
    ref array<byte> key = ref _addr_key.val;
    ref macState m = ref _addr_m.val;

    m.r[0] = binary.LittleEndian.Uint64(key[(int)0..(int)8]) & rMask0;
    m.r[1] = binary.LittleEndian.Uint64(key[(int)8..(int)16]) & rMask1;
    m.s[0] = binary.LittleEndian.Uint64(key[(int)16..(int)24]);
    m.s[1] = binary.LittleEndian.Uint64(key[(int)24..(int)32]);
}

// uint128 holds a 128-bit number as two 64-bit limbs, for use with the
// bits.Mul64 and bits.Add64 intrinsics.
private partial struct uint128 {
    public ulong lo;
    public ulong hi;
}

private static uint128 mul64(ulong a, ulong b) {
    var (hi, lo) = bitsMul64(a, b);
    return new uint128(lo,hi);
}

private static uint128 add128(uint128 a, uint128 b) => func((_, panic, _) => {
    var (lo, c) = bitsAdd64(a.lo, b.lo, 0);
    var (hi, c) = bitsAdd64(a.hi, b.hi, c);
    if (c != 0) {
        panic("poly1305: unexpected overflow");
    }
    return new uint128(lo,hi);
});

private static uint128 shiftRightBy2(uint128 a) {
    a.lo = a.lo >> 2 | (a.hi & 3) << 62;
    a.hi = a.hi >> 2;
    return a;
}

// updateGeneric absorbs msg into the state.h accumulator. For each chunk m of
// 128 bits of message, it computes
//
//     h₊ = (h + m) * r  mod  2¹³⁰ - 5
//
// If the msg length is not a multiple of TagSize, it assumes the last
// incomplete chunk is the final one.
private static void updateGeneric(ptr<macState> _addr_state, slice<byte> msg) => func((_, panic, _) => {
    ref macState state = ref _addr_state.val;

    var h0 = state.h[0];
    var h1 = state.h[1];
    var h2 = state.h[2];
    var r0 = state.r[0];
    var r1 = state.r[1];

    while (len(msg) > 0) {
        ulong c = default; 

        // For the first step, h + m, we use a chain of bits.Add64 intrinsics.
        // The resulting value of h might exceed 2¹³⁰ - 5, but will be partially
        // reduced at the end of the multiplication below.
        //
        // The spec requires us to set a bit just above the message size, not to
        // hide leading zeroes. For full chunks, that's 1 << 128, so we can just
        // add 1 to the most significant (2¹²⁸) limb, h2.
        if (len(msg) >= TagSize) {
            h0, c = bitsAdd64(h0, binary.LittleEndian.Uint64(msg[(int)0..(int)8]), 0);
            h1, c = bitsAdd64(h1, binary.LittleEndian.Uint64(msg[(int)8..(int)16]), c);
            h2 += c + 1;

            msg = msg[(int)TagSize..];
        }
        else
 {
            array<byte> buf = new array<byte>(TagSize);
            copy(buf[..], msg);
            buf[len(msg)] = 1;

            h0, c = bitsAdd64(h0, binary.LittleEndian.Uint64(buf[(int)0..(int)8]), 0);
            h1, c = bitsAdd64(h1, binary.LittleEndian.Uint64(buf[(int)8..(int)16]), c);
            h2 += c;

            msg = null;
        }
        var h0r0 = mul64(h0, r0);
        var h1r0 = mul64(h1, r0);
        var h2r0 = mul64(h2, r0);
        var h0r1 = mul64(h0, r1);
        var h1r1 = mul64(h1, r1);
        var h2r1 = mul64(h2, r1); 

        // Since h2 is known to be at most 7 (5 + 1 + 1), and r0 and r1 have their
        // top 4 bits cleared by rMask{0,1}, we know that their product is not going
        // to overflow 64 bits, so we can ignore the high part of the products.
        //
        // This also means that the product doesn't have a fifth limb (t4).
        if (h2r0.hi != 0) {
            panic("poly1305: unexpected overflow");
        }
        if (h2r1.hi != 0) {
            panic("poly1305: unexpected overflow");
        }
        var m0 = h0r0;
        var m1 = add128(h1r0, h0r1); // These two additions don't overflow thanks again
        var m2 = add128(h2r0, h1r1); // to the 4 masked bits at the top of r0 and r1.
        var m3 = h2r1;

        var t0 = m0.lo;
        var (t1, c) = bitsAdd64(m1.lo, m0.hi, 0);
        var (t2, c) = bitsAdd64(m2.lo, m1.hi, c);
        var (t3, _) = bitsAdd64(m3.lo, m2.hi, c); 

        // Now we have the result as 4 64-bit limbs, and we need to reduce it
        // modulo 2¹³⁰ - 5. The special shape of this Crandall prime lets us do
        // a cheap partial reduction according to the reduction identity
        //
        //     c * 2¹³⁰ + n  =  c * 5 + n  mod  2¹³⁰ - 5
        //
        // because 2¹³⁰ = 5 mod 2¹³⁰ - 5. Partial reduction since the result is
        // likely to be larger than 2¹³⁰ - 5, but still small enough to fit the
        // assumptions we make about h in the rest of the code.
        //
        // See also https://speakerdeck.com/gtank/engineering-prime-numbers?slide=23

        // We split the final result at the 2¹³⁰ mark into h and cc, the carry.
        // Note that the carry bits are effectively shifted left by 2, in other
        // words, cc = c * 4 for the c in the reduction identity.
        (h0, h1, h2) = (t0, t1, t2 & maskLow2Bits);        uint128 cc = new uint128(t2&maskNotLow2Bits,t3); 

        // To add c * 5 to h, we first add cc = c * 4, and then add (cc >> 2) = c.

        h0, c = bitsAdd64(h0, cc.lo, 0);
        h1, c = bitsAdd64(h1, cc.hi, c);
        h2 += c;

        cc = shiftRightBy2(cc);

        h0, c = bitsAdd64(h0, cc.lo, 0);
        h1, c = bitsAdd64(h1, cc.hi, c);
        h2 += c; 

        // h2 is at most 3 + 1 + 1 = 5, making the whole of h at most
        //
        //     5 * 2¹²⁸ + (2¹²⁸ - 1) = 6 * 2¹²⁸ - 1
    }

    (state.h[0], state.h[1], state.h[2]) = (h0, h1, h2);
});

private static readonly ulong maskLow2Bits = 0x0000000000000003;
private static readonly ulong maskNotLow2Bits = ~maskLow2Bits;

// select64 returns x if v == 1 and y if v == 0, in constant time.
private static ulong select64(ulong v, ulong x, ulong y) {
    return ~(v - 1) & x | (v - 1) & y;
}

// [p0, p1, p2] is 2¹³⁰ - 5 in little endian order.
private static readonly nuint p0 = 0xFFFFFFFFFFFFFFFB;
private static readonly nuint p1 = 0xFFFFFFFFFFFFFFFF;
private static readonly nuint p2 = 0x0000000000000003;

// finalize completes the modular reduction of h and computes
//
//     out = h + s  mod  2¹²⁸
//
private static void finalize(ptr<array<byte>> _addr_@out, ptr<array<ulong>> _addr_h, ptr<array<ulong>> _addr_s) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<ulong> h = ref _addr_h.val;
    ref array<ulong> s = ref _addr_s.val;

    var h0 = h[0];
    var h1 = h[1];
    var h2 = h[2]; 

    // After the partial reduction in updateGeneric, h might be more than
    // 2¹³⁰ - 5, but will be less than 2 * (2¹³⁰ - 5). To complete the reduction
    // in constant time, we compute t = h - (2¹³⁰ - 5), and select h as the
    // result if the subtraction underflows, and t otherwise.

    var (hMinusP0, b) = bitsSub64(h0, p0, 0);
    var (hMinusP1, b) = bitsSub64(h1, p1, b);
    _, b = bitsSub64(h2, p2, b); 

    // h = h if h < p else h - p
    h0 = select64(b, h0, hMinusP0);
    h1 = select64(b, h1, hMinusP1); 

    // Finally, we compute the last Poly1305 step
    //
    //     tag = h + s  mod  2¹²⁸
    //
    // by just doing a wide addition with the 128 low bits of h and discarding
    // the overflow.
    var (h0, c) = bitsAdd64(h0, s[0], 0);
    h1, _ = bitsAdd64(h1, s[1], c);

    binary.LittleEndian.PutUint64(out[(int)0..(int)8], h0);
    binary.LittleEndian.PutUint64(out[(int)8..(int)16], h1);
}

} // end poly1305_package
