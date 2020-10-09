// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cipher -- go2cs converted at 2020 October 09 04:53:43 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\gcm.go
using subtleoverlap = go.crypto.@internal.subtle_package;
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
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
// To reuse plaintext's storage for the encrypted output, use plaintext[:0]
// as dst. Otherwise, the remaining capacity of dst must not overlap plaintext.
            (slice<byte>, error) Seal(slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData); // Open decrypts and authenticates ciphertext, authenticates the
// additional data and, if successful, appends the resulting plaintext
// to dst, returning the updated slice. The nonce must be NonceSize()
// bytes long and both it and the additional data must match the
// value passed to Seal.
//
// To reuse ciphertext's storage for the decrypted output, use ciphertext[:0]
// as dst. Otherwise, the remaining capacity of dst must not overlap plaintext.
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
            (AEAD, error) NewGCM(long nonceSize, long tagSize);
        }

        // gcmFieldElement represents a value in GF(2¹²⁸). In order to reflect the GCM
        // standard and make binary.BigEndian suitable for marshaling these values, the
        // bits are stored in big endian order. For example:
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
        // https://csrc.nist.gov/groups/ST/toolkit/BCM/documents/proposedmodes/gcm/gcm-revised-spec.pdf
        private partial struct gcm
        {
            public Block cipher;
            public long nonceSize;
            public long tagSize; // productTable contains the first sixteen powers of the key, H.
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
            AEAD _p0 = default;
            error _p0 = default!;

            return newGCMWithNonceAndTagSize(cipher, gcmStandardNonceSize, gcmTagSize);
        }

        // NewGCMWithNonceSize returns the given 128-bit, block cipher wrapped in Galois
        // Counter Mode, which accepts nonces of the given length. The length must not
        // be zero.
        //
        // Only use this function if you require compatibility with an existing
        // cryptosystem that uses non-standard nonce lengths. All other users should use
        // NewGCM, which is faster and more resistant to misuse.
        public static (AEAD, error) NewGCMWithNonceSize(Block cipher, long size)
        {
            AEAD _p0 = default;
            error _p0 = default!;

            return newGCMWithNonceAndTagSize(cipher, size, gcmTagSize);
        }

        // NewGCMWithTagSize returns the given 128-bit, block cipher wrapped in Galois
        // Counter Mode, which generates tags with the given length.
        //
        // Tag sizes between 12 and 16 bytes are allowed.
        //
        // Only use this function if you require compatibility with an existing
        // cryptosystem that uses non-standard tag lengths. All other users should use
        // NewGCM, which is more resistant to misuse.
        public static (AEAD, error) NewGCMWithTagSize(Block cipher, long tagSize)
        {
            AEAD _p0 = default;
            error _p0 = default!;

            return newGCMWithNonceAndTagSize(cipher, gcmStandardNonceSize, tagSize);
        }

        private static (AEAD, error) newGCMWithNonceAndTagSize(Block cipher, long nonceSize, long tagSize)
        {
            AEAD _p0 = default;
            error _p0 = default!;

            if (tagSize < gcmMinimumTagSize || tagSize > gcmBlockSize)
            {
                return (null, error.As(errors.New("cipher: incorrect tag size given to GCM"))!);
            }

            if (nonceSize <= 0L)
            {
                return (null, error.As(errors.New("cipher: the nonce can't have zero length, or the security of the key will be immediately compromised"))!);
            }

            {
                gcmAble (cipher, ok) = gcmAble.As(cipher._<gcmAble>())!;

                if (ok)
                {
                    return cipher.NewGCM(nonceSize, tagSize);
                }

            }


            if (cipher.BlockSize() != gcmBlockSize)
            {
                return (null, error.As(errors.New("cipher: NewGCM requires 128-bit block cipher"))!);
            }

            array<byte> key = new array<byte>(gcmBlockSize);
            cipher.Encrypt(key[..], key[..]);

            ptr<gcm> g = addr(new gcm(cipher:cipher,nonceSize:nonceSize,tagSize:tagSize)); 

            // We precompute 16 multiples of |key|. However, when we do lookups
            // into this table we'll be using bits from a field element and
            // therefore the bits will be in the reverse order. So normally one
            // would expect, say, 4*key to be in index 4 of the table but due to
            // this bit ordering it will actually be in index 0010 (base 2) = 2.
            ref gcmFieldElement x = ref heap(new gcmFieldElement(binary.BigEndian.Uint64(key[:8]),binary.BigEndian.Uint64(key[8:]),), out ptr<gcmFieldElement> _addr_x);
            g.productTable[reverseBits(1L)] = x;

            {
                long i = 2L;

                while (i < 16L)
                {
                    g.productTable[reverseBits(i)] = gcmDouble(_addr_g.productTable[reverseBits(i / 2L)]);
                    g.productTable[reverseBits(i + 1L)] = gcmAdd(_addr_g.productTable[reverseBits(i)], _addr_x);
                    i += 2L;
                }

            }

            return (g, error.As(null!)!);

        }

        private static readonly long gcmBlockSize = (long)16L;
        private static readonly long gcmTagSize = (long)16L;
        private static readonly long gcmMinimumTagSize = (long)12L; // NIST SP 800-38D recommends tags with 12 or more bytes.
        private static readonly long gcmStandardNonceSize = (long)12L;


        private static long NonceSize(this ptr<gcm> _addr_g)
        {
            ref gcm g = ref _addr_g.val;

            return g.nonceSize;
        }

        private static long Overhead(this ptr<gcm> _addr_g)
        {
            ref gcm g = ref _addr_g.val;

            return g.tagSize;
        }

        private static slice<byte> Seal(this ptr<gcm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func((_, panic, __) =>
        {
            ref gcm g = ref _addr_g.val;

            if (len(nonce) != g.nonceSize)
            {
                panic("crypto/cipher: incorrect nonce length given to GCM");
            }

            if (uint64(len(plaintext)) > ((1L << (int)(32L)) - 2L) * uint64(g.cipher.BlockSize()))
            {
                panic("crypto/cipher: message too large for GCM");
            }

            var (ret, out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
            if (subtleoverlap.InexactOverlap(out, plaintext))
            {
                panic("crypto/cipher: invalid buffer overlap");
            }

            ref array<byte> counter = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_counter);            ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);

            g.deriveCounter(_addr_counter, nonce);

            g.cipher.Encrypt(tagMask[..], counter[..]);
            gcmInc32(_addr_counter);

            g.counterCrypt(out, plaintext, _addr_counter);

            array<byte> tag = new array<byte>(gcmTagSize);
            g.auth(tag[..], out[..len(plaintext)], data, _addr_tagMask);
            copy(out[len(plaintext)..], tag[..]);

            return ret;

        });

        private static var errOpen = errors.New("cipher: message authentication failed");

        private static (slice<byte>, error) Open(this ptr<gcm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func((_, panic, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref gcm g = ref _addr_g.val;

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

            if (uint64(len(ciphertext)) > ((1L << (int)(32L)) - 2L) * uint64(g.cipher.BlockSize()) + uint64(g.tagSize))
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
            if (subtleoverlap.InexactOverlap(out, ciphertext))
            {
                panic("crypto/cipher: invalid buffer overlap");
            }

            if (subtle.ConstantTimeCompare(expectedTag[..g.tagSize], tag) != 1L)
            { 
                // The AESNI code decrypts and authenticates concurrently, and
                // so overwrites dst in the event of a tag mismatch. That
                // behavior is mimicked here in order to be consistent across
                // platforms.
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, error.As(errOpen)!);

            }

            g.counterCrypt(out, ciphertext, _addr_counter);

            return (ret, error.As(null!)!);

        });

        // reverseBits reverses the order of the bits of 4-bit number in i.
        private static long reverseBits(long i)
        {
            i = ((i << (int)(2L)) & 0xcUL) | ((i >> (int)(2L)) & 0x3UL);
            i = ((i << (int)(1L)) & 0xaUL) | ((i >> (int)(1L)) & 0x5UL);
            return i;
        }

        // gcmAdd adds two elements of GF(2¹²⁸) and returns the sum.
        private static gcmFieldElement gcmAdd(ptr<gcmFieldElement> _addr_x, ptr<gcmFieldElement> _addr_y)
        {
            ref gcmFieldElement x = ref _addr_x.val;
            ref gcmFieldElement y = ref _addr_y.val;
 
            // Addition in a characteristic 2 field is just XOR.
            return new gcmFieldElement(x.low^y.low,x.high^y.high);

        }

        // gcmDouble returns the result of doubling an element of GF(2¹²⁸).
        private static gcmFieldElement gcmDouble(ptr<gcmFieldElement> _addr_x)
        {
            gcmFieldElement @double = default;
            ref gcmFieldElement x = ref _addr_x.val;

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

            return ;

        }

        private static ushort gcmReductionTable = new slice<ushort>(new ushort[] { 0x0000, 0x1c20, 0x3840, 0x2460, 0x7080, 0x6ca0, 0x48c0, 0x54e0, 0xe100, 0xfd20, 0xd940, 0xc560, 0x9180, 0x8da0, 0xa9c0, 0xb5e0 });

        // mul sets y to y*H, where H is the GCM key, fixed during NewGCMWithNonceSize.
        private static void mul(this ptr<gcm> _addr_g, ptr<gcmFieldElement> _addr_y)
        {
            ref gcm g = ref _addr_g.val;
            ref gcmFieldElement y = ref _addr_y.val;

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
                        var t = _addr_g.productTable[word & 0xfUL];

                        z.low ^= t.low;
                        z.high ^= t.high;
                        word >>= 4L;
                        j += 4L;
                    }

                }

            }


            y = z;

        }

        // updateBlocks extends y with more polynomial terms from blocks, based on
        // Horner's rule. There must be a multiple of gcmBlockSize bytes in blocks.
        private static void updateBlocks(this ptr<gcm> _addr_g, ptr<gcmFieldElement> _addr_y, slice<byte> blocks)
        {
            ref gcm g = ref _addr_g.val;
            ref gcmFieldElement y = ref _addr_y.val;

            while (len(blocks) > 0L)
            {
                y.low ^= binary.BigEndian.Uint64(blocks);
                y.high ^= binary.BigEndian.Uint64(blocks[8L..]);
                g.mul(y);
                blocks = blocks[gcmBlockSize..];
            }


        }

        // update extends y with more polynomial terms from data. If data is not a
        // multiple of gcmBlockSize bytes long then the remainder is zero padded.
        private static void update(this ptr<gcm> _addr_g, ptr<gcmFieldElement> _addr_y, slice<byte> data)
        {
            ref gcm g = ref _addr_g.val;
            ref gcmFieldElement y = ref _addr_y.val;

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
        private static void gcmInc32(ptr<array<byte>> _addr_counterBlock)
        {
            ref array<byte> counterBlock = ref _addr_counterBlock.val;

            var ctr = counterBlock[len(counterBlock) - 4L..];
            binary.BigEndian.PutUint32(ctr, binary.BigEndian.Uint32(ctr) + 1L);
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
            return ;

        }

        // counterCrypt crypts in to out using g.cipher in counter mode.
        private static void counterCrypt(this ptr<gcm> _addr_g, slice<byte> @out, slice<byte> @in, ptr<array<byte>> _addr_counter)
        {
            ref gcm g = ref _addr_g.val;
            ref array<byte> counter = ref _addr_counter.val;

            array<byte> mask = new array<byte>(gcmBlockSize);

            while (len(in) >= gcmBlockSize)
            {
                g.cipher.Encrypt(mask[..], counter[..]);
                gcmInc32(_addr_counter);

                xorWords(out, in, mask[..]);
                out = out[gcmBlockSize..];
                in = in[gcmBlockSize..];
            }


            if (len(in) > 0L)
            {
                g.cipher.Encrypt(mask[..], counter[..]);
                gcmInc32(_addr_counter);
                xorBytes(out, in, mask[..]);
            }

        }

        // deriveCounter computes the initial GCM counter state from the given nonce.
        // See NIST SP 800-38D, section 7.1. This assumes that counter is filled with
        // zeros on entry.
        private static void deriveCounter(this ptr<gcm> _addr_g, ptr<array<byte>> _addr_counter, slice<byte> nonce)
        {
            ref gcm g = ref _addr_g.val;
            ref array<byte> counter = ref _addr_counter.val;
 
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
                ref gcmFieldElement y = ref heap(out ptr<gcmFieldElement> _addr_y);
                g.update(_addr_y, nonce);
                y.high ^= uint64(len(nonce)) * 8L;
                g.mul(_addr_y);
                binary.BigEndian.PutUint64(counter[..8L], y.low);
                binary.BigEndian.PutUint64(counter[8L..], y.high);
            }

        }

        // auth calculates GHASH(ciphertext, additionalData), masks the result with
        // tagMask and writes the result to out.
        private static void auth(this ptr<gcm> _addr_g, slice<byte> @out, slice<byte> ciphertext, slice<byte> additionalData, ptr<array<byte>> _addr_tagMask)
        {
            ref gcm g = ref _addr_g.val;
            ref array<byte> tagMask = ref _addr_tagMask.val;

            ref gcmFieldElement y = ref heap(out ptr<gcmFieldElement> _addr_y);
            g.update(_addr_y, additionalData);
            g.update(_addr_y, ciphertext);

            y.low ^= uint64(len(additionalData)) * 8L;
            y.high ^= uint64(len(ciphertext)) * 8L;

            g.mul(_addr_y);

            binary.BigEndian.PutUint64(out, y.low);
            binary.BigEndian.PutUint64(out[8L..], y.high);

            xorWords(out, out, tagMask[..]);
        }
    }
}}
