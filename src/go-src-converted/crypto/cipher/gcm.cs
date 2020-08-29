// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cipher -- go2cs converted at 2020 August 29 08:28:53 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\gcm.go
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class cipher_package
    {
        // AEAD is a cipher mode providing authenticated encryption with associated
        // data. For a description of the methodology, see
        //    https://en.wikipedia.org/wiki/Authenticated_encryption
        public partial interface AEAD
        {
            (slice<byte>, error) NonceSize(); // Overhead returns the maximum difference between the lengths of a
// plaintext and its ciphertext.
            (slice<byte>, error) Overhead(); // Seal encrypts and authenticates plaintext, authenticates the
// additional data and appends the result to dst, returning the updated
// slice. The nonce must be NonceSize() bytes long and unique for all
// time, for a given key.
//
// The plaintext and dst must overlap exactly or not at all. To reuse
// plaintext's storage for the encrypted output, use plaintext[:0] as dst.
            (slice<byte>, error) Seal(slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData); // Open decrypts and authenticates ciphertext, authenticates the
// additional data and, if successful, appends the resulting plaintext
// to dst, returning the updated slice. The nonce must be NonceSize()
// bytes long and both it and the additional data must match the
// value passed to Seal.
//
// The ciphertext and dst must overlap exactly or not at all. To reuse
// ciphertext's storage for the decrypted output, use ciphertext[:0] as dst.
//
// Even if the function fails, the contents of dst, up to its capacity,
// may be overwritten.
            (slice<byte>, error) Open(slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData);
        }

        // gcmAble is an interface implemented by ciphers that have a specific optimized
        // implementation of GCM, like crypto/aes. NewGCM will check for this interface
        // and return the specific AEAD if found.
        private partial interface gcmAble
        {
            (AEAD, error) NewGCM(long _p0);
        }

        // gcmFieldElement represents a value in GF(2¹²⁸). In order to reflect the GCM
        // standard and make getUint64 suitable for marshaling these values, the bits
        // are stored backwards. For example:
        //   the coefficient of x⁰ can be obtained by v.low >> 63.
        //   the coefficient of x⁶³ can be obtained by v.low & 1.
        //   the coefficient of x⁶⁴ can be obtained by v.high >> 63.
        //   the coefficient of x¹²⁷ can be obtained by v.high & 1.
        private partial struct gcmFieldElement
        {
            public ulong low;
            public ulong high;
        }

        // gcm represents a Galois Counter Mode with a specific key. See
        // http://csrc.nist.gov/groups/ST/toolkit/BCM/documents/proposedmodes/gcm/gcm-revised-spec.pdf
        private partial struct gcm
        {
            public Block cipher;
            public long nonceSize; // productTable contains the first sixteen powers of the key, H.
// However, they are in bit reversed order. See NewGCMWithNonceSize.
            public array<gcmFieldElement> productTable;
        }

        // NewGCM returns the given 128-bit, block cipher wrapped in Galois Counter Mode
        // with the standard nonce length.
        //
        // In general, the GHASH operation performed by this implementation of GCM is not constant-time.
        // An exception is when the underlying Block was created by aes.NewCipher
        // on systems with hardware support for AES. See the crypto/aes package documentation for details.
        public static (AEAD, error) NewGCM(Block cipher)
        {
            return NewGCMWithNonceSize(cipher, gcmStandardNonceSize);
        }

        // NewGCMWithNonceSize returns the given 128-bit, block cipher wrapped in Galois
        // Counter Mode, which accepts nonces of the given length.
        //
        // Only use this function if you require compatibility with an existing
        // cryptosystem that uses non-standard nonce lengths. All other users should use
        // NewGCM, which is faster and more resistant to misuse.
        public static (AEAD, error) NewGCMWithNonceSize(Block cipher, long size)
        {
            {
                gcmAble (cipher, ok) = cipher._<gcmAble>();

                if (ok)
                {
                    return cipher.NewGCM(size);
                }

            }

            if (cipher.BlockSize() != gcmBlockSize)
            {
                return (null, errors.New("cipher: NewGCM requires 128-bit block cipher"));
            }
            array<byte> key = new array<byte>(gcmBlockSize);
            cipher.Encrypt(key[..], key[..]);

            gcm g = ref new gcm(cipher:cipher,nonceSize:size); 

            // We precompute 16 multiples of |key|. However, when we do lookups
            // into this table we'll be using bits from a field element and
            // therefore the bits will be in the reverse order. So normally one
            // would expect, say, 4*key to be in index 4 of the table but due to
            // this bit ordering it will actually be in index 0010 (base 2) = 2.
            gcmFieldElement x = new gcmFieldElement(getUint64(key[:8]),getUint64(key[8:]),);
            g.productTable[reverseBits(1L)] = x;

            {
                long i = 2L;

                while (i < 16L)
                {
                    g.productTable[reverseBits(i)] = gcmDouble(ref g.productTable[reverseBits(i / 2L)]);
                    g.productTable[reverseBits(i + 1L)] = gcmAdd(ref g.productTable[reverseBits(i)], ref x);
                    i += 2L;
                }

            }

            return (g, null);
        }

        private static readonly long gcmBlockSize = 16L;
        private static readonly long gcmTagSize = 16L;
        private static readonly long gcmStandardNonceSize = 12L;

        private static long NonceSize(this ref gcm g)
        {
            return g.nonceSize;
        }

        private static long Overhead(this ref gcm _p0)
        {
            return gcmTagSize;
        }

        private static slice<byte> Seal(this ref gcm _g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func(_g, (ref gcm g, Defer _, Panic panic, Recover __) =>
        {
            if (len(nonce) != g.nonceSize)
            {
                panic("cipher: incorrect nonce length given to GCM");
            }
            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * uint64(g.cipher.BlockSize()))
            {
                panic("cipher: message too large for GCM");
            }
            var (ret, out) = sliceForAppend(dst, len(plaintext) + gcmTagSize);

            array<byte> counter = new array<byte>(gcmBlockSize);            array<byte> tagMask = new array<byte>(gcmBlockSize);

            g.deriveCounter(ref counter, nonce);

            g.cipher.Encrypt(tagMask[..], counter[..]);
            gcmInc32(ref counter);

            g.counterCrypt(out, plaintext, ref counter);
            g.auth(out[len(plaintext)..], out[..len(plaintext)], data, ref tagMask);

            return ret;
        });

        private static var errOpen = errors.New("cipher: message authentication failed");

        private static (slice<byte>, error) Open(this ref gcm _g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func(_g, (ref gcm g, Defer _, Panic panic, Recover __) =>
        {
            if (len(nonce) != g.nonceSize)
            {
                panic("cipher: incorrect nonce length given to GCM");
            }
            if (len(ciphertext) < gcmTagSize)
            {
                return (null, errOpen);
            }
            if (uint64(len(ciphertext)) > ((1L << (int)(32L)) - 2L) * uint64(g.cipher.BlockSize()) + gcmTagSize)
            {
                return (null, errOpen);
            }
            var tag = ciphertext[len(ciphertext) - gcmTagSize..];
            ciphertext = ciphertext[..len(ciphertext) - gcmTagSize];

            array<byte> counter = new array<byte>(gcmBlockSize);            array<byte> tagMask = new array<byte>(gcmBlockSize);

            g.deriveCounter(ref counter, nonce);

            g.cipher.Encrypt(tagMask[..], counter[..]);
            gcmInc32(ref counter);

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

        // reverseBits reverses the order of the bits of 4-bit number in i.
        private static long reverseBits(long i)
        {
            i = ((i << (int)(2L)) & 0xcUL) | ((i >> (int)(2L)) & 0x3UL);
            i = ((i << (int)(1L)) & 0xaUL) | ((i >> (int)(1L)) & 0x5UL);
            return i;
        }

        // gcmAdd adds two elements of GF(2¹²⁸) and returns the sum.
        private static gcmFieldElement gcmAdd(ref gcmFieldElement x, ref gcmFieldElement y)
        { 
            // Addition in a characteristic 2 field is just XOR.
            return new gcmFieldElement(x.low^y.low,x.high^y.high);
        }

        // gcmDouble returns the result of doubling an element of GF(2¹²⁸).
        private static gcmFieldElement gcmDouble(ref gcmFieldElement x)
        {
            var msbSet = x.high & 1L == 1L; 

            // Because of the bit-ordering, doubling is actually a right shift.
            @double.high = x.high >> (int)(1L);
            @double.high |= x.low << (int)(63L);
            @double.low = x.low >> (int)(1L); 

            // If the most-significant bit was set before shifting then it,
            // conceptually, becomes a term of x^128. This is greater than the
            // irreducible polynomial so the result has to be reduced. The
            // irreducible polynomial is 1+x+x^2+x^7+x^128. We can subtract that to
            // eliminate the term at x^128 which also means subtracting the other
            // four terms. In characteristic 2 fields, subtraction == addition ==
            // XOR.
            if (msbSet)
            {
                @double.low ^= 0xe100000000000000UL;
            }
            return;
        }

        private static ushort gcmReductionTable = new slice<ushort>(new ushort[] { 0x0000, 0x1c20, 0x3840, 0x2460, 0x7080, 0x6ca0, 0x48c0, 0x54e0, 0xe100, 0xfd20, 0xd940, 0xc560, 0x9180, 0x8da0, 0xa9c0, 0xb5e0 });

        // mul sets y to y*H, where H is the GCM key, fixed during NewGCMWithNonceSize.
        private static void mul(this ref gcm g, ref gcmFieldElement y)
        {
            gcmFieldElement z = default;

            for (long i = 0L; i < 2L; i++)
            {
                var word = y.high;
                if (i == 1L)
                {
                    word = y.low;
                } 

                // Multiplication works by multiplying z by 16 and adding in
                // one of the precomputed multiples of H.
                {
                    long j = 0L;

                    while (j < 64L)
                    {
                        var msw = z.high & 0xfUL;
                        z.high >>= 4L;
                        z.high |= z.low << (int)(60L);
                        z.low >>= 4L;
                        z.low ^= uint64(gcmReductionTable[msw]) << (int)(48L); 

                        // the values in |table| are ordered for
                        // little-endian bit positions. See the comment
                        // in NewGCMWithNonceSize.
                        var t = ref g.productTable[word & 0xfUL];

                        z.low ^= t.low;
                        z.high ^= t.high;
                        word >>= 4L;
                        j += 4L;
                    }

                }
            }


            y.Value = z;
        }

        // updateBlocks extends y with more polynomial terms from blocks, based on
        // Horner's rule. There must be a multiple of gcmBlockSize bytes in blocks.
        private static void updateBlocks(this ref gcm g, ref gcmFieldElement y, slice<byte> blocks)
        {
            while (len(blocks) > 0L)
            {
                y.low ^= getUint64(blocks);
                y.high ^= getUint64(blocks[8L..]);
                g.mul(y);
                blocks = blocks[gcmBlockSize..];
            }

        }

        // update extends y with more polynomial terms from data. If data is not a
        // multiple of gcmBlockSize bytes long then the remainder is zero padded.
        private static void update(this ref gcm g, ref gcmFieldElement y, slice<byte> data)
        {
            var fullBlocks = (len(data) >> (int)(4L)) << (int)(4L);
            g.updateBlocks(y, data[..fullBlocks]);

            if (len(data) != fullBlocks)
            {
                array<byte> partialBlock = new array<byte>(gcmBlockSize);
                copy(partialBlock[..], data[fullBlocks..]);
                g.updateBlocks(y, partialBlock[..]);
            }
        }

        // gcmInc32 treats the final four bytes of counterBlock as a big-endian value
        // and increments it.
        private static void gcmInc32(ref array<byte> counterBlock)
        {
            for (var i = gcmBlockSize - 1L; i >= gcmBlockSize - 4L; i--)
            {
                counterBlock[i]++;
                if (counterBlock[i] != 0L)
                {
                    break;
                }
            }

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

        // counterCrypt crypts in to out using g.cipher in counter mode.
        private static void counterCrypt(this ref gcm g, slice<byte> @out, slice<byte> @in, ref array<byte> counter)
        {
            array<byte> mask = new array<byte>(gcmBlockSize);

            while (len(in) >= gcmBlockSize)
            {
                g.cipher.Encrypt(mask[..], counter[..]);
                gcmInc32(counter);

                xorWords(out, in, mask[..]);
                out = out[gcmBlockSize..];
                in = in[gcmBlockSize..];
            }


            if (len(in) > 0L)
            {
                g.cipher.Encrypt(mask[..], counter[..]);
                gcmInc32(counter);
                xorBytes(out, in, mask[..]);
            }
        }

        // deriveCounter computes the initial GCM counter state from the given nonce.
        // See NIST SP 800-38D, section 7.1. This assumes that counter is filled with
        // zeros on entry.
        private static void deriveCounter(this ref gcm g, ref array<byte> counter, slice<byte> nonce)
        { 
            // GCM has two modes of operation with respect to the initial counter
            // state: a "fast path" for 96-bit (12-byte) nonces, and a "slow path"
            // for nonces of other lengths. For a 96-bit nonce, the nonce, along
            // with a four-byte big-endian counter starting at one, is used
            // directly as the starting counter. For other nonce sizes, the counter
            // is computed by passing it through the GHASH function.
            if (len(nonce) == gcmStandardNonceSize)
            {
                copy(counter[..], nonce);
                counter[gcmBlockSize - 1L] = 1L;
            }
            else
            {
                gcmFieldElement y = default;
                g.update(ref y, nonce);
                y.high ^= uint64(len(nonce)) * 8L;
                g.mul(ref y);
                putUint64(counter[..8L], y.low);
                putUint64(counter[8L..], y.high);
            }
        }

        // auth calculates GHASH(ciphertext, additionalData), masks the result with
        // tagMask and writes the result to out.
        private static void auth(this ref gcm g, slice<byte> @out, slice<byte> ciphertext, slice<byte> additionalData, ref array<byte> tagMask)
        {
            gcmFieldElement y = default;
            g.update(ref y, additionalData);
            g.update(ref y, ciphertext);

            y.low ^= uint64(len(additionalData)) * 8L;
            y.high ^= uint64(len(ciphertext)) * 8L;

            g.mul(ref y);

            putUint64(out, y.low);
            putUint64(out[8L..], y.high);

            xorWords(out, out, tagMask[..]);
        }

        private static ulong getUint64(slice<byte> data)
        {
            var r = uint64(data[0L]) << (int)(56L) | uint64(data[1L]) << (int)(48L) | uint64(data[2L]) << (int)(40L) | uint64(data[3L]) << (int)(32L) | uint64(data[4L]) << (int)(24L) | uint64(data[5L]) << (int)(16L) | uint64(data[6L]) << (int)(8L) | uint64(data[7L]);
            return r;
        }

        private static void putUint64(slice<byte> @out, ulong v)
        {
            out[0L] = byte(v >> (int)(56L));
            out[1L] = byte(v >> (int)(48L));
            out[2L] = byte(v >> (int)(40L));
            out[3L] = byte(v >> (int)(32L));
            out[4L] = byte(v >> (int)(24L));
            out[5L] = byte(v >> (int)(16L));
            out[6L] = byte(v >> (int)(8L));
            out[7L] = byte(v);
        }
    }
}}
