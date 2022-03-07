// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// AMD64-specific hardware-assisted CRC32 algorithms. See crc32.go for a
// description of the interface that each architecture-specific file
// implements.

// package crc32 -- go2cs converted at 2022 March 06 22:14:53 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Program Files\Go\src\hash\crc32\crc32_amd64.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;

namespace go.hash;

public static partial class crc32_package {

    // This file contains the code to call the SSE 4.2 version of the Castagnoli
    // and IEEE CRC.

    // castagnoliSSE42 is defined in crc32_amd64.s and uses the SSE 4.2 CRC32
    // instruction.
    //go:noescape
private static uint castagnoliSSE42(uint crc, slice<byte> p);

// castagnoliSSE42Triple is defined in crc32_amd64.s and uses the SSE 4.2 CRC32
// instruction.
//go:noescape
private static (uint, uint, uint) castagnoliSSE42Triple(uint crcA, uint crcB, uint crcC, slice<byte> a, slice<byte> b, slice<byte> c, uint rounds);

// ieeeCLMUL is defined in crc_amd64.s and uses the PCLMULQDQ
// instruction as well as SSE 4.1.
//go:noescape
private static uint ieeeCLMUL(uint crc, slice<byte> p);

private static readonly nint castagnoliK1 = 168;

private static readonly nint castagnoliK2 = 1344;



private partial struct sse42Table { // : array<Table>
}

private static ptr<sse42Table> castagnoliSSE42TableK1;
private static ptr<sse42Table> castagnoliSSE42TableK2;

private static bool archAvailableCastagnoli() {
    return cpu.X86.HasSSE42;
}

private static void archInitCastagnoli() => func((_, panic, _) => {
    if (!cpu.X86.HasSSE42) {>>MARKER:FUNCTION_ieeeCLMUL_BLOCK_PREFIX<<
        panic("arch-specific Castagnoli not available");
    }
    castagnoliSSE42TableK1 = @new<sse42Table>();
    castagnoliSSE42TableK2 = @new<sse42Table>(); 
    // See description in updateCastagnoli.
    //    t[0][i] = CRC(i000, O)
    //    t[1][i] = CRC(0i00, O)
    //    t[2][i] = CRC(00i0, O)
    //    t[3][i] = CRC(000i, O)
    // where O is a sequence of K zeros.
    array<byte> tmp = new array<byte>(castagnoliK2);
    for (nint b = 0; b < 4; b++) {>>MARKER:FUNCTION_castagnoliSSE42Triple_BLOCK_PREFIX<<
        for (nint i = 0; i < 256; i++) {>>MARKER:FUNCTION_castagnoliSSE42_BLOCK_PREFIX<<
            var val = uint32(i) << (int)(uint32(b * 8));
            castagnoliSSE42TableK1[b][i] = castagnoliSSE42(val, tmp[..(int)castagnoliK1]);
            castagnoliSSE42TableK2[b][i] = castagnoliSSE42(val, tmp[..]);
        }
    }

});

// castagnoliShift computes the CRC32-C of K1 or K2 zeroes (depending on the
// table given) with the given initial crc value. This corresponds to
// CRC(crc, O) in the description in updateCastagnoli.
private static uint castagnoliShift(ptr<sse42Table> _addr_table, uint crc) {
    ref sse42Table table = ref _addr_table.val;

    return table[3][crc >> 24] ^ table[2][(crc >> 16) & 0xFF] ^ table[1][(crc >> 8) & 0xFF] ^ table[0][crc & 0xFF];
}

private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, _) => {
    if (!cpu.X86.HasSSE42) {
        panic("not available");
    }
    crc = ~crc; 

    // If a buffer is long enough to use the optimization, process the first few
    // bytes to align the buffer to an 8 byte boundary (if necessary).
    if (len(p) >= castagnoliK1 * 3) {
        var delta = int(uintptr(@unsafe.Pointer(_addr_p[0])) & 7);
        if (delta != 0) {
            delta = 8 - delta;
            crc = castagnoliSSE42(crc, p[..(int)delta]);
            p = p[(int)delta..];
        }
    }
    while (len(p) >= castagnoliK2 * 3) { 
        // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
        var (crcA, crcB, crcC) = castagnoliSSE42Triple(crc, 0, 0, p, p[(int)castagnoliK2..], p[(int)castagnoliK2 * 2..], castagnoliK2 / 24); 

        // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
        var crcAB = castagnoliShift(_addr_castagnoliSSE42TableK2, crcA) ^ crcB; 
        // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
        crc = castagnoliShift(_addr_castagnoliSSE42TableK2, crcAB) ^ crcC;
        p = p[(int)castagnoliK2 * 3..];

    } 

    // Process 3*K1 at a time.
    while (len(p) >= castagnoliK1 * 3) { 
        // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
        (crcA, crcB, crcC) = castagnoliSSE42Triple(crc, 0, 0, p, p[(int)castagnoliK1..], p[(int)castagnoliK1 * 2..], castagnoliK1 / 24); 

        // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
        crcAB = castagnoliShift(_addr_castagnoliSSE42TableK1, crcA) ^ crcB; 
        // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
        crc = castagnoliShift(_addr_castagnoliSSE42TableK1, crcAB) ^ crcC;
        p = p[(int)castagnoliK1 * 3..];

    } 

    // Use the simple implementation for what's left.
    crc = castagnoliSSE42(crc, p);
    return ~crc;

});

private static bool archAvailableIEEE() {
    return cpu.X86.HasPCLMULQDQ && cpu.X86.HasSSE41;
}

private static ptr<slicing8Table> archIeeeTable8;

private static void archInitIEEE() => func((_, panic, _) => {
    if (!cpu.X86.HasPCLMULQDQ || !cpu.X86.HasSSE41) {
        panic("not available");
    }
    archIeeeTable8 = slicingMakeTable(IEEE);

});

private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, _) => {
    if (!cpu.X86.HasPCLMULQDQ || !cpu.X86.HasSSE41) {
        panic("not available");
    }
    if (len(p) >= 64) {
        var left = len(p) & 15;
        var @do = len(p) - left;
        crc = ~ieeeCLMUL(~crc, p[..(int)do]);
        p = p[(int)do..];
    }
    if (len(p) == 0) {
        return crc;
    }
    return slicingUpdate(crc, archIeeeTable8, p);

});

} // end crc32_package
