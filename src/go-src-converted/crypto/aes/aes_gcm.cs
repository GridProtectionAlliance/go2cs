// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64

// package aes -- go2cs converted at 2020 August 29 08:28:31 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\aes_gcm.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // The following functions are defined in gcm_amd64.s.
        private static bool hasGCMAsm()
;

        //go:noescape
        private static void aesEncBlock(ref array<byte> dst, ref array<byte> src, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesInit(ref array<byte> productTable, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesData(ref array<byte> productTable, slice<byte> data, ref array<byte> T)
;

        //go:noescape
        private static void gcmAesEnc(ref array<byte> productTable, slice<byte> dst, slice<byte> src, ref array<byte> ctr, ref array<byte> T, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesDec(ref array<byte> productTable, slice<byte> dst, slice<byte> src, ref array<byte> ctr, ref array<byte> T, slice<uint> ks)
;

        //go:noescape
        private static void gcmAesFinish(ref array<byte> productTable, ref array<byte> tagMask, ref array<byte> T, ulong pLen, ulong dLen)
;

        private static readonly long gcmBlockSize = 16L;
        private static readonly long gcmTagSize = 16L;
        private static readonly long gcmStandardNonceSize = 12L;

        private static var errOpen = errors.New("cipher: message authentication failed");

        // aesCipherGCM implements crypto/cipher.gcmAble so that crypto/cipher.NewGCM
        // will use the optimised implementation in this file when possible. Instances
        // of this type only exist when hasGCMAsm returns true.
        private partial struct aesCipherGCM
        {
            public ref aesCipherAsm aesCipherAsm => ref aesCipherAsm_val;
        }

        // Assert that aesCipherGCM implements the gcmAble interface.
        private static gcmAble _ = (aesCipherGCM.Value)(null);

        // NewGCM returns the AES cipher wrapped in Galois Counter Mode. This is only
        // called by crypto/cipher.NewGCM via the gcmAble interface.
        private static (cipher.AEAD, error) NewGCM(this ref aesCipherGCM c, long nonceSize)
        {>>MARKER:FUNCTION_gcmAesFinish_BLOCK_PREFIX<<
            gcmAsm g = ref new gcmAsm(ks:c.enc,nonceSize:nonceSize);
            gcmAesInit(ref g.productTable, g.ks);
            return (g, null);
        }

        private partial struct gcmAsm
        {
            public slice<uint> ks; // productTable contains pre-computed multiples of the binary-field
// element used in GHASH.
            public array<byte> productTable; // nonceSize contains the expected size of the nonce, in bytes.
            public long nonceSize;
        }

        private static long NonceSize(this ref gcmAsm g)
        {>>MARKER:FUNCTION_gcmAesDec_BLOCK_PREFIX<<
            return g.nonceSize;
        }

        private static long Overhead(this ref gcmAsm _p0)
        {>>MARKER:FUNCTION_gcmAesEnc_BLOCK_PREFIX<<
            return gcmTagSize;
        }

        // sliceForAppend takes a slice and a requested number of bytes. It returns a
        // slice with the contents of the given slice followed by that many bytes and a
        // second slice that aliases into it and contains only the extra bytes. If the
        // original slice has sufficient capacity then no allocation is performed.
        private static (slice<byte>, slice<byte>) sliceForAppend(slice<byte> @in, long n)
        {
            {
                var total = len(in) + n;

                if (cap(in) >= total)
                {>>MARKER:FUNCTION_gcmAesData_BLOCK_PREFIX<<
                    head = in[..total];
                }
                else
                {>>MARKER:FUNCTION_gcmAesInit_BLOCK_PREFIX<<
                    head = make_slice<byte>(total);
                    copy(head, in);
                }

            }
            tail = head[len(in)..];
            return;
        }

        // Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
        // details.
        private static slice<byte> Seal(this ref gcmAsm _g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func(_g, (ref gcmAsm g, Defer _, Panic panic, Recover __) =>
        {>>MARKER:FUNCTION_aesEncBlock_BLOCK_PREFIX<<
            if (len(nonce) != g.nonceSize)
            {>>MARKER:FUNCTION_hasGCMAsm_BLOCK_PREFIX<<
                panic("cipher: incorrect nonce length given to GCM");
            }
            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * BlockSize)
            {
                panic("cipher: message too large for GCM");
            }
            array<byte> counter = new array<byte>(gcmBlockSize);            array<byte> tagMask = new array<byte>(gcmBlockSize);



            if (len(nonce) == gcmStandardNonceSize)
            { 
                // Init counter to nonce||1
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;
            }
            else
            { 
                // Otherwise counter = GHASH(nonce)
                gcmAesData(ref g.productTable, nonce, ref counter);
                gcmAesFinish(ref g.productTable, ref tagMask, ref counter, uint64(len(nonce)), uint64(0L));
            }
            aesEncBlock(ref tagMask, ref counter, g.ks);

            array<byte> tagOut = new array<byte>(gcmTagSize);
            gcmAesData(ref g.productTable, data, ref tagOut);

            var (ret, out) = sliceForAppend(dst, len(plaintext) + gcmTagSize);
            if (len(plaintext) > 0L)
            {
                gcmAesEnc(ref g.productTable, out, plaintext, ref counter, ref tagOut, g.ks);
            }
            gcmAesFinish(ref g.productTable, ref tagMask, ref tagOut, uint64(len(plaintext)), uint64(len(data)));
            copy(out[len(plaintext)..], tagOut[..]);

            return ret;
        });

        // Open authenticates and decrypts ciphertext. See the cipher.AEAD interface
        // for details.
        private static (slice<byte>, error) Open(this ref gcmAsm _g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func(_g, (ref gcmAsm g, Defer _, Panic panic, Recover __) =>
        {
            if (len(nonce) != g.nonceSize)
            {
                panic("cipher: incorrect nonce length given to GCM");
            }
            if (len(ciphertext) < gcmTagSize)
            {
                return (null, errOpen);
            }
            if (uint64(len(ciphertext)) > ((1L << (int)(32L)) - 2L) * BlockSize + gcmTagSize)
            {
                return (null, errOpen);
            }
            var tag = ciphertext[len(ciphertext) - gcmTagSize..];
            ciphertext = ciphertext[..len(ciphertext) - gcmTagSize]; 

            // See GCM spec, section 7.1.
            array<byte> counter = new array<byte>(gcmBlockSize);            array<byte> tagMask = new array<byte>(gcmBlockSize);



            if (len(nonce) == gcmStandardNonceSize)
            { 
                // Init counter to nonce||1
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;
            }
            else
            { 
                // Otherwise counter = GHASH(nonce)
                gcmAesData(ref g.productTable, nonce, ref counter);
                gcmAesFinish(ref g.productTable, ref tagMask, ref counter, uint64(len(nonce)), uint64(0L));
            }
            aesEncBlock(ref tagMask, ref counter, g.ks);

            array<byte> expectedTag = new array<byte>(gcmTagSize);
            gcmAesData(ref g.productTable, data, ref expectedTag);

            var (ret, out) = sliceForAppend(dst, len(ciphertext));
            if (len(ciphertext) > 0L)
            {
                gcmAesDec(ref g.productTable, out, ciphertext, ref counter, ref expectedTag, g.ks);
            }
            gcmAesFinish(ref g.productTable, ref tagMask, ref expectedTag, uint64(len(ciphertext)), uint64(len(data)));

            if (subtle.ConstantTimeCompare(expectedTag[..], tag) != 1L)
            {
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, errOpen);
            }
            return (ret, null);
        });
    }
}}
