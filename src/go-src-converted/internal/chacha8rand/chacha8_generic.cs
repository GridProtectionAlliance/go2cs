// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// ChaCha8 is ChaCha with 8 rounds.
// See https://cr.yp.to/chacha/chacha-20080128.pdf.
//
// ChaCha8 operates on a 4x4 matrix of uint32 values, initially set to:
//
//	const1 const2 const3 const4
//	seed   seed   seed   seed
//	seed   seed   seed   seed
//	counter64     0      0
//
// We use the same constants as ChaCha20 does, a random seed,
// and a counter. Running ChaCha8 on this input produces
// a 4x4 matrix of pseudo-random values with as much entropy
// as the seed.
//
// Given SIMD registers that can hold N uint32s, it is possible
// to run N ChaCha8 block transformations in parallel by filling
// the first register with the N copies of const1, the second
// with N copies of const2, and so on, and then running the operations.
//
// Each iteration of ChaCha8Rand operates over 32 bytes of input and
// produces 992 bytes of RNG output, plus 32 bytes of input for the next
// iteration.
//
// The 32 bytes of input are used as a ChaCha8 key, with a zero nonce, to
// produce 1024 bytes of output (16 blocks, with counters 0 to 15).
// First, for each block, the values 0x61707865, 0x3320646e, 0x79622d32,
// 0x6b206574 are subtracted from the 32-bit little-endian words at
// position 0, 1, 2, and 3 respectively, and an increasing counter
// starting at zero is subtracted from each word at position 12. Then,
// this stream is permuted such that for each sequence of four blocks,
// first we output the first four bytes of each block, then the next four
// bytes of each block, and so on. Finally, the last 32 bytes of output
// are used as the input of the next iteration, and the remaining 992
// bytes are the RNG output.
//
// See https://c2sp.org/chacha8rand for additional details.
//
// Normal ChaCha20 implementations for encryption use this same
// parallelism but then have to deinterlace the results so that
// it appears the blocks were generated separately. For the purposes
// of generating random numbers, the interlacing is fine.
// We are simply locked in to preserving the 4-way interlacing
// in any future optimizations.
namespace go.@internal;

