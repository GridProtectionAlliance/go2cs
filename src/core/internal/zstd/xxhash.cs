// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using binary = encoding.binary_package;
using bits = math.bits_package;
using encoding;
using math;

partial class zstd_package {

internal static readonly UntypedInt xxhPrime64c1 = /* 0x9e3779b185ebca87 */ 11400714785074694791;
internal static readonly UntypedInt xxhPrime64c2 = /* 0xc2b2ae3d27d4eb4f */ 14029467366897019727;
internal static readonly UntypedInt xxhPrime64c3 = /* 0x165667b19e3779f9 */ 1609587929392839161;
internal static readonly UntypedInt xxhPrime64c4 = /* 0x85ebca77c2b2ae63 */ 9650029242287828579;
internal static readonly UntypedInt xxhPrime64c5 = /* 0x27d4eb2f165667c5 */ 2870177450012600261;

// xxhash64 is the state of a xxHash-64 checksum.
[GoType] partial struct xxhash64 {
    internal uint64 len;    // total length hashed
    internal array<uint64> v = new(4); // accumulators
    internal array<byte> buf = new(32); // buffer
    internal nint cnt;      // number of bytes in buffer
}

// reset discards the current state and prepares to compute a new hash.
// We assume a seed of 0 since that is what zstd uses.
[GoRecv] internal static void reset(this ref xxhash64 xh) {
    xh.len = 0;
    // Separate addition for awkward constant overflow.
    xh.v[0] = xxhPrime64c1;
    xh.v[0] += xxhPrime64c2;
    xh.v[1] = xxhPrime64c2;
    xh.v[2] = 0;
    // Separate negation for awkward constant overflow.
    xh.v[3] = xxhPrime64c1;
    xh.v[3] = -xh.v[3];
    foreach (var (i, _) in xh.buf) {
        xh.buf[i] = 0;
    }
    xh.cnt = 0;
}

// update adds a buffer to the has.
[GoRecv] internal static void update(this ref xxhash64 xh, slice<byte> b) {
    xh.len += ((uint64)len(b));
    if (xh.cnt + len(b) < len(xh.buf)) {
        copy(xh.buf[(int)(xh.cnt)..], b);
        xh.cnt += len(b);
        return;
    }
    if (xh.cnt > 0) {
        nint n = copy(xh.buf[(int)(xh.cnt)..], b);
        b = b[(int)(n)..];
        xh.v[0] = xh.round(xh.v[0], binary.LittleEndian.Uint64(xh.buf[..]));
        xh.v[1] = xh.round(xh.v[1], binary.LittleEndian.Uint64(xh.buf[8..]));
        xh.v[2] = xh.round(xh.v[2], binary.LittleEndian.Uint64(xh.buf[16..]));
        xh.v[3] = xh.round(xh.v[3], binary.LittleEndian.Uint64(xh.buf[24..]));
        xh.cnt = 0;
    }
    while (len(b) >= 32) {
        xh.v[0] = xh.round(xh.v[0], binary.LittleEndian.Uint64(b));
        xh.v[1] = xh.round(xh.v[1], binary.LittleEndian.Uint64(b[8..]));
        xh.v[2] = xh.round(xh.v[2], binary.LittleEndian.Uint64(b[16..]));
        xh.v[3] = xh.round(xh.v[3], binary.LittleEndian.Uint64(b[24..]));
        b = b[32..];
    }
    if (len(b) > 0) {
        copy(xh.buf[..], b);
        xh.cnt = len(b);
    }
}

// digest returns the final hash value.
[GoRecv] internal static uint64 digest(this ref xxhash64 xh) {
    uint64 h64 = default!;
    if (xh.len < 32){
        h64 = xh.v[2] + xxhPrime64c5;
    } else {
        h64 = bits.RotateLeft64(xh.v[0], 1) + bits.RotateLeft64(xh.v[1], 7) + bits.RotateLeft64(xh.v[2], 12) + bits.RotateLeft64(xh.v[3], 18);
        h64 = xh.mergeRound(h64, xh.v[0]);
        h64 = xh.mergeRound(h64, xh.v[1]);
        h64 = xh.mergeRound(h64, xh.v[2]);
        h64 = xh.mergeRound(h64, xh.v[3]);
    }
    h64 += xh.len;
    var len = xh.len;
    len &= (uint64)(31);
    var buf = xh.buf[..];
    while (len >= 8) {
        var k1 = xh.round(0, binary.LittleEndian.Uint64(buf));
        buf = buf[8..];
        h64 ^= (uint64)(k1);
        h64 = bits.RotateLeft64(h64, 27) * xxhPrime64c1 + xxhPrime64c4;
        len -= 8;
    }
    if (len >= 4) {
        h64 ^= (uint64)(((uint64)binary.LittleEndian.Uint32(buf)) * xxhPrime64c1);
        buf = buf[4..];
        h64 = bits.RotateLeft64(h64, 23) * xxhPrime64c2 + xxhPrime64c3;
        len -= 4;
    }
    while (len > 0) {
        h64 ^= (uint64)(((uint64)buf[0]) * xxhPrime64c5);
        buf = buf[1..];
        h64 = bits.RotateLeft64(h64, 11) * xxhPrime64c1;
        len--;
    }
    h64 ^= (uint64)(h64 >> (int)(33));
    h64 *= xxhPrime64c2;
    h64 ^= (uint64)(h64 >> (int)(29));
    h64 *= xxhPrime64c3;
    h64 ^= (uint64)(h64 >> (int)(32));
    return h64;
}

// round updates a value.
[GoRecv] internal static uint64 round(this ref xxhash64 xh, uint64 v, uint64 n) {
    v += n * xxhPrime64c2;
    v = bits.RotateLeft64(v, 31);
    v *= xxhPrime64c1;
    return v;
}

// mergeRound updates a value in the final round.
[GoRecv] internal static uint64 mergeRound(this ref xxhash64 xh, uint64 v, uint64 n) {
    n = xh.round(0, n);
    v ^= (uint64)(n);
    v = v * xxhPrime64c1 + xxhPrime64c4;
    return v;
}

} // end zstd_package
