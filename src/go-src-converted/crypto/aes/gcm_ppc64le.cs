// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build ppc64le

// package aes -- go2cs converted at 2020 October 09 04:53:49 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\gcm_ppc64le.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // This file implements GCM using an optimized GHASH function.

        //go:noescape
        private static void gcmInit(ptr<array<byte>> productTable, slice<byte> h)
;

        //go:noescape
        private static void gcmHash(slice<byte> output, ptr<array<byte>> productTable, slice<byte> inp, long len)
;

        //go:noescape
        private static void gcmMul(slice<byte> output, ptr<array<byte>> productTable)
;

        private static readonly long gcmCounterSize = (long)16L;
        private static readonly long gcmBlockSize = (long)16L;
        private static readonly long gcmTagSize = (long)16L;
        private static readonly long gcmStandardNonceSize = (long)12L;


        private static var errOpen = errors.New("cipher: message authentication failed");

        // Assert that aesCipherGCM implements the gcmAble interface.
        private static gcmAble _ = (aesCipherAsm.val)(null);

        private partial struct gcmAsm
        {
            public ptr<aesCipherAsm> cipher; // ks is the key schedule, the length of which depends on the size of
// the AES key.
            public slice<uint> ks; // productTable contains pre-computed multiples of the binary-field
// element used in GHASH.
            public array<byte> productTable; // nonceSize contains the expected size of the nonce, in bytes.
            public long nonceSize; // tagSize contains the size of the tag, in bytes.
            public long tagSize;
        }

        // NewGCM returns the AES cipher wrapped in Galois Counter Mode. This is only
        // called by crypto/cipher.NewGCM via the gcmAble interface.
        private static (cipher.AEAD, error) NewGCM(this ptr<aesCipherAsm> _addr_c, long nonceSize, long tagSize)
        {
            cipher.AEAD _p0 = default;
            error _p0 = default!;
            ref aesCipherAsm c = ref _addr_c.val;

            ptr<gcmAsm> g = addr(new gcmAsm(cipher:c,ks:c.enc,nonceSize:nonceSize,tagSize:tagSize));

            var hle = make_slice<byte>(gcmBlockSize);
            c.Encrypt(hle, hle); 

            // Reverse the bytes in each 8 byte chunk
            // Load little endian, store big endian
            var h1 = binary.LittleEndian.Uint64(hle[..8L]);
            var h2 = binary.LittleEndian.Uint64(hle[8L..]);
            binary.BigEndian.PutUint64(hle[..8L], h1);
            binary.BigEndian.PutUint64(hle[8L..], h2);
            gcmInit(_addr_g.productTable, hle);

            return (g, error.As(null!)!);

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

        private static (slice<byte>, slice<byte>) sliceForAppend(slice<byte> @in, long n)
        {
            slice<byte> head = default;
            slice<byte> tail = default;

            {
                var total = len(in) + n;

                if (cap(in) >= total)
                {>>MARKER:FUNCTION_gcmMul_BLOCK_PREFIX<<
                    head = in[..total];
                }
                else
                {>>MARKER:FUNCTION_gcmHash_BLOCK_PREFIX<<
                    head = make_slice<byte>(total);
                    copy(head, in);
                }

            }

            tail = head[len(in)..];
            return ;

        }

        // deriveCounter computes the initial GCM counter state from the given nonce.
        private static void deriveCounter(this ptr<gcmAsm> _addr_g, ptr<array<byte>> _addr_counter, slice<byte> nonce)
        {
            ref gcmAsm g = ref _addr_g.val;
            ref array<byte> counter = ref _addr_counter.val;

            if (len(nonce) == gcmStandardNonceSize)
            {>>MARKER:FUNCTION_gcmInit_BLOCK_PREFIX<<
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;
            }
            else
            {
                ref array<byte> hash = ref heap(new array<byte>(16L), out ptr<array<byte>> _addr_hash);
                g.paddedGHASH(_addr_hash, nonce);
                var lens = gcmLengths(0L, uint64(len(nonce)) * 8L);
                g.paddedGHASH(_addr_hash, lens[..]);
                copy(counter[..], hash[..]);
            }

        }

        // counterCrypt encrypts in using AES in counter mode and places the result
        // into out. counter is the initial count value and will be updated with the next
        // count value. The length of out must be greater than or equal to the length
        // of in.
        private static void counterCrypt(this ptr<gcmAsm> _addr_g, slice<byte> @out, slice<byte> @in, ptr<array<byte>> _addr_counter)
        {
            ref gcmAsm g = ref _addr_g.val;
            ref array<byte> counter = ref _addr_counter.val;

            array<byte> mask = new array<byte>(gcmBlockSize);

            while (len(in) >= gcmBlockSize)
            { 
                // Hint to avoid bounds check
                _ = in[15L];
                _ = out[15L];
                g.cipher.Encrypt(mask[..], counter[..]);
                gcmInc32(_addr_counter); 

                // XOR 16 bytes each loop iteration in 8 byte chunks
                var in0 = binary.LittleEndian.Uint64(in[0L..]);
                var in1 = binary.LittleEndian.Uint64(in[8L..]);
                var m0 = binary.LittleEndian.Uint64(mask[..8L]);
                var m1 = binary.LittleEndian.Uint64(mask[8L..]);
                binary.LittleEndian.PutUint64(out[..8L], in0 ^ m0);
                binary.LittleEndian.PutUint64(out[8L..], in1 ^ m1);
                out = out[16L..];
                in = in[16L..];

            }


            if (len(in) > 0L)
            {
                g.cipher.Encrypt(mask[..], counter[..]);
                gcmInc32(_addr_counter); 
                // XOR leftover bytes
                foreach (var (i, inb) in in)
                {
                    out[i] = inb ^ mask[i];
                }

            }

        }

        // increments the rightmost 32-bits of the count value by 1.
        private static void gcmInc32(ptr<array<byte>> _addr_counterBlock)
        {
            ref array<byte> counterBlock = ref _addr_counterBlock.val;

            var c = counterBlock[len(counterBlock) - 4L..];
            var x = binary.BigEndian.Uint32(c) + 1L;
            binary.BigEndian.PutUint32(c, x);
        }

        // paddedGHASH pads data with zeroes until its length is a multiple of
        // 16-bytes. It then calculates a new value for hash using the ghash
        // algorithm.
        private static void paddedGHASH(this ptr<gcmAsm> _addr_g, ptr<array<byte>> _addr_hash, slice<byte> data)
        {
            ref gcmAsm g = ref _addr_g.val;
            ref array<byte> hash = ref _addr_hash.val;

            {
                var siz = len(data) - (len(data) % gcmBlockSize);

                if (siz > 0L)
                {
                    gcmHash(hash[..], _addr_g.productTable, data[..], siz);
                    data = data[siz..];
                }

            }

            if (len(data) > 0L)
            {
                array<byte> s = new array<byte>(16L);
                copy(s[..], data);
                gcmHash(hash[..], _addr_g.productTable, s[..], len(s));
            }

        }

        // auth calculates GHASH(ciphertext, additionalData), masks the result with
        // tagMask and writes the result to out.
        private static void auth(this ptr<gcmAsm> _addr_g, slice<byte> @out, slice<byte> ciphertext, slice<byte> aad, ptr<array<byte>> _addr_tagMask)
        {
            ref gcmAsm g = ref _addr_g.val;
            ref array<byte> tagMask = ref _addr_tagMask.val;

            ref array<byte> hash = ref heap(new array<byte>(16L), out ptr<array<byte>> _addr_hash);
            g.paddedGHASH(_addr_hash, aad);
            g.paddedGHASH(_addr_hash, ciphertext);
            var lens = gcmLengths(uint64(len(aad)) * 8L, uint64(len(ciphertext)) * 8L);
            g.paddedGHASH(_addr_hash, lens[..]);

            copy(out, hash[..]);
            foreach (var (i) in out)
            {
                out[i] ^= tagMask[i];
            }

        }

        // Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
        // details.
        private static slice<byte> Seal(this ptr<gcmAsm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func((_, panic, __) =>
        {
            ref gcmAsm g = ref _addr_g.val;

            if (len(nonce) != g.nonceSize)
            {
                panic("cipher: incorrect nonce length given to GCM");
            }

            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * BlockSize)
            {
                panic("cipher: message too large for GCM");
            }

            var (ret, out) = sliceForAppend(dst, len(plaintext) + g.tagSize);

            ref array<byte> counter = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_counter);            ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);

            g.deriveCounter(_addr_counter, nonce);

            g.cipher.Encrypt(tagMask[..], counter[..]);
            gcmInc32(_addr_counter);

            g.counterCrypt(out, plaintext, _addr_counter);
            g.auth(out[len(plaintext)..], out[..len(plaintext)], data, _addr_tagMask);

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
                panic("cipher: incorrect nonce length given to GCM");
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

            ref array<byte> counter = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_counter);            ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);

            g.deriveCounter(_addr_counter, nonce);

            g.cipher.Encrypt(tagMask[..], counter[..]);
            gcmInc32(_addr_counter);

            array<byte> expectedTag = new array<byte>(gcmTagSize);
            g.auth(expectedTag[..], ciphertext, data, _addr_tagMask);

            var (ret, out) = sliceForAppend(dst, len(ciphertext));

            if (subtle.ConstantTimeCompare(expectedTag[..g.tagSize], tag) != 1L)
            {
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, error.As(errOpen)!);

            }

            g.counterCrypt(out, ciphertext, _addr_counter);
            return (ret, error.As(null!)!);

        });

        private static array<byte> gcmLengths(ulong len0, ulong len1)
        {
            return new array<byte>(new byte[] { byte(len0>>56), byte(len0>>48), byte(len0>>40), byte(len0>>32), byte(len0>>24), byte(len0>>16), byte(len0>>8), byte(len0), byte(len1>>56), byte(len1>>48), byte(len1>>40), byte(len1>>32), byte(len1>>24), byte(len1>>16), byte(len1>>8), byte(len1) });
        }
    }
}}
