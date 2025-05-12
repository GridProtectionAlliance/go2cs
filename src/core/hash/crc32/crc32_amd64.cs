// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// AMD64-specific hardware-assisted CRC32 algorithms. See crc32.go for a
// description of the interface that each architecture-specific file
// implements.
namespace go.hash;

using cpu = @internal.cpu_package;
using @unsafe = unsafe_package;
using @internal;

partial class crc32_package {

// This file contains the code to call the SSE 4.2 version of the Castagnoli
// and IEEE CRC.

// castagnoliSSE42 is defined in crc32_amd64.s and uses the SSE 4.2 CRC32
// instruction.
//
//go:noescape
internal static partial uint32 castagnoliSSE42(uint32 crc, slice<byte> p);

// castagnoliSSE42Triple is defined in crc32_amd64.s and uses the SSE 4.2 CRC32
// instruction.
//
//go:noescape
internal static partial (uint32 retA, uint32 retB, uint32 retC) castagnoliSSE42Triple(uint32 crcA, uint32 crcB, uint32 crcC, slice<byte> a, slice<byte> b, slice<byte> c, uint32 rounds);

// ieeeCLMUL is defined in crc_amd64.s and uses the PCLMULQDQ
// instruction as well as SSE 4.1.
//
//go:noescape
internal static partial uint32 ieeeCLMUL(uint32 crc, slice<byte> p);

internal static readonly UntypedInt castagnoliK1 = 168;

internal static readonly UntypedInt castagnoliK2 = 1344;

[GoType("[4]Table")] partial struct sse42Table;

internal static ж<sse42Table> castagnoliSSE42TableK1;

internal static ж<sse42Table> castagnoliSSE42TableK2;

internal static bool archAvailableCastagnoli() {
    return cpu.X86.HasSSE42;
}

internal static void archInitCastagnoli() {
    if (!cpu.X86.HasSSE42) {
        throw panic("arch-specific Castagnoli not available");
    }
    castagnoliSSE42TableK1 = @new<sse42Table>();
    castagnoliSSE42TableK2 = @new<sse42Table>();
    // See description in updateCastagnoli.
    //    t[0][i] = CRC(i000, O)
    //    t[1][i] = CRC(0i00, O)
    //    t[2][i] = CRC(00i0, O)
    //    t[3][i] = CRC(000i, O)
    // where O is a sequence of K zeros.
    array<byte> tmp = new(1344); /* castagnoliK2 */
    for (nint b = 0; b < 4; b++) {
        for (nint i = 0; i < 256; i++) {
            var val = ((uint32)i) << (int)(((uint32)(b * 8)));
            castagnoliSSE42TableK1.val[b][i] = castagnoliSSE42(val, tmp[..(int)(castagnoliK1)]);
            castagnoliSSE42TableK2.val[b][i] = castagnoliSSE42(val, tmp[..]);
        }
    }
}

// castagnoliShift computes the CRC32-C of K1 or K2 zeroes (depending on the
// table given) with the given initial crc value. This corresponds to
// CRC(crc, O) in the description in updateCastagnoli.
internal static uint32 castagnoliShift(ж<sse42Table> Ꮡtable, uint32 crc) {
    ref var table = ref Ꮡtable.val;

    return (uint32)((uint32)((uint32)(table[3][crc >> (int)(24)] ^ table[2][(uint32)((crc >> (int)(16)) & 255)]) ^ table[1][(uint32)((crc >> (int)(8)) & 255)]) ^ table[0][(uint32)(crc & 255)]);
}

internal static uint32 archUpdateCastagnoli(uint32 crc, slice<byte> p) {
    if (!cpu.X86.HasSSE42) {
        throw panic("not available");
    }
    // This method is inspired from the algorithm in Intel's white paper:
    //    "Fast CRC Computation for iSCSI Polynomial Using CRC32 Instruction"
    // The same strategy of splitting the buffer in three is used but the
    // combining calculation is different; the complete derivation is explained
    // below.
    //
    // -- The basic idea --
    //
    // The CRC32 instruction (available in SSE4.2) can process 8 bytes at a
    // time. In recent Intel architectures the instruction takes 3 cycles;
    // however the processor can pipeline up to three instructions if they
    // don't depend on each other.
    //
    // Roughly this means that we can process three buffers in about the same
    // time we can process one buffer.
    //
    // The idea is then to split the buffer in three, CRC the three pieces
    // separately and then combine the results.
    //
    // Combining the results requires precomputed tables, so we must choose a
    // fixed buffer length to optimize. The longer the length, the faster; but
    // only buffers longer than this length will use the optimization. We choose
    // two cutoffs and compute tables for both:
    //  - one around 512: 168*3=504
    //  - one around 4KB: 1344*3=4032
    //
    // -- The nitty gritty --
    //
    // Let CRC(I, X) be the non-inverted CRC32-C of the sequence X (with
    // initial non-inverted CRC I). This function has the following properties:
    //   (a) CRC(I, AB) = CRC(CRC(I, A), B)
    //   (b) CRC(I, A xor B) = CRC(I, A) xor CRC(0, B)
    //
    // Say we want to compute CRC(I, ABC) where A, B, C are three sequences of
    // K bytes each, where K is a fixed constant. Let O be the sequence of K zero
    // bytes.
    //
    // CRC(I, ABC) = CRC(I, ABO xor C)
    //             = CRC(I, ABO) xor CRC(0, C)
    //             = CRC(CRC(I, AB), O) xor CRC(0, C)
    //             = CRC(CRC(I, AO xor B), O) xor CRC(0, C)
    //             = CRC(CRC(I, AO) xor CRC(0, B), O) xor CRC(0, C)
    //             = CRC(CRC(CRC(I, A), O) xor CRC(0, B), O) xor CRC(0, C)
    //
    // The castagnoliSSE42Triple function can compute CRC(I, A), CRC(0, B),
    // and CRC(0, C) efficiently.  We just need to find a way to quickly compute
    // CRC(uvwx, O) given a 4-byte initial value uvwx. We can precompute these
    // values; since we can't have a 32-bit table, we break it up into four
    // 8-bit tables:
    //
    //    CRC(uvwx, O) = CRC(u000, O) xor
    //                   CRC(0v00, O) xor
    //                   CRC(00w0, O) xor
    //                   CRC(000x, O)
    //
    // We can compute tables corresponding to the four terms for all 8-bit
    // values.
    crc = ~crc;
    // If a buffer is long enough to use the optimization, process the first few
    // bytes to align the buffer to an 8 byte boundary (if necessary).
    if (len(p) >= castagnoliK1 * 3) {
        nint delta = ((nint)((uintptr)(((uintptr)new @unsafe.Pointer(Ꮡ(p, 0))) & 7)));
        if (delta != 0) {
            delta = 8 - delta;
            crc = castagnoliSSE42(crc, p[..(int)(delta)]);
            p = p[(int)(delta)..];
        }
    }
    // Process 3*K2 at a time.
    while (len(p) >= castagnoliK2 * 3) {
        // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
        var (crcA, crcB, crcC) = castagnoliSSE42Triple(
            crc, 0, 0,
            p, p[(int)(castagnoliK2)..], p[(int)(castagnoliK2 * 2)..],
            castagnoliK2 / 24);
        // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
        var crcAB = (uint32)(castagnoliShift(castagnoliSSE42TableK2, crcA) ^ crcB);
        // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
        crc = (uint32)(castagnoliShift(castagnoliSSE42TableK2, crcAB) ^ crcC);
        p = p[(int)(castagnoliK2 * 3)..];
    }
    // Process 3*K1 at a time.
    while (len(p) >= castagnoliK1 * 3) {
        // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
        var (crcA, crcB, crcC) = castagnoliSSE42Triple(
            crc, 0, 0,
            p, p[(int)(castagnoliK1)..], p[(int)(castagnoliK1 * 2)..],
            castagnoliK1 / 24);
        // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
        var crcAB = (uint32)(castagnoliShift(castagnoliSSE42TableK1, crcA) ^ crcB);
        // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
        crc = (uint32)(castagnoliShift(castagnoliSSE42TableK1, crcAB) ^ crcC);
        p = p[(int)(castagnoliK1 * 3)..];
    }
    // Use the simple implementation for what's left.
    crc = castagnoliSSE42(crc, p);
    return ~crc;
}

internal static bool archAvailableIEEE() {
    return cpu.X86.HasPCLMULQDQ && cpu.X86.HasSSE41;
}

internal static ж<slicing8Table> archIeeeTable8;

internal static void archInitIEEE() {
    if (!cpu.X86.HasPCLMULQDQ || !cpu.X86.HasSSE41) {
        throw panic("not available");
    }
    // We still use slicing-by-8 for small buffers.
    archIeeeTable8 = slicingMakeTable(IEEE);
}

internal static uint32 archUpdateIEEE(uint32 crc, slice<byte> p) {
    if (!cpu.X86.HasPCLMULQDQ || !cpu.X86.HasSSE41) {
        throw panic("not available");
    }
    if (len(p) >= 64) {
        nint left = (nint)(len(p) & 15);
        nint @do = len(p) - left;
        crc = ~ieeeCLMUL(~crc, p[..(int)(@do)]);
        p = p[(int)(@do)..];
    }
    if (len(p) == 0) {
        return crc;
    }
    return slicingUpdate(crc, archIeeeTable8, p);
}

} // end crc32_package
