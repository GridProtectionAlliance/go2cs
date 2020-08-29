// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:50 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\gcm_s390x.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // This file contains two implementations of AES-GCM. The first implementation
        // (gcmAsm) uses the KMCTR instruction to encrypt using AES in counter mode and
        // the KIMD instruction for GHASH. The second implementation (gcmKMA) uses the
        // newer KMA instruction which performs both operations.

        // gcmCount represents a 16-byte big-endian count value.
        private partial struct gcmCount // : array<byte>
        {
        }

        // inc increments the rightmost 32-bits of the count value by 1.
        private static void inc(this ref gcmCount x)
        { 
            // The compiler should optimize this to a 32-bit addition.
            var n = uint32(x[15L]) | uint32(x[14L]) << (int)(8L) | uint32(x[13L]) << (int)(16L) | uint32(x[12L]) << (int)(24L);
            n += 1L;
            x[12L] = byte(n >> (int)(24L));
            x[13L] = byte(n >> (int)(16L));
            x[14L] = byte(n >> (int)(8L));
            x[15L] = byte(n);
        }

        // gcmLengths writes len0 || len1 as big-endian values to a 16-byte array.
        private static array<byte> gcmLengths(ulong len0, ulong len1)
        {
            return new array<byte>(new byte[] { byte(len0>>56), byte(len0>>48), byte(len0>>40), byte(len0>>32), byte(len0>>24), byte(len0>>16), byte(len0>>8), byte(len0), byte(len1>>56), byte(len1>>48), byte(len1>>40), byte(len1>>32), byte(len1>>24), byte(len1>>16), byte(len1>>8), byte(len1) });
        }

        // gcmHashKey represents the 16-byte hash key required by the GHASH algorithm.
        private partial struct gcmHashKey // : array<byte>
        {
        }

        private partial struct gcmAsm
        {
            public ptr<aesCipherAsm> block;
            public gcmHashKey hashKey;
            public long nonceSize;
        }

        private static readonly long gcmBlockSize = 16L;
        private static readonly long gcmTagSize = 16L;
        private static readonly long gcmStandardNonceSize = 12L;

        private static var errOpen = errors.New("cipher: message authentication failed");

        // Assert that aesCipherAsm implements the gcmAble interface.
        private static gcmAble _ = (aesCipherAsm.Value)(null);

        // NewGCM returns the AES cipher wrapped in Galois Counter Mode. This is only
        // called by crypto/cipher.NewGCM via the gcmAble interface.
        private static (cipher.AEAD, error) NewGCM(this ref aesCipherAsm c, long nonceSize)
        {
            gcmHashKey hk = default;
            c.Encrypt(hk[..], hk[..]);
            gcmAsm g = new gcmAsm(block:c,hashKey:hk,nonceSize:nonceSize,);
            if (hasKMA)
            {
                g = new gcmKMA(g);
                return (ref g, null);
            }
            return (ref g, null);
        }

        private static long NonceSize(this ref gcmAsm g)
        {
            return g.nonceSize;
        }

        private static long Overhead(this ref gcmAsm _p0)
        {
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
                {
                    head = in[..total];
                }
                else
                {
                    head = make_slice<byte>(total);
                    copy(head, in);
                }

            }
            tail = head[len(in)..];
            return;
        }

        // ghash uses the GHASH algorithm to hash data with the given key. The initial
        // hash value is given by hash which will be updated with the new hash value.
        // The length of data must be a multiple of 16-bytes.
        //go:noescape
        private static void ghash(ref gcmHashKey key, ref array<byte> hash, slice<byte> data)
;

        // paddedGHASH pads data with zeroes until its length is a multiple of
        // 16-bytes. It then calculates a new value for hash using the GHASH algorithm.
        private static void paddedGHASH(this ref gcmAsm g, ref array<byte> hash, slice<byte> data)
        {>>MARKER:FUNCTION_ghash_BLOCK_PREFIX<<
            var siz = len(data) & ~0xfUL; // align size to 16-bytes
            if (siz > 0L)
            {
                ghash(ref g.hashKey, hash, data[..siz]);
                data = data[siz..];
            }
            if (len(data) > 0L)
            {
                array<byte> s = new array<byte>(16L);
                copy(s[..], data);
                ghash(ref g.hashKey, hash, s[..]);
            }
        }

        // cryptBlocksGCM encrypts src using AES in counter mode using the given
        // function code and key. The rightmost 32-bits of the counter are incremented
        // between each block as required by the GCM spec. The initial counter value
        // is given by cnt, which is updated with the value of the next counter value
        // to use.
        //
        // The lengths of both dst and buf must be greater than or equal to the length
        // of src. buf may be partially or completely overwritten during the execution
        // of the function.
        //go:noescape
        private static void cryptBlocksGCM(code fn, slice<byte> key, slice<byte> dst, slice<byte> src, slice<byte> buf, ref gcmCount cnt)
