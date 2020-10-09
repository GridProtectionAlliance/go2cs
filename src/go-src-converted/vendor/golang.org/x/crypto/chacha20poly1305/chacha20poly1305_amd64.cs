// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo,!purego

// package chacha20poly1305 -- go2cs converted at 2020 October 09 06:06:16 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305_amd64.go
using binary = go.encoding.binary_package;

using subtle = go.golang.org.x.crypto.@internal.subtle_package;
using cpu = go.golang.org.x.sys.cpu_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
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

        private static var useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI2;

        // setupState writes a ChaCha20 input matrix to state. See
        // https://tools.ietf.org/html/rfc7539#section-2.3.
        private static void setupState(ptr<array<uint>> _addr_state, ptr<array<byte>> _addr_key, slice<byte> nonce)
        {
            ref array<uint> state = ref _addr_state.val;
            ref array<byte> key = ref _addr_key.val;

            state[0L] = 0x61707865UL;
            state[1L] = 0x3320646eUL;
            state[2L] = 0x79622d32UL;
            state[3L] = 0x6b206574UL;

            state[4L] = binary.LittleEndian.Uint32(key[0L..4L]);
            state[5L] = binary.LittleEndian.Uint32(key[4L..8L]);
            state[6L] = binary.LittleEndian.Uint32(key[8L..12L]);
            state[7L] = binary.LittleEndian.Uint32(key[12L..16L]);
            state[8L] = binary.LittleEndian.Uint32(key[16L..20L]);
            state[9L] = binary.LittleEndian.Uint32(key[20L..24L]);
            state[10L] = binary.LittleEndian.Uint32(key[24L..28L]);
            state[11L] = binary.LittleEndian.Uint32(key[28L..32L]);

            state[12L] = 0L;
            state[13L] = binary.LittleEndian.Uint32(nonce[0L..4L]);
            state[14L] = binary.LittleEndian.Uint32(nonce[4L..8L]);
            state[15L] = binary.LittleEndian.Uint32(nonce[8L..12L]);
        }

        private static slice<byte> seal(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func((_, panic, __) =>
        {
            ref chacha20poly1305 c = ref _addr_c.val;

            if (!cpu.X86.HasSSSE3)
            {>>MARKER:FUNCTION_chacha20Poly1305Seal_BLOCK_PREFIX<<
                return c.sealGeneric(dst, nonce, plaintext, additionalData);
            }

            ref array<uint> state = ref heap(new array<uint>(16L), out ptr<array<uint>> _addr_state);
            setupState(_addr_state, _addr_c.key, nonce);

            var (ret, out) = sliceForAppend(dst, len(plaintext) + 16L);
            if (subtle.InexactOverlap(out, plaintext))
            {>>MARKER:FUNCTION_chacha20Poly1305Open_BLOCK_PREFIX<<
                panic("chacha20poly1305: invalid buffer overlap");
            }

            chacha20Poly1305Seal(out[..], state[..], plaintext, additionalData);
            return ret;

        });

        private static (slice<byte>, error) open(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func((_, panic, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref chacha20poly1305 c = ref _addr_c.val;

            if (!cpu.X86.HasSSSE3)
            {
                return c.openGeneric(dst, nonce, ciphertext, additionalData);
            }

            ref array<uint> state = ref heap(new array<uint>(16L), out ptr<array<uint>> _addr_state);
            setupState(_addr_state, _addr_c.key, nonce);

            ciphertext = ciphertext[..len(ciphertext) - 16L];
            var (ret, out) = sliceForAppend(dst, len(ciphertext));
            if (subtle.InexactOverlap(out, ciphertext))
            {
                panic("chacha20poly1305: invalid buffer overlap");
            }

            if (!chacha20Poly1305Open(out, state[..], ciphertext, additionalData))
            {
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, error.As(errOpen)!);

            }

            return (ret, error.As(null!)!);

        });
    }
}}}}}
