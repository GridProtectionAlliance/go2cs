// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using errors = errors_package;
using byteorder = @internal.byteorder_package;
using bits = math.bits_package;
using @internal;
using math;

partial class rand_package {

// https://numpy.org/devdocs/reference/random/upgrading-pcg64.html
// https://github.com/imneme/pcg-cpp/commit/871d0494ee9c9a7b7c43f753e3d8ca47c26f8005

// A PCG is a PCG generator with 128 bits of internal state.
// A zero PCG is equivalent to NewPCG(0, 0).
[GoType] partial struct PCG {
    internal uint64 hi;
    internal uint64 lo;
}

// NewPCG returns a new PCG seeded with the given values.
public static ж<PCG> NewPCG(uint64 seed1, uint64 seed2) {
    return Ꮡ(new PCG(seed1, seed2));
}

// Seed resets the PCG to behave the same way as NewPCG(seed1, seed2).
[GoRecv] public static void Seed(this ref PCG p, uint64 seed1, uint64 seed2) {
    p.hi = seed1;
    p.lo = seed2;
}

// MarshalBinary implements the encoding.BinaryMarshaler interface.
[GoRecv] public static (slice<byte>, error) MarshalBinary(this ref PCG p) {
    var b = new slice<byte>(20);
    copy(b, "pcg:"u8);
    byteorder.BePutUint64(b[4..], p.hi);
    byteorder.BePutUint64(b[(int)(4 + 8)..], p.lo);
    return (b, default!);
}

internal static error errUnmarshalPCG = errors.New("invalid PCG encoding"u8);

// UnmarshalBinary implements the encoding.BinaryUnmarshaler interface.
[GoRecv] public static error UnmarshalBinary(this ref PCG p, slice<byte> data) {
    if (len(data) != 20 || ((@string)(data[..4])) != "pcg:"u8) {
        return errUnmarshalPCG;
    }
    p.hi = byteorder.BeUint64(data[4..]);
    p.lo = byteorder.BeUint64(data[(int)(4 + 8)..]);
    return default!;
}

[GoRecv] internal static (uint64 hi, uint64 lo) next(this ref PCG p) {
    uint64 hi = default!;
    uint64 lo = default!;

    // https://github.com/imneme/pcg-cpp/blob/428802d1a5/include/pcg_random.hpp#L161
    //
    // Numpy's PCG multiplies by the 64-bit value cheapMul
    // instead of the 128-bit value used here and in the official PCG code.
    // This does not seem worthwhile, at least for Go: not having any high
    // bits in the multiplier reduces the effect of low bits on the highest bits,
    // and it only saves 1 multiply out of 3.
    // (On 32-bit systems, it saves 1 out of 6, since Mul64 is doing 4.)
    static readonly UntypedInt mulHi = 2549297995355413924;
    
    static readonly UntypedInt mulLo = 4865540595714422341;
    
    static readonly UntypedInt incHi = 6364136223846793005;
    
    static readonly UntypedInt incLo = 1442695040888963407;
    // state = state * mul + inc
    (hi, lo) = bits.Mul64(p.lo, mulLo);
    hi += p.hi * mulLo + p.lo * mulHi;
    var (lo, c) = bits.Add64(lo, incLo, 0);
    (hi, _) = bits.Add64(hi, incHi, c);
    p.lo = lo;
    p.hi = hi;
    return (hi, lo);
}

// Uint64 return a uniformly-distributed random uint64 value.
[GoRecv] public static uint64 Uint64(this ref PCG p) {
    var (hi, lo) = p.next();
    // XSL-RR would be
    //	hi, lo := p.next()
    //	return bits.RotateLeft64(lo^hi, -int(hi>>58))
    // but Numpy uses DXSM and O'Neill suggests doing the same.
    // See https://github.com/golang/go/issues/21835#issuecomment-739065688
    // and following comments.
    // DXSM "double xorshift multiply"
    // https://github.com/imneme/pcg-cpp/blob/428802d1a5/include/pcg_random.hpp#L1015
    // https://github.com/imneme/pcg-cpp/blob/428802d1a5/include/pcg_random.hpp#L176
    static readonly UntypedInt cheapMul = /* 0xda942042e4dd58b5 */ 15750249268501108917;
    hi ^= (uint64)(hi >> (int)(32));
    hi *= cheapMul;
    hi ^= (uint64)(hi >> (int)(48));
    hi *= ((uint64)(lo | 1));
    return hi;
}

} // end rand_package