;

        // counterCrypt encrypts src using AES in counter mode and places the result
        // into dst. cnt is the initial count value and will be updated with the next
        // count value. The length of dst must be greater than or equal to the length
        // of src.
        private static void counterCrypt(this ref gcmAsm g, slice<byte> dst, slice<byte> src, ref gcmCount cnt)
        {>>MARKER:FUNCTION_cryptBlocksGCM_BLOCK_PREFIX<< 
            // Copying src into a buffer improves performance on some models when
            // src and dst point to the same underlying array. We also need a
            // buffer for counter values.
            array<byte> ctrbuf = new array<byte>(2048L);            array<byte> srcbuf = new array<byte>(2048L);

            while (len(src) >= 16L)
            {
                var siz = len(src);
                if (len(src) > len(ctrbuf))
                {
                    siz = len(ctrbuf);
                }
                siz &= 0xfUL; // align siz to 16-bytes
                copy(srcbuf[..], src[..siz]);
                cryptBlocksGCM(g.block.function, g.block.key, dst[..siz], srcbuf[..siz], ctrbuf[..], cnt);
                src = src[siz..];
                dst = dst[siz..];
            }

            if (len(src) > 0L)
            {
                array<byte> x = new array<byte>(16L);
                g.block.Encrypt(x[..], cnt[..]);
                foreach (var (i) in src)
                {
                    dst[i] = src[i] ^ x[i];
                }
                cnt.inc();
            }
        }

        // deriveCounter computes the initial GCM counter state from the given nonce.
        // See NIST SP 800-38D, section 7.1.
        private static gcmCount deriveCounter(this ref gcmAsm g, slice<byte> nonce)
        { 
            // GCM has two modes of operation with respect to the initial counter
            // state: a "fast path" for 96-bit (12-byte) nonces, and a "slow path"
            // for nonces of other lengths. For a 96-bit nonce, the nonce, along
            // with a four-byte big-endian counter starting at one, is used
            // directly as the starting counter. For other nonce sizes, the counter
            // is computed by passing it through the GHASH function.
            gcmCount counter = default;
            if (len(nonce) == gcmStandardNonceSize)
            {
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;
            }
            else
            {
                array<byte> hash = new array<byte>(16L);
                g.paddedGHASH(ref hash, nonce);
                var lens = gcmLengths(0L, uint64(len(nonce)) * 8L);
                g.paddedGHASH(ref hash, lens[..]);
                copy(counter[..], hash[..]);
            }
            return counter;
        }

        // auth calculates GHASH(ciphertext, additionalData), masks the result with
        // tagMask and writes the result to out.
        private static void auth(this ref gcmAsm g, slice<byte> @out, slice<byte> ciphertext, slice<byte> additionalData, ref array<byte> tagMask)
        {
            array<byte> hash = new array<byte>(16L);
            g.paddedGHASH(ref hash, additionalData);
            g.paddedGHASH(ref hash, ciphertext);
            var lens = gcmLengths(uint64(len(additionalData)) * 8L, uint64(len(ciphertext)) * 8L);
            g.paddedGHASH(ref hash, lens[..]);

            copy(out, hash[..]);
            foreach (var (i) in out)
            {
                out[i] ^= tagMask[i];
            }
        }

        // Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
        // details.
        private static slice<byte> Seal(this ref gcmAsm _g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func(_g, (ref gcmAsm g, Defer _, Panic panic, Recover __) =>
        {
            if (len(nonce) != g.nonceSize)
            {
                panic("cipher: incorrect nonce length given to GCM");
            }
            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * BlockSize)
            {
                panic("cipher: message too large for GCM");
            }
            var (ret, out) = sliceForAppend(dst, len(plaintext) + gcmTagSize);

            var counter = g.deriveCounter(nonce);

            array<byte> tagMask = new array<byte>(gcmBlockSize);
            g.block.Encrypt(tagMask[..], counter[..]);
            counter.inc();

            g.counterCrypt(out, plaintext, ref counter);
            g.auth(out[len(plaintext)..], out[..len(plaintext)], data, ref tagMask);

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

            var counter = g.deriveCounter(nonce);

            array<byte> tagMask = new array<byte>(gcmBlockSize);
            g.block.Encrypt(tagMask[..], counter[..]);
            counter.inc();

            array<byte> expectedTag = new array<byte>(gcmTagSize);
            g.auth(expectedTag[..], ciphertext, data, ref tagMask);

            var (ret, out) = sliceForAppend(dst, len(ciphertext));

            if (subtle.ConstantTimeCompare(expectedTag[..], tag) != 1L)
            { 
                // The AESNI code decrypts and authenticates concurrently, and
                // so overwrites dst in the event of a tag mismatch. That
                // behavior is mimicked here in order to be consistent across
                // platforms.
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, errOpen);
            }
            g.counterCrypt(out, ciphertext, ref counter);
            return (ret, null);
        });

        // supportsKMA reports whether the message-security-assist 8 facility is available.
        // This function call may be expensive so hasKMA should be queried instead.
        private static bool supportsKMA()
