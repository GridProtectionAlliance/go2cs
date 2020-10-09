// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 arm64

// package aes -- go2cs converted at 2020 October 09 04:52:53 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\aes_gcm.go
using cipher = go.crypto.cipher_package;
using subtleoverlap = go.crypto.@internal.subtle_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // The following functions are defined in gcm_*.s.

        //go:noescape
        private static void gcmAesInit(ptr<array<byte>> productTable, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesData(ptr<array<byte>> productTable, slice<byte> data, ptr<array<byte>> T)
;

        //go:noescape
        private static void gcmAesEnc(ptr<array<byte>> productTable, slice<byte> dst, slice<byte> src, ptr<array<byte>> ctr, ptr<array<byte>> T, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesDec(ptr<array<byte>> productTable, slice<byte> dst, slice<byte> src, ptr<array<byte>> ctr, ptr<array<byte>> T, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesFinish(ptr<array<byte>> productTable, ptr<array<byte>> tagMask, ptr<array<byte>> T, ulong pLen, ulong dLen)
;

        private static readonly long gcmBlockSize = (long)16L;
        private static readonly long gcmTagSize = (long)16L;
        private static readonly long gcmMinimumTagSize = (long)12L; // NIST SP 800-38D recommends tags with 12 or more bytes.
        private static readonly long gcmStandardNonceSize = (long)12L;


        private static var errOpen = errors.New("cipher: message authentication failed");

        // aesCipherGCM implements crypto/cipher.gcmAble so that crypto/cipher.NewGCM
        // will use the optimised implementation in this file when possible. Instances
        // of this type only exist when hasGCMAsm returns true.
        private partial struct aesCipherGCM
        {
            public ref aesCipherAsm aesCipherAsm => ref aesCipherAsm_val;
        }

        // Assert that aesCipherGCM implements the gcmAble interface.
        private static gcmAble _ = (aesCipherGCM.val)(null);

        // NewGCM returns the AES cipher wrapped in Galois Counter Mode. This is only
        // called by crypto/cipher.NewGCM via the gcmAble interface.
        private static (cipher.AEAD, error) NewGCM(this ptr<aesCipherGCM> _addr_c, long nonceSize, long tagSize)
        {
            cipher.AEAD _p0 = default;
            error _p0 = default!;
            ref aesCipherGCM c = ref _addr_c.val;

            ptr<gcmAsm> g = addr(new gcmAsm(ks:c.enc,nonceSize:nonceSize,tagSize:tagSize));
            gcmAesInit(_addr_g.productTable, g.ks);
            return (g, error.As(null!)!);
        }

        private partial struct gcmAsm
        {
            public slice<uint> ks; // productTable contains pre-computed multiples of the binary-field
// element used in GHASH.
            public array<byte> productTable; // nonceSize contains the expected size of the nonce, in bytes.
            public long nonceSize; // tagSize contains the size of the tag, in bytes.
            public long tagSize;
        }

        private static long NonceSize(this ptr<gcmAsm> _addr_g)
        {
            ref gcmAsm g = ref _addr_g.val;

            return g.nonceSize;
        }

        private static long Overhead(this ptr<gcmAsm> _addr_g)
        {
            ref gcmAsm g = ref _addr_g.val;

            return g.tagSize;
        }

        // sliceForAppend takes a slice and a requested number of bytes. It returns a
        // slice with the contents of the given slice followed by that many bytes and a
        // second slice that aliases into it and contains only the extra bytes. If the
        // original slice has sufficient capacity then no allocation is performed.
        private static (slice<byte>, slice<byte>) sliceForAppend(slice<byte> @in, long n)
        {
            slice<byte> head = default;
            slice<byte> tail = default;

            {
                var total = len(in) + n;

                if (cap(in) >= total)
                {>>MARKER:FUNCTION_gcmAesFinish_BLOCK_PREFIX<<
                    head = in[..total];
                }
                else
                {>>MARKER:FUNCTION_gcmAesDec_BLOCK_PREFIX<<
                    head = make_slice<byte>(total);
                    copy(head, in);
                }

            }

            tail = head[len(in)..];
            return ;

        }

        // Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
        // details.
        private static slice<byte> Seal(this ptr<gcmAsm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func((_, panic, __) =>
        {
            ref gcmAsm g = ref _addr_g.val;

            if (len(nonce) != g.nonceSize)
            {>>MARKER:FUNCTION_gcmAesEnc_BLOCK_PREFIX<<
                panic("crypto/cipher: incorrect nonce length given to GCM");
            }

            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * BlockSize)
            {>>MARKER:FUNCTION_gcmAesData_BLOCK_PREFIX<<
                panic("crypto/cipher: message too large for GCM");
            }

            ref array<byte> counter = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_counter);            ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);



            if (len(nonce) == gcmStandardNonceSize)
            {>>MARKER:FUNCTION_gcmAesInit_BLOCK_PREFIX<< 
                // Init counter to nonce||1
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;

            }
            else
            { 
                // Otherwise counter = GHASH(nonce)
                gcmAesData(_addr_g.productTable, nonce, _addr_counter);
                gcmAesFinish(_addr_g.productTable, _addr_tagMask, _addr_counter, uint64(len(nonce)), uint64(0L));

            }

            encryptBlockAsm(len(g.ks) / 4L - 1L, _addr_g.ks[0L], _addr_tagMask[0L], _addr_counter[0L]);

            ref array<byte> tagOut = ref heap(new array<byte>(gcmTagSize), out ptr<array<byte>> _addr_tagOut);
            gcmAesData(_addr_g.productTable, data, _addr_tagOut);

            var (ret, out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
            if (subtleoverlap.InexactOverlap(out[..len(plaintext)], plaintext))
            {
                panic("crypto/cipher: invalid buffer overlap");
            }

            if (len(plaintext) > 0L)
            {
                gcmAesEnc(_addr_g.productTable, out, plaintext, _addr_counter, _addr_tagOut, g.ks);
            }

            gcmAesFinish(_addr_g.productTable, _addr_tagMask, _addr_tagOut, uint64(len(plaintext)), uint64(len(data)));
            copy(out[len(plaintext)..], tagOut[..]);

            return ret;

        });

        // Open authenticates and decrypts ciphertext. See the cipher.AEAD interface
        // for details.
        private static (slice<byte>, error) Open(this ptr<gcmAsm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func((_, panic, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref gcmAsm g = ref _addr_g.val;

            if (len(nonce) != g.nonceSize)
            {
                panic("crypto/cipher: incorrect nonce length given to GCM");
            } 
            // Sanity check to prevent the authentication from always succeeding if an implementation
            // leaves tagSize uninitialized, for example.
            if (g.tagSize < gcmMinimumTagSize)
            {
                panic("crypto/cipher: incorrect GCM tag size");
            }

            if (len(ciphertext) < g.tagSize)
            {
                return (null, error.As(errOpen)!);
            }

            if (uint64(len(ciphertext)) > ((1L << (int)(32L)) - 2L) * uint64(BlockSize) + uint64(g.tagSize))
            {
                return (null, error.As(errOpen)!);
            }

            var tag = ciphertext[len(ciphertext) - g.tagSize..];
            ciphertext = ciphertext[..len(ciphertext) - g.tagSize]; 

            // See GCM spec, section 7.1.
            ref array<byte> counter = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_counter);            ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);



            if (len(nonce) == gcmStandardNonceSize)
            { 
                // Init counter to nonce||1
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;

            }
            else
            { 
                // Otherwise counter = GHASH(nonce)
                gcmAesData(_addr_g.productTable, nonce, _addr_counter);
                gcmAesFinish(_addr_g.productTable, _addr_tagMask, _addr_counter, uint64(len(nonce)), uint64(0L));

            }

            encryptBlockAsm(len(g.ks) / 4L - 1L, _addr_g.ks[0L], _addr_tagMask[0L], _addr_counter[0L]);

            ref array<byte> expectedTag = ref heap(new array<byte>(gcmTagSize), out ptr<array<byte>> _addr_expectedTag);
            gcmAesData(_addr_g.productTable, data, _addr_expectedTag);

            var (ret, out) = sliceForAppend(dst, len(ciphertext));
            if (subtleoverlap.InexactOverlap(out, ciphertext))
            {
                panic("crypto/cipher: invalid buffer overlap");
            }

            if (len(ciphertext) > 0L)
            {
                gcmAesDec(_addr_g.productTable, out, ciphertext, _addr_counter, _addr_expectedTag, g.ks);
            }

            gcmAesFinish(_addr_g.productTable, _addr_tagMask, _addr_expectedTag, uint64(len(ciphertext)), uint64(len(data)));

            if (subtle.ConstantTimeCompare(expectedTag[..g.tagSize], tag) != 1L)
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
}}
