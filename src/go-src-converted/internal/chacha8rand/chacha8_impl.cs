// chacha8_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using go.golib;

namespace go.@internal;

// Hand-written body for internal/chacha8rand's `block`, which Go implements in ASSEMBLY on amd64
// and arm64 (chacha8_amd64.s / chacha8_arm64.s), so the converter emits it as a bodyless partial
// and the PartialStubGenerator would otherwise fill it with a throwing stub — taking every
// ChaCha8 consumer (math/rand/v2's ChaCha8 source, and the runtime's per-m generator) down at
// first use.
//
// This is GOARCH-gated in Go, not build-tag gated, so no `-tags` selection can substitute a
// portable file: on amd64 the ONLY Go body is the .s one. Forwarding to the converted
// `block_generic` is also not available — it opens the `*[32]uint64` output buffer as
// `(*[16][4]uint32)(unsafe.Pointer(buf))`, an array-SHAPE reinterpretation that a managed
// nested-array view cannot reconstruct.
//
// So the permutation is written directly here, following chacha8_generic.go exactly: the 4x4
// ChaCha8 matrix is held 4-ways interlaced (16 rows x 4 lanes of uint32, matching Go's
// [16][4]uint32 view of the buffer), setup seeds it, four iterations of eight quarter-rounds each
// give the 8 rounds, rows 4..11 are added back to the original key material, and the uint32 words
// are packed back into the uint64 buffer little-endian. Verified bit-exact against Go by
// internal/chacha8rand's own vectors through math/rand/v2's TestChaCha8* suite.
partial class chacha8rand_package
{
    // ChaCha20's "expand 32-byte k" constants, one per matrix row 0..3.
    private static readonly uint[] s_chacha8Constants = [0x61707865u, 0x3320646eu, 0x79622d32u, 0x6b206574u];

    internal static partial void block(ж<array<uint64>> seed, ж<array<uint64>> blocks, uint32 counter)
    {
        // b[row * 4 + lane] — the [16][4]uint32 view of the 32-uint64 output buffer.
        Span<uint> b = stackalloc uint[64];

        Setup(seed, b, counter);

        for (int i = 0; i < 4; i++)
        {
            uint b0 = b[0 * 4 + i], b1 = b[1 * 4 + i], b2 = b[2 * 4 + i], b3 = b[3 * 4 + i];
            uint b4 = b[4 * 4 + i], b5 = b[5 * 4 + i], b6 = b[6 * 4 + i], b7 = b[7 * 4 + i];
            uint b8 = b[8 * 4 + i], b9 = b[9 * 4 + i], b10 = b[10 * 4 + i], b11 = b[11 * 4 + i];
            uint b12 = b[12 * 4 + i], b13 = b[13 * 4 + i], b14 = b[14 * 4 + i], b15 = b[15 * 4 + i];

            // 4 iterations of eight quarter-rounds each is 8 rounds.
            for (int round = 0; round < 4; round++)
            {
                QuarterRound(ref b0, ref b4, ref b8, ref b12);
                QuarterRound(ref b1, ref b5, ref b9, ref b13);
                QuarterRound(ref b2, ref b6, ref b10, ref b14);
                QuarterRound(ref b3, ref b7, ref b11, ref b15);

                QuarterRound(ref b0, ref b5, ref b10, ref b15);
                QuarterRound(ref b1, ref b6, ref b11, ref b12);
                QuarterRound(ref b2, ref b7, ref b8, ref b13);
                QuarterRound(ref b3, ref b4, ref b9, ref b14);
            }

            // Add b4..b11 back to the original key material, like in ChaCha20, to avoid trivial
            // invertibility. There is no entropy in b0..b3 and b12..b15, so those are plain stores.
            b[0 * 4 + i] = b0;
            b[1 * 4 + i] = b1;
            b[2 * 4 + i] = b2;
            b[3 * 4 + i] = b3;
            b[4 * 4 + i] += b4;
            b[5 * 4 + i] += b5;
            b[6 * 4 + i] += b6;
            b[7 * 4 + i] += b7;
            b[8 * 4 + i] += b8;
            b[9 * 4 + i] += b9;
            b[10 * 4 + i] += b10;
            b[11 * 4 + i] += b11;
            b[12 * 4 + i] = b12;
            b[13 * 4 + i] = b13;
            b[14 * 4 + i] = b14;
            b[15 * 4 + i] = b15;
        }

        // Pack the uint32 words back into the uint64 buffer. Go reads the buffer through the same
        // little-endian aliasing the [16][4]uint32 view gave it (its big-endian branch word-swaps
        // to reach this same value), so the low word is the even index.
        ref array<uint64> target = ref blocks.Value;

        for (int j = 0; j < 32; j++)
            target[j] = (uint64)b[2 * j] | ((uint64)b[2 * j + 1] << 32);
    }

    // Fills the interlaced matrix: constant rows, the 4 seed uint64s split into 8 uint32 rows,
    // the per-lane counter row, and three zero rows. Every lane of a row carries the same value
    // except the counter row, whose lane i is counter+i — that is what makes the four
    // simultaneously-computed blocks distinct.
    private static void Setup(ж<array<uint64>> seed, Span<uint> b, uint32 counter)
    {
        ref array<uint64> s = ref seed.Value;

        for (int row = 0; row < 4; row++)
        {
            uint constant = s_chacha8Constants[row];

            for (int lane = 0; lane < 4; lane++)
                b[row * 4 + lane] = constant;
        }

        for (int word = 0; word < 8; word++)
        {
            // Seed word 2k is the low half of seed[k], word 2k+1 the high half.
            uint value = (uint)(s[word / 2] >> (32 * (word % 2)));

            for (int lane = 0; lane < 4; lane++)
                b[(4 + word) * 4 + lane] = value;
        }

        for (int lane = 0; lane < 4; lane++)
            b[12 * 4 + lane] = counter + (uint)lane;

        for (int row = 13; row < 16; row++)
        {
            for (int lane = 0; lane < 4; lane++)
                b[row * 4 + lane] = 0;
        }
    }

    // The ChaCha8 quarter round (chacha8_generic.go qr).
    private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
    {
        a += b;
        d ^= a;
        d = d << 16 | d >> 16;
        c += d;
        b ^= c;
        b = b << 12 | b >> 20;
        a += b;
        d ^= a;
        d = d << 8 | d >> 24;
        c += d;
        b ^= c;
        b = b << 7 | b >> 25;
    }
}
