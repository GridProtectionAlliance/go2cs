// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// AMD64-specific hardware-assisted CRC32 algorithms. See crc32.go for a
// description of the interface that each architecture-specific file
// implements.

// package crc32 -- go2cs converted at 2020 October 09 04:50:07 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32_amd64.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        // This file contains the code to call the SSE 4.2 version of the Castagnoli
        // and IEEE CRC.

        // castagnoliSSE42 is defined in crc32_amd64.s and uses the SSE 4.2 CRC32
        // instruction.
        //go:noescape
        private static uint castagnoliSSE42(uint crc, slice<byte> p)
;

        // castagnoliSSE42Triple is defined in crc32_amd64.s and uses the SSE 4.2 CRC32
        // instruction.
        //go:noescape
        private static (uint, uint, uint) castagnoliSSE42Triple(uint crcA, uint crcB, uint crcC, slice<byte> a, slice<byte> b, slice<byte> c, uint rounds)
;

        // ieeeCLMUL is defined in crc_amd64.s and uses the PCLMULQDQ
        // instruction as well as SSE 4.1.
        //go:noescape
        private static uint ieeeCLMUL(uint crc, slice<byte> p)
;

        private static readonly long castagnoliK1 = (long)168L;

        private static readonly long castagnoliK2 = (long)1344L;



        private partial struct sse42Table // : array<Table>
        {
        }

        private static ptr<sse42Table> castagnoliSSE42TableK1;
        private static ptr<sse42Table> castagnoliSSE42TableK2;

        private static bool archAvailableCastagnoli()
        {
            return cpu.X86.HasSSE42;
        }

        private static void archInitCastagnoli() => func((_, panic, __) =>
        {
            if (!cpu.X86.HasSSE42)
            {>>MARKER:FUNCTION_ieeeCLMUL_BLOCK_PREFIX<<
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
            for (long b = 0L; b < 4L; b++)
            {>>MARKER:FUNCTION_castagnoliSSE42Triple_BLOCK_PREFIX<<
                for (long i = 0L; i < 256L; i++)
                {>>MARKER:FUNCTION_castagnoliSSE42_BLOCK_PREFIX<<
                    var val = uint32(i) << (int)(uint32(b * 8L));
                    castagnoliSSE42TableK1[b][i] = castagnoliSSE42(val, tmp[..castagnoliK1]);
                    castagnoliSSE42TableK2[b][i] = castagnoliSSE42(val, tmp[..]);
                }


            }


        });

        // castagnoliShift computes the CRC32-C of K1 or K2 zeroes (depending on the
        // table given) with the given initial crc value. This corresponds to
        // CRC(crc, O) in the description in updateCastagnoli.
        private static uint castagnoliShift(ptr<sse42Table> _addr_table, uint crc)
        {
            ref sse42Table table = ref _addr_table.val;

            return table[3L][crc >> (int)(24L)] ^ table[2L][(crc >> (int)(16L)) & 0xFFUL] ^ table[1L][(crc >> (int)(8L)) & 0xFFUL] ^ table[0L][crc & 0xFFUL];
        }

        private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!cpu.X86.HasSSE42)
            {
                panic("not available");
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
            if (len(p) >= castagnoliK1 * 3L)
            {
                var delta = int(uintptr(@unsafe.Pointer(_addr_p[0L])) & 7L);
                if (delta != 0L)
                {
                    delta = 8L - delta;
                    crc = castagnoliSSE42(crc, p[..delta]);
                    p = p[delta..];
                }

            } 

            // Process 3*K2 at a time.
            while (len(p) >= castagnoliK2 * 3L)
            { 
                // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
                var (crcA, crcB, crcC) = castagnoliSSE42Triple(crc, 0L, 0L, p, p[castagnoliK2..], p[castagnoliK2 * 2L..], castagnoliK2 / 24L); 

                // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
                var crcAB = castagnoliShift(_addr_castagnoliSSE42TableK2, crcA) ^ crcB; 
                // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
                crc = castagnoliShift(_addr_castagnoliSSE42TableK2, crcAB) ^ crcC;
                p = p[castagnoliK2 * 3L..];

            } 

            // Process 3*K1 at a time.
 

            // Process 3*K1 at a time.
            while (len(p) >= castagnoliK1 * 3L)
            { 
                // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
                (crcA, crcB, crcC) = castagnoliSSE42Triple(crc, 0L, 0L, p, p[castagnoliK1..], p[castagnoliK1 * 2L..], castagnoliK1 / 24L); 

                // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
                crcAB = castagnoliShift(_addr_castagnoliSSE42TableK1, crcA) ^ crcB; 
                // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
                crc = castagnoliShift(_addr_castagnoliSSE42TableK1, crcAB) ^ crcC;
                p = p[castagnoliK1 * 3L..];

            } 

            // Use the simple implementation for what's left.
 

            // Use the simple implementation for what's left.
            crc = castagnoliSSE42(crc, p);
            return ~crc;

        });

        private static bool archAvailableIEEE()
        {
            return cpu.X86.HasPCLMULQDQ && cpu.X86.HasSSE41;
        }

        private static ptr<slicing8Table> archIeeeTable8;

        private static void archInitIEEE() => func((_, panic, __) =>
        {
            if (!cpu.X86.HasPCLMULQDQ || !cpu.X86.HasSSE41)
            {
                panic("not available");
            } 
            // We still use slicing-by-8 for small buffers.
            archIeeeTable8 = slicingMakeTable(IEEE);

        });

        private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!cpu.X86.HasPCLMULQDQ || !cpu.X86.HasSSE41)
            {
                panic("not available");
            }

            if (len(p) >= 64L)
            {
                var left = len(p) & 15L;
                var @do = len(p) - left;
                crc = ~ieeeCLMUL(~crc, p[..do]);
                p = p[do..];
            }

            if (len(p) == 0L)
            {
                return crc;
            }

            return slicingUpdate(crc, archIeeeTable8, p);

        });
    }
}}