;

        // hasKMA contains the result of supportsKMA.
        private static var hasKMA = supportsKMA();

        // gcmKMA implements the cipher.AEAD interface using the KMA instruction. It should
        // only be used if hasKMA is true.
        private partial struct gcmKMA
        {
            public ref gcmAsm gcmAsm => ref gcmAsm_val;
        }

        // flags for the KMA instruction
        private static readonly long kmaHS = 1L << (int)(10L); // hash subkey supplied
        private static readonly long kmaLAAD = 1L << (int)(9L); // last series of additional authenticated data
        private static readonly long kmaLPC = 1L << (int)(8L); // last series of plaintext or ciphertext blocks
        private static readonly long kmaDecrypt = 1L << (int)(7L); // decrypt

        // kmaGCM executes the encryption or decryption operation given by fn. The tag
        // will be calculated and written to tag. cnt should contain the current
        // counter state and will be overwritten with the updated counter state.
        // TODO(mundaym): could pass in hash subkey
        //go:noescape
        private static void kmaGCM(code fn, slice<byte> key, slice<byte> dst, slice<byte> src, slice<byte> aad, ref array<byte> tag, ref gcmCount cnt)
;

        // Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
        // details.
        private static slice<byte> Seal(this ref gcmKMA _g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func(_g, (ref gcmKMA g, Defer _, Panic panic, Recover __) =>
        {>>MARKER:FUNCTION_kmaGCM_BLOCK_PREFIX<<
            if (len(nonce) != g.nonceSize)
            {>>MARKER:FUNCTION_supportsKMA_BLOCK_PREFIX<<
                panic("cipher: incorrect nonce length given to GCM");
            }
            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * BlockSize)
            {
                panic("cipher: message too large for GCM");
            }
            var (ret, out) = sliceForAppend(dst, len(plaintext) + gcmTagSize);

            var counter = g.deriveCounter(nonce);
            var fc = g.block.function | kmaLAAD | kmaLPC;

            array<byte> tag = new array<byte>(gcmTagSize);
            kmaGCM(fc, g.block.key, out[..len(plaintext)], plaintext, data, ref tag, ref counter);
            copy(out[len(plaintext)..], tag[..]);

            return ret;
        });

        // Open authenticates and decrypts ciphertext. See the cipher.AEAD interface
        // for details.
        private static (slice<byte>, error) Open(this ref gcmKMA _g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func(_g, (ref gcmKMA g, Defer _, Panic panic, Recover __) =>
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
            var (ret, out) = sliceForAppend(dst, len(ciphertext));

            var counter = g.deriveCounter(nonce);
            var fc = g.block.function | kmaLAAD | kmaLPC | kmaDecrypt;

            array<byte> expectedTag = new array<byte>(gcmTagSize);
            kmaGCM(fc, g.block.key, out[..len(ciphertext)], ciphertext, data, ref expectedTag, ref counter);

            if (subtle.ConstantTimeCompare(expectedTag[..], tag) != 1L)
            { 
                // The AESNI code decrypts and authenticates concurrently, and
                // so overwrites dst in the event of a tag mismatch. That
                // behavior is mimicked here in order to be consistent across
                // platforms.
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