using goarch = go.@internal.goarch_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class chacha8rand_package {

// setup sets up 4 ChaCha8 blocks in b32 with the counter and seed.
// Note that b32 is [16][4]uint32 not [4][16]uint32: the blocks are interlaced
// the same way they would be in a 4-way SIMD implementations.
internal static void setup(ж<array<uint64>> Ꮡseed, ж<array<array<uint32>>> Ꮡb32, uint32 counter) {
    ref var seed = ref Ꮡseed.Value;
    ref var b32 = ref Ꮡb32.Value;

    // Convert to uint64 to do half as many stores to memory.
    var b = (ж<array<array<uint64>>>)(uintptr)(new @unsafe.Pointer(Ꮡb32));
    // Constants; same as in ChaCha20: "expand 32-byte k"
    b.Value[0][0] = 0x61707865_61707865UL;
    b.Value[0][1] = 0x61707865_61707865UL;
    b.Value[1][0] = 0x3320646e_3320646eUL;
    b.Value[1][1] = 0x3320646e_3320646eUL;
    b.Value[2][0] = 0x79622d32_79622d32UL;
    b.Value[2][1] = 0x79622d32_79622d32UL;
    b.Value[3][0] = 0x6b206574_6b206574UL;
    b.Value[3][1] = 0x6b206574_6b206574UL;
    // Seed values.
    uint64 x64 = default!;
    uint32 x = default!;
    x = (uint32)seed[0];
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[4][0] = x64;
    b.Value[4][1] = x64;
    x = (uint32)((seed[0] >> (int)(32)));
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[5][0] = x64;
    b.Value[5][1] = x64;
    x = (uint32)seed[1];
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[6][0] = x64;
    b.Value[6][1] = x64;
    x = (uint32)((seed[1] >> (int)(32)));
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[7][0] = x64;
    b.Value[7][1] = x64;
    x = (uint32)seed[2];
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[8][0] = x64;
    b.Value[8][1] = x64;
    x = (uint32)((seed[2] >> (int)(32)));
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[9][0] = x64;
    b.Value[9][1] = x64;
    x = (uint32)seed[3];
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[10][0] = x64;
    b.Value[10][1] = x64;
    x = (uint32)((seed[3] >> (int)(32)));
    x64 = (uint64)(((uint64)x << (int)(32)) | (uint64)x);
    b.Value[11][0] = x64;
    b.Value[11][1] = x64;
    // Counters.
    if (goarch.BigEndian){
        b.Value[12][0] = (uint64)(((uint64)(counter + 0) << (int)(32)) | (uint64)(counter + 1));
        b.Value[12][1] = (uint64)(((uint64)(counter + 2) << (int)(32)) | (uint64)(counter + 3));
    } else {
        b.Value[12][0] = (uint64)((uint64)(counter + 0) | ((uint64)(counter + 1) << (int)(32)));
        b.Value[12][1] = (uint64)((uint64)(counter + 2) | ((uint64)(counter + 3) << (int)(32)));
    }
    // Zeros.
    b.Value[13][0] = 0;
    b.Value[13][1] = 0;
    b.Value[14][0] = 0;
    b.Value[14][1] = 0;
    b.Value[15][0] = 0;
    b.Value[15][1] = 0;
}

internal static void _ᴛ1() {
    // block and block_generic must have same type
    var x = block;
    x = block_generic;
    _ = x;
}

// block_generic is the non-assembly block implementation,
// for use on systems without special assembly.
// Even on such systems, it is quite fast: on GOOS=386,
// ChaCha8 using this code generates random values faster than PCG-DXSM.
internal static void block_generic(ж<array<uint64>> Ꮡseed, ж<array<uint64>> Ꮡbuf, uint32 counter) {
    ref var seed = ref Ꮡseed.Value;
    ref var buf = ref Ꮡbuf.Value;

    var b = (ж<array<array<uint32>>>)(uintptr)(new @unsafe.Pointer(Ꮡbuf));
    setup(Ꮡseed, b, counter);
    foreach (var (i, _) in b.Value[0]) {
        // Load block i from b[*][i] into local variables.
        var b0 = b.Value[0][i];
        var b1 = b.Value[1][i];
        var b2 = b.Value[2][i];
        var b3 = b.Value[3][i];
        var b4 = b.Value[4][i];
        var b5 = b.Value[5][i];
        var b6 = b.Value[6][i];
        var b7 = b.Value[7][i];
        var b8 = b.Value[8][i];
        var b9 = b.Value[9][i];
        var b10 = b.Value[10][i];
        var b11 = b.Value[11][i];
        var b12 = b.Value[12][i];
        var b13 = b.Value[13][i];
        var b14 = b.Value[14][i];
        var b15 = b.Value[15][i];
        // 4 iterations of eight quarter-rounds each is 8 rounds
        for (nint round = 0; round < 4; round++) {
            (b0, b4, b8, b12) = qr(b0, b4, b8, b12);
            (b1, b5, b9, b13) = qr(b1, b5, b9, b13);
            (b2, b6, b10, b14) = qr(b2, b6, b10, b14);
            (b3, b7, b11, b15) = qr(b3, b7, b11, b15);
            (b0, b5, b10, b15) = qr(b0, b5, b10, b15);
            (b1, b6, b11, b12) = qr(b1, b6, b11, b12);
            (b2, b7, b8, b13) = qr(b2, b7, b8, b13);
            (b3, b4, b9, b14) = qr(b3, b4, b9, b14);
        }
        // Store block i back into b[*][i].
        // Add b4..b11 back to the original key material,
        // like in ChaCha20, to avoid trivial invertibility.
        // There is no entropy in b0..b3 and b12..b15
        // so we can skip the additions and save some time.
        b.Value[0][i] = b0;
        b.Value[1][i] = b1;
        b.Value[2][i] = b2;
        b.Value[3][i] = b3;
        b.Value[4][i] += b4;
        b.Value[5][i] += b5;
        b.Value[6][i] += b6;
        b.Value[7][i] += b7;
        b.Value[8][i] += b8;
        b.Value[9][i] += b9;
        b.Value[10][i] += b10;
        b.Value[11][i] += b11;
        b.Value[12][i] = b12;
        b.Value[13][i] = b13;
        b.Value[14][i] = b14;
        b.Value[15][i] = b15;
    }
    if (goarch.BigEndian) {
        // On a big-endian system, reading the uint32 pairs as uint64s
        // will word-swap them compared to little-endian, so we word-swap
        // them here first to make the next swap get the right answer.
        foreach (var (i, x) in buf) {
            buf[i] = (uint64)((x >> (int)(32)) | (x << (int)(32)));
        }
    }
}

// qr is the (inlinable) ChaCha8 quarter round.
internal static (uint32 _a, uint32 _b, uint32 _c, uint32 _d) qr(uint32 a, uint32 b, uint32 c, uint32 d) {
    uint32 _a = default!;
    uint32 _b = default!;
    uint32 _c = default!;
    uint32 _d = default!;

    a += b;
    d ^= (uint32)(a);
    d = (uint32)((d << (int)(16)) | (d >> (int)(16)));
    c += d;
    b ^= (uint32)(c);
    b = (uint32)((b << (int)(12)) | (b >> (int)(20)));
    a += b;
    d ^= (uint32)(a);
    d = (uint32)((d << (int)(8)) | (d >> (int)(24)));
    c += d;
    b ^= (uint32)(c);
    b = (uint32)((b << (int)(7)) | (b >> (int)(25)));
    return (a, b, c, d);
}

} // end chacha8rand_package
