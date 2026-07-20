// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build purego
namespace go.hash;

using rand = crypto.rand_package;
using byteorder = @internal.byteorder_package;
using bits = math.bits_package;
using @internal;
using crypto;
using math;

partial class maphash_package {

internal static uint64 rthash(slice<byte> buf, uint64 seed) {
    if (len(buf) == 0) {
        return seed;
    }
    return wyhash(buf, seed, (uint64)len(buf));
}

internal static uint64 rthashString(@string s, uint64 state) {
    return rthash(slice<byte>(s), state);
}

internal static uint64 randUint64() {
    var buf = new slice<byte>(8);
    (_, _) = rand.Read(buf);
    return byteorder.LeUint64(buf);
}

// This is a port of wyhash implementation in runtime/hash64.go,
// without using unsafe for purego.
internal static readonly UntypedInt m1 = 0xa0761d6478bd642f;
internal static readonly UntypedInt m2 = 0xe7037ed1a0b428db;
internal static readonly UntypedInt m3 = 0x8ebc6af09c88c6e3;
internal static readonly UntypedInt m4 = 0x589965cc75374cc3;
internal static readonly UntypedInt m5 = 0x1d8e4e27c47d124f;

internal static uint64 wyhash(slice<byte> key, uint64 seed, uint64 len) {
    var p = key;
    var i = len;
    uint64 a = default!;
    uint64 b = default!;
    seed ^= (uint64)(m1);
    if (i > 16) {
        if (i > 48) {
            var seed1 = seed;
            var seed2 = seed;
            for (; i > 48; i -= 48) {
                seed = mix((uint64)(r8(p) ^ (uint64)m2), (uint64)(r8(p[8..]) ^ seed));
                seed1 = mix((uint64)(r8(p[16..]) ^ (uint64)m3), (uint64)(r8(p[24..]) ^ seed1));
                seed2 = mix((uint64)(r8(p[32..]) ^ (uint64)m4), (uint64)(r8(p[40..]) ^ seed2));
                p = p[48..];
            }
            seed ^= (uint64)((uint64)(seed1 ^ seed2));
        }
        for (; i > 16; i -= 16) {
            seed = mix((uint64)(r8(p) ^ (uint64)m2), (uint64)(r8(p[8..]) ^ seed));
            p = p[16..];
        }
    }
    switch (ᐧ) {
    case {} when i is 0: {
        return seed;
    }
    case {} when i is < 4: {
        a = r3(p, i);
        break;
    }
    default: {
        var n = (((i >> (int)(3))) << (int)(2));
        a = (uint64)((r4(p) << (int)(32)) | r4(p[(int)(n)..]));
        b = (uint64)((r4(p[(int)(i - 4)..]) << (int)(32)) | r4(p[(int)(i - 4 - n)..]));
        break;
    }}

    return mix((uint64)((uint64)m5 ^ len), mix((uint64)(a ^ (uint64)m2), (uint64)(b ^ seed)));
}

internal static uint64 r3(slice<byte> p, uint64 k) {
    return (uint64)((uint64)((((uint64)p[0] << (int)(16))) | (((uint64)p[(nint)((k >> (int)(1)))] << (int)(8)))) | (uint64)p[(nint)(k - 1)]);
}

internal static uint64 r4(slice<byte> p) {
    return (uint64)byteorder.LeUint32(p);
}

internal static uint64 r8(slice<byte> p) {
    return byteorder.LeUint64(p);
}

internal static uint64 mix(uint64 a, uint64 b) {
    var (hi, lo) = bits.Mul64(a, b);
    return (uint64)(hi ^ lo);
}

} // end maphash_package
