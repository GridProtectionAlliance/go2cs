// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.7,amd64,!gccgo,!appengine

// package chacha20poly1305 -- go2cs converted at 2020 August 29 10:11:12 UTC
// import "vendor/golang_org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang_org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\chacha20poly1305\chacha20poly1305_amd64.go
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class chacha20poly1305_package
    {
        //go:noescape
        private static bool chacha20Poly1305Open(slice<byte> dst, slice<uint> key, slice<byte> src, slice<byte> ad)
;

        //go:noescape
        private static void chacha20Poly1305Seal(slice<byte> dst, slice<uint> key, slice<byte> src, slice<byte> ad)
;

        // cpuid is implemented in chacha20poly1305_amd64.s.
        private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg)
;

        // xgetbv with ecx = 0 is implemented in chacha20poly1305_amd64.s.
        private static (uint, uint) xgetbv()
;

        private static bool useASM = default;        private static bool useAVX2 = default;

        private static void init()
        {
            detectCpuFeatures();
        }

        // detectCpuFeatures is used to detect if cpu instructions
        // used by the functions implemented in assembler in
        // chacha20poly1305_amd64.s are supported.
        private static void detectCpuFeatures()
        {
            var (maxId, _, _, _) = cpuid(0L, 0L);
            if (maxId < 1L)
            {>>MARKER:FUNCTION_xgetbv_BLOCK_PREFIX<<
                return;
            }
            var (_, _, ecx1, _) = cpuid(1L, 0L);

            var haveSSSE3 = isSet(9L, ecx1);
            useASM = haveSSSE3;

            var haveOSXSAVE = isSet(27L, ecx1);

            var osSupportsAVX = false; 
            // For XGETBV, OSXSAVE bit is required and sufficient.
            if (haveOSXSAVE)
            {>>MARKER:FUNCTION_cpuid_BLOCK_PREFIX<<
                var (eax, _) = xgetbv(); 
                // Check if XMM and YMM registers have OS support.
                osSupportsAVX = isSet(1L, eax) && isSet(2L, eax);
            }
            var haveAVX = isSet(28L, ecx1) && osSupportsAVX;

            if (maxId < 7L)
            {>>MARKER:FUNCTION_chacha20Poly1305Seal_BLOCK_PREFIX<<
                return;
            }
            var (_, ebx7, _, _) = cpuid(7L, 0L);
            var haveAVX2 = isSet(5L, ebx7) && haveAVX;
            var haveBMI2 = isSet(8L, ebx7);

            useAVX2 = haveAVX2 && haveBMI2;
        }

        // isSet checks if bit at bitpos is set in value.
        private static bool isSet(ulong bitpos, uint value)
        {
            return value & (1L << (int)(bitpos)) != 0L;
        }

        // setupState writes a ChaCha20 input matrix to state. See
        // https://tools.ietf.org/html/rfc7539#section-2.3.
        private static void setupState(ref array<uint> state, ref array<byte> key, slice<byte> nonce)
        {
            state[0L] = 0x61707865UL;
            state[1L] = 0x3320646eUL;
            state[2L] = 0x79622d32UL;
            state[3L] = 0x6b206574UL;

            state[4L] = binary.LittleEndian.Uint32(key[..4L]);
            state[5L] = binary.LittleEndian.Uint32(key[4L..8L]);
            state[6L] = binary.LittleEndian.Uint32(key[8L..12L]);
            state[7L] = binary.LittleEndian.Uint32(key[12L..16L]);
            state[8L] = binary.LittleEndian.Uint32(key[16L..20L]);
            state[9L] = binary.LittleEndian.Uint32(key[20L..24L]);
            state[10L] = binary.LittleEndian.Uint32(key[24L..28L]);
            state[11L] = binary.LittleEndian.Uint32(key[28L..32L]);

            state[12L] = 0L;
            state[13L] = binary.LittleEndian.Uint32(nonce[..4L]);
            state[14L] = binary.LittleEndian.Uint32(nonce[4L..8L]);
            state[15L] = binary.LittleEndian.Uint32(nonce[8L..12L]);
        }

        private static slice<byte> seal(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {>>MARKER:FUNCTION_chacha20Poly1305Open_BLOCK_PREFIX<<
            if (!useASM)
            {
                return c.sealGeneric(dst, nonce, plaintext, additionalData);
            }
            array<uint> state = new array<uint>(16L);
            setupState(ref state, ref c.key, nonce);

            var (ret, out) = sliceForAppend(dst, len(plaintext) + 16L);
            chacha20Poly1305Seal(out[..], state[..], plaintext, additionalData);
            return ret;
        }

        private static (slice<byte>, error) open(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData)
        {
            if (!useASM)
            {
                return c.openGeneric(dst, nonce, ciphertext, additionalData);
            }
            array<uint> state = new array<uint>(16L);
            setupState(ref state, ref c.key, nonce);

            ciphertext = ciphertext[..len(ciphertext) - 16L];
            var (ret, out) = sliceForAppend(dst, len(ciphertext));
            if (!chacha20Poly1305Open(out, state[..], ciphertext, additionalData))
            {
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, errOpen);
            }
            return (ret, null);
        }
    }
}}}}}
