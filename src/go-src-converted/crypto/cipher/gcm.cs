// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using alias = crypto.@internal.alias_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using byteorder = @internal.byteorder_package;
using @internal;
using crypto.@internal;

partial class cipher_package {

// AEAD is a cipher mode providing authenticated encryption with associated
// data. For a description of the methodology, see
// https://en.wikipedia.org/wiki/Authenticated_encryption.
[GoType] partial interface AEAD {
    // NonceSize returns the size of the nonce that must be passed to Seal
    // and Open.
    nint NonceSize();
    // Overhead returns the maximum difference between the lengths of a
    // plaintext and its ciphertext.
    nint Overhead();
    // Seal encrypts and authenticates plaintext, authenticates the
    // additional data and appends the result to dst, returning the updated
    // slice. The nonce must be NonceSize() bytes long and unique for all
    // time, for a given key.
    //
    // To reuse plaintext's storage for the encrypted output, use plaintext[:0]
    // as dst. Otherwise, the remaining capacity of dst must not overlap plaintext.
    slice<byte> Seal(slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData);
    // Open decrypts and authenticates ciphertext, authenticates the
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
[GoType] partial interface gcmAble {
    (AEAD, error) NewGCM(nint nonceSize, nint tagSize);
}

// gcmFieldElement represents a value in GF(2¹²⁸). In order to reflect the GCM
// standard and make binary.BigEndian suitable for marshaling these values, the
// bits are stored in big endian order. For example:
//
//	the coefficient of x⁰ can be obtained by v.low >> 63.
//	the coefficient of x⁶³ can be obtained by v.low & 1.
//	the coefficient of x⁶⁴ can be obtained by v.high >> 63.
//	the coefficient of x¹²⁷ can be obtained by v.high & 1.
[GoType] partial struct gcmFieldElement {
    internal uint64 low;
    internal uint64 high;
}

// gcm represents a Galois Counter Mode with a specific key. See
// https://csrc.nist.gov/groups/ST/toolkit/BCM/documents/proposedmodes/gcm/gcm-revised-spec.pdf
[GoType] partial struct gcm {
    internal Block cipher;
    internal nint nonceSize;
    internal nint tagSize;
    // productTable contains the first sixteen powers of the key, H.
    // However, they are in bit reversed order. See NewGCMWithNonceSize.
    internal array<gcmFieldElement> productTable = new(16);
}

// NewGCM returns the given 128-bit, block cipher wrapped in Galois Counter Mode
// with the standard nonce length.
//
// In general, the GHASH operation performed by this implementation of GCM is not constant-time.
// An exception is when the underlying [Block] was created by aes.NewCipher
// on systems with hardware support for AES. See the [crypto/aes] package documentation for details.
public static (AEAD, error) NewGCM(Block cipher) {
    return newGCMWithNonceAndTagSize(cipher, gcmStandardNonceSize, gcmTagSize);
}

// NewGCMWithNonceSize returns the given 128-bit, block cipher wrapped in Galois
// Counter Mode, which accepts nonces of the given length. The length must not
// be zero.
//
// Only use this function if you require compatibility with an existing
// cryptosystem that uses non-standard nonce lengths. All other users should use
// [NewGCM], which is faster and more resistant to misuse.
public static (AEAD, error) NewGCMWithNonceSize(Block cipher, nint size) {
    return newGCMWithNonceAndTagSize(cipher, size, gcmTagSize);
}

// NewGCMWithTagSize returns the given 128-bit, block cipher wrapped in Galois
// Counter Mode, which generates tags with the given length.
//
// Tag sizes between 12 and 16 bytes are allowed.
//
// Only use this function if you require compatibility with an existing
// cryptosystem that uses non-standard tag lengths. All other users should use
// [NewGCM], which is more resistant to misuse.
public static (AEAD, error) NewGCMWithTagSize(Block cipher, nint tagSize) {
    return newGCMWithNonceAndTagSize(cipher, gcmStandardNonceSize, tagSize);
}

internal static (AEAD, error) newGCMWithNonceAndTagSize(Block cipher, nint nonceSize, nint tagSize) {
    if (tagSize < gcmMinimumTagSize || tagSize > gcmBlockSize) {
        return (default!, errors.New("cipher: incorrect tag size given to GCM"u8));
    }
    if (nonceSize <= 0) {
        return (default!, errors.New("cipher: the nonce can't have zero length, or the security of the key will be immediately compromised"u8));
    }
    {
        var (cipherΔ1, ok) = cipher._<gcmAble>(ᐧ); if (ok) {
            return cipherΔ1.NewGCM(nonceSize, tagSize);
        }
    }
    if (cipher.BlockSize() != gcmBlockSize) {
        return (default!, errors.New("cipher: NewGCM requires 128-bit block cipher"u8));
    }
    array<byte> key = new(16); /* gcmBlockSize */
    cipher.Encrypt(key[..], key[..]);
    var g = Ꮡ(new gcm(cipher: cipher, nonceSize: nonceSize, tagSize: tagSize));
    // We precompute 16 multiples of |key|. However, when we do lookups
    // into this table we'll be using bits from a field element and
    // therefore the bits will be in the reverse order. So normally one
    // would expect, say, 4*key to be in index 4 of the table but due to
    // this bit ordering it will actually be in index 0010 (base 2) = 2.
    ref var x = ref heap<gcmFieldElement>(out var Ꮡx);
    x = new gcmFieldElement(
        byteorder.BeUint64(key[..8]),
        byteorder.BeUint64(key[8..])
    );
    (~g).productTable[reverseBits(1)] = x;
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 2; i < 16; i += 2) {
        (~g).productTable[reverseBits(i)] = gcmDouble(Ꮡ(~g).productTable.at<gcmFieldElement>(reverseBits(i / 2)));
        (~g).productTable[reverseBits(i + 1)] = gcmAdd(Ꮡ(~g).productTable.at<gcmFieldElement>(reverseBits(i)), Ꮡx);
    }
    return (~g, default!);
}

internal static readonly UntypedInt gcmBlockSize = 16;
internal static readonly UntypedInt gcmTagSize = 16;
internal static readonly UntypedInt gcmMinimumTagSize = 12; // NIST SP 800-38D recommends tags with 12 or more bytes.
internal static readonly UntypedInt gcmStandardNonceSize = 12;

[GoRecv] internal static nint NonceSize(this ref gcm g) {
    return g.nonceSize;
}

[GoRecv] internal static nint Overhead(this ref gcm g) {
    return g.tagSize;
}

[GoRecv] internal static slice<byte> Seal(this ref gcm g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    if (len(nonce) != g.nonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if (((uint64)len(plaintext)) > ((1 << (int)(32)) - 2) * ((uint64)g.cipher.BlockSize())) {
        throw panic("crypto/cipher: message too large for GCM");
    }
    (ret, @out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
    if (alias.InexactOverlap(@out, plaintext)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    g.deriveCounter(Ꮡcounter, nonce);
    g.cipher.Encrypt(tagMask[..], counter[..]);
    gcmInc32(Ꮡcounter);
    g.counterCrypt(@out, plaintext, Ꮡcounter);
    array<byte> tag = new(16); /* gcmTagSize */
    g.auth(tag[..], @out[..(int)(len(plaintext))], data, ᏑtagMask);
    copy(@out[(int)(len(plaintext))..], tag[..]);
    return ret;
}

internal static error errOpen = errors.New("cipher: message authentication failed"u8);

[GoRecv] internal static (slice<byte>, error) Open(this ref gcm g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    if (len(nonce) != g.nonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    // Sanity check to prevent the authentication from always succeeding if an implementation
    // leaves tagSize uninitialized, for example.
    if (g.tagSize < gcmMinimumTagSize) {
        throw panic("crypto/cipher: incorrect GCM tag size");
    }
    if (len(ciphertext) < g.tagSize) {
        return (default!, errOpen);
    }
    if (((uint64)len(ciphertext)) > ((1 << (int)(32)) - 2) * ((uint64)g.cipher.BlockSize()) + ((uint64)g.tagSize)) {
        return (default!, errOpen);
    }
    var tag = ciphertext[(int)(len(ciphertext) - g.tagSize)..];
    ciphertext = ciphertext[..(int)(len(ciphertext) - g.tagSize)];
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    g.deriveCounter(Ꮡcounter, nonce);
    g.cipher.Encrypt(tagMask[..], counter[..]);
    gcmInc32(Ꮡcounter);
    array<byte> expectedTag = new(16); /* gcmTagSize */
    g.auth(expectedTag[..], ciphertext, data, ᏑtagMask);
    (ret, @out) = sliceForAppend(dst, len(ciphertext));
    if (alias.InexactOverlap(@out, ciphertext)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    if (subtle.ConstantTimeCompare(expectedTag[..(int)(g.tagSize)], tag) != 1) {
        // The AESNI code decrypts and authenticates concurrently, and
        // so overwrites dst in the event of a tag mismatch. That
        // behavior is mimicked here in order to be consistent across
        // platforms.
        clear(@out);
        return (default!, errOpen);
    }
    g.counterCrypt(@out, ciphertext, Ꮡcounter);
    return (ret, default!);
}

// reverseBits reverses the order of the bits of 4-bit number in i.
internal static nint reverseBits(nint i) {
    i = (nint)(((nint)((i << (int)(2)) & 12)) | ((nint)((i >> (int)(2)) & 3)));
    i = (nint)(((nint)((i << (int)(1)) & 10)) | ((nint)((i >> (int)(1)) & 5)));
    return i;
}

// gcmAdd adds two elements of GF(2¹²⁸) and returns the sum.
internal static gcmFieldElement gcmAdd(ж<gcmFieldElement> Ꮡx, ж<gcmFieldElement> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    // Addition in a characteristic 2 field is just XOR.
    return new gcmFieldElement((uint64)(x.low ^ y.low), (uint64)(x.high ^ y.high));
}

// gcmDouble returns the result of doubling an element of GF(2¹²⁸).
internal static gcmFieldElement /*double*/ gcmDouble(ж<gcmFieldElement> Ꮡx) {
    gcmFieldElement @double = default!;

    ref var x = ref Ꮡx.val;
    var msbSet = (uint64)(x.high & 1) == 1;
    // Because of the bit-ordering, doubling is actually a right shift.
    @double.high = x.high >> (int)(1);
    @double.high |= (uint64)(x.low << (int)(63));
    @double.low = x.low >> (int)(1);
    // If the most-significant bit was set before shifting then it,
    // conceptually, becomes a term of x^128. This is greater than the
    // irreducible polynomial so the result has to be reduced. The
    // irreducible polynomial is 1+x+x^2+x^7+x^128. We can subtract that to
    // eliminate the term at x^128 which also means subtracting the other
    // four terms. In characteristic 2 fields, subtraction == addition ==
    // XOR.
    if (msbSet) {
        @double.low ^= (uint64)((nuint)16212958658533785600UL);
    }
    return @double;
}

internal static slice<uint16> gcmReductionTable = new uint16[]{
    0, 7200, 14400, 9312, 28800, 27808, 18624, 21728,
    57600, 64800, 55616, 50528, 37248, 36256, 43456, 46560
}.slice();

// mul sets y to y*H, where H is the GCM key, fixed during NewGCMWithNonceSize.
[GoRecv] internal static void mul(this ref gcm g, ж<gcmFieldElement> Ꮡy) {
    ref var y = ref Ꮡy.val;

    gcmFieldElement z = default!;
    for (nint i = 0; i < 2; i++) {
        var word = y.high;
        if (i == 1) {
            word = y.low;
        }
        // Multiplication works by multiplying z by 16 and adding in
        // one of the precomputed multiples of H.
        for (nint j = 0; j < 64; j += 4) {
            var msw = (uint64)(z.high & 15);
            z.high >>= (UntypedInt)(4);
            z.high |= (uint64)(z.low << (int)(60));
            z.low >>= (UntypedInt)(4);
            z.low ^= (uint64)(((uint64)gcmReductionTable[msw]) << (int)(48));
            // the values in |table| are ordered for
            // little-endian bit positions. See the comment
            // in NewGCMWithNonceSize.
            var t = Ꮡ(g.productTable[(uint64)(word & 15)]);
            z.low ^= (uint64)(t.val.low);
            z.high ^= (uint64)(t.val.high);
            word >>= (UntypedInt)(4);
        }
    }
    y = z;
}

// updateBlocks extends y with more polynomial terms from blocks, based on
// Horner's rule. There must be a multiple of gcmBlockSize bytes in blocks.
[GoRecv] internal static void updateBlocks(this ref gcm g, ж<gcmFieldElement> Ꮡy, slice<byte> blocks) {
    ref var y = ref Ꮡy.val;

    while (len(blocks) > 0) {
        y.low ^= (uint64)(byteorder.BeUint64(blocks));
        y.high ^= (uint64)(byteorder.BeUint64(blocks[8..]));
        g.mul(Ꮡy);
        blocks = blocks[(int)(gcmBlockSize)..];
    }
}

// update extends y with more polynomial terms from data. If data is not a
// multiple of gcmBlockSize bytes long then the remainder is zero padded.
[GoRecv] internal static void update(this ref gcm g, ж<gcmFieldElement> Ꮡy, slice<byte> data) {
    ref var y = ref Ꮡy.val;

    nint fullBlocks = (len(data) >> (int)(4)) << (int)(4);
    g.updateBlocks(Ꮡy, data[..(int)(fullBlocks)]);
    if (len(data) != fullBlocks) {
        array<byte> partialBlock = new(16); /* gcmBlockSize */
        copy(partialBlock[..], data[(int)(fullBlocks)..]);
        g.updateBlocks(Ꮡy, partialBlock[..]);
    }
}

// gcmInc32 treats the final four bytes of counterBlock as a big-endian value
// and increments it.
internal static void gcmInc32(ж<array<byte>> ᏑcounterBlock) {
    ref var counterBlock = ref ᏑcounterBlock.val;

    var ctr = counterBlock[(int)(len(counterBlock) - 4)..];
    byteorder.BePutUint32(ctr, byteorder.BeUint32(ctr) + 1);
}

// sliceForAppend takes a slice and a requested number of bytes. It returns a
// slice with the contents of the given slice followed by that many bytes and a
// second slice that aliases into it and contains only the extra bytes. If the
// original slice has sufficient capacity then no allocation is performed.
internal static (slice<byte> head, slice<byte> tail) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default!;
    slice<byte> tail = default!;

    {
        nint total = len(@in) + n; if (cap(@in) >= total){
            head = @in[..(int)(total)];
        } else {
            head = new slice<byte>(total);
            copy(head, @in);
        }
    }
    tail = head[(int)(len(@in))..];
    return (head, tail);
}

// counterCrypt crypts in to out using g.cipher in counter mode.
[GoRecv] internal static void counterCrypt(this ref gcm g, slice<byte> @out, slice<byte> @in, ж<array<byte>> Ꮡcounter) {
    ref var counter = ref Ꮡcounter.val;

    array<byte> mask = new(16); /* gcmBlockSize */
    while (len(@in) >= gcmBlockSize) {
        g.cipher.Encrypt(mask[..], counter[..]);
        gcmInc32(Ꮡcounter);
        subtle.XORBytes(@out, @in, mask[..]);
        @out = @out[(int)(gcmBlockSize)..];
        @in = @in[(int)(gcmBlockSize)..];
    }
    if (len(@in) > 0) {
        g.cipher.Encrypt(mask[..], counter[..]);
        gcmInc32(Ꮡcounter);
        subtle.XORBytes(@out, @in, mask[..]);
    }
}

// deriveCounter computes the initial GCM counter state from the given nonce.
// See NIST SP 800-38D, section 7.1. This assumes that counter is filled with
// zeros on entry.
[GoRecv] internal static void deriveCounter(this ref gcm g, ж<array<byte>> Ꮡcounter, slice<byte> nonce) {
    ref var counter = ref Ꮡcounter.val;

    // GCM has two modes of operation with respect to the initial counter
    // state: a "fast path" for 96-bit (12-byte) nonces, and a "slow path"
    // for nonces of other lengths. For a 96-bit nonce, the nonce, along
    // with a four-byte big-endian counter starting at one, is used
    // directly as the starting counter. For other nonce sizes, the counter
    // is computed by passing it through the GHASH function.
    if (len(nonce) == gcmStandardNonceSize){
        copy(counter[..], nonce);
        counter[gcmBlockSize - 1] = 1;
    } else {
        ref var y = ref heap(new gcmFieldElement(), out var Ꮡy);
        g.update(Ꮡy, nonce);
        y.high ^= (uint64)(((uint64)len(nonce)) * 8);
        g.mul(Ꮡy);
        byteorder.BePutUint64(counter[..8], y.low);
        byteorder.BePutUint64(counter[8..], y.high);
    }
}

// auth calculates GHASH(ciphertext, additionalData), masks the result with
// tagMask and writes the result to out.
[GoRecv] internal static void auth(this ref gcm g, slice<byte> @out, slice<byte> ciphertext, slice<byte> additionalData, ж<array<byte>> ᏑtagMask) {
    ref var tagMask = ref ᏑtagMask.val;

    ref var y = ref heap(new gcmFieldElement(), out var Ꮡy);
    g.update(Ꮡy, additionalData);
    g.update(Ꮡy, ciphertext);
    y.low ^= (uint64)(((uint64)len(additionalData)) * 8);
    y.high ^= (uint64)(((uint64)len(ciphertext)) * 8);
    g.mul(Ꮡy);
    byteorder.BePutUint64(@out, y.low);
    byteorder.BePutUint64(@out[8..], y.high);
    subtle.XORBytes(@out, @out, tagMask[..]);
}

} // end cipher_package
