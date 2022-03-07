// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2022 March 06 22:18:17 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\gcm_s390x.go
using cipher = go.crypto.cipher_package;
using subtleoverlap = go.crypto.@internal.subtle_package;
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using cpu = go.@internal.cpu_package;

namespace go.crypto;

public static partial class aes_package {

    // This file contains two implementations of AES-GCM. The first implementation
    // (gcmAsm) uses the KMCTR instruction to encrypt using AES in counter mode and
    // the KIMD instruction for GHASH. The second implementation (gcmKMA) uses the
    // newer KMA instruction which performs both operations.

    // gcmCount represents a 16-byte big-endian count value.
private partial struct gcmCount { // : array<byte>
}

// inc increments the rightmost 32-bits of the count value by 1.
private static void inc(this ptr<gcmCount> _addr_x) {
    ref gcmCount x = ref _addr_x.val;

    binary.BigEndian.PutUint32(x[(int)len(x) - 4..], binary.BigEndian.Uint32(x[(int)len(x) - 4..]) + 1);
}

// gcmLengths writes len0 || len1 as big-endian values to a 16-byte array.
private static array<byte> gcmLengths(ulong len0, ulong len1) {
    array<byte> v = new array<byte>(new byte[] {  });
    binary.BigEndian.PutUint64(v[(int)0..], len0);
    binary.BigEndian.PutUint64(v[(int)8..], len1);
    return v;
}

// gcmHashKey represents the 16-byte hash key required by the GHASH algorithm.
private partial struct gcmHashKey { // : array<byte>
}

private partial struct gcmAsm {
    public ptr<aesCipherAsm> block;
    public gcmHashKey hashKey;
    public nint nonceSize;
    public nint tagSize;
}

private static readonly nint gcmBlockSize = 16;
private static readonly nint gcmTagSize = 16;
private static readonly nint gcmMinimumTagSize = 12; // NIST SP 800-38D recommends tags with 12 or more bytes.
private static readonly nint gcmStandardNonceSize = 12;


private static var errOpen = errors.New("cipher: message authentication failed");

// Assert that aesCipherAsm implements the gcmAble interface.
private static gcmAble _ = (aesCipherAsm.val)(null);

// NewGCM returns the AES cipher wrapped in Galois Counter Mode. This is only
// called by crypto/cipher.NewGCM via the gcmAble interface.
private static (cipher.AEAD, error) NewGCM(this ptr<aesCipherAsm> _addr_c, nint nonceSize, nint tagSize) {
    cipher.AEAD _p0 = default;
    error _p0 = default!;
    ref aesCipherAsm c = ref _addr_c.val;

    gcmHashKey hk = default;
    c.Encrypt(hk[..], hk[..]);
    ref gcmAsm g = ref heap(new gcmAsm(block:c,hashKey:hk,nonceSize:nonceSize,tagSize:tagSize,), out ptr<gcmAsm> _addr_g);
    if (cpu.S390X.HasAESGCM) {
        g = new gcmKMA(g);
        return (_addr_g, error.As(null!)!);
    }
    return (_addr_g, error.As(null!)!);

}

private static nint NonceSize(this ptr<gcmAsm> _addr_g) {
    ref gcmAsm g = ref _addr_g.val;

    return g.nonceSize;
}

private static nint Overhead(this ptr<gcmAsm> _addr_g) {
    ref gcmAsm g = ref _addr_g.val;

    return g.tagSize;
}

// sliceForAppend takes a slice and a requested number of bytes. It returns a
// slice with the contents of the given slice followed by that many bytes and a
// second slice that aliases into it and contains only the extra bytes. If the
// original slice has sufficient capacity then no allocation is performed.
private static (slice<byte>, slice<byte>) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default;
    slice<byte> tail = default;

    {
        var total = len(in) + n;

        if (cap(in) >= total) {
            head = in[..(int)total];
        }
        else
 {
            head = make_slice<byte>(total);
            copy(head, in);
        }
    }

    tail = head[(int)len(in)..];
    return ;

}

// ghash uses the GHASH algorithm to hash data with the given key. The initial
// hash value is given by hash which will be updated with the new hash value.
// The length of data must be a multiple of 16-bytes.
//go:noescape
private static void ghash(ptr<gcmHashKey> key, ptr<array<byte>> hash, slice<byte> data);

// paddedGHASH pads data with zeroes until its length is a multiple of
// 16-bytes. It then calculates a new value for hash using the GHASH algorithm.
private static void paddedGHASH(this ptr<gcmAsm> _addr_g, ptr<array<byte>> _addr_hash, slice<byte> data) {
    ref gcmAsm g = ref _addr_g.val;
    ref array<byte> hash = ref _addr_hash.val;

    var siz = len(data) & ~0xf; // align size to 16-bytes
    if (siz > 0) {>>MARKER:FUNCTION_ghash_BLOCK_PREFIX<<
        ghash(_addr_g.hashKey, _addr_hash, data[..(int)siz]);
        data = data[(int)siz..];
    }
    if (len(data) > 0) {
        array<byte> s = new array<byte>(16);
        copy(s[..], data);
        ghash(_addr_g.hashKey, _addr_hash, s[..]);
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
private static void cryptBlocksGCM(code fn, slice<byte> key, slice<byte> dst, slice<byte> src, slice<byte> buf, ptr<gcmCount> cnt);

// counterCrypt encrypts src using AES in counter mode and places the result
// into dst. cnt is the initial count value and will be updated with the next
// count value. The length of dst must be greater than or equal to the length
// of src.
private static void counterCrypt(this ptr<gcmAsm> _addr_g, slice<byte> dst, slice<byte> src, ptr<gcmCount> _addr_cnt) {
    ref gcmAsm g = ref _addr_g.val;
    ref gcmCount cnt = ref _addr_cnt.val;
 
    // Copying src into a buffer improves performance on some models when
    // src and dst point to the same underlying array. We also need a
    // buffer for counter values.
    array<byte> ctrbuf = new array<byte>(2048);    array<byte> srcbuf = new array<byte>(2048);

    while (len(src) >= 16) {>>MARKER:FUNCTION_cryptBlocksGCM_BLOCK_PREFIX<<
        var siz = len(src);
        if (len(src) > len(ctrbuf)) {
            siz = len(ctrbuf);
        }
        siz &= 0xf; // align siz to 16-bytes
        copy(srcbuf[..], src[..(int)siz]);
        cryptBlocksGCM(g.block.function, g.block.key, dst[..(int)siz], srcbuf[..(int)siz], ctrbuf[..], _addr_cnt);
        src = src[(int)siz..];
        dst = dst[(int)siz..];

    }
    if (len(src) > 0) {
        array<byte> x = new array<byte>(16);
        g.block.Encrypt(x[..], cnt[..]);
        foreach (var (i) in src) {
            dst[i] = src[i] ^ x[i];
        }        cnt.inc();
    }
}

// deriveCounter computes the initial GCM counter state from the given nonce.
// See NIST SP 800-38D, section 7.1.
private static gcmCount deriveCounter(this ptr<gcmAsm> _addr_g, slice<byte> nonce) {
    ref gcmAsm g = ref _addr_g.val;
 
    // GCM has two modes of operation with respect to the initial counter
    // state: a "fast path" for 96-bit (12-byte) nonces, and a "slow path"
    // for nonces of other lengths. For a 96-bit nonce, the nonce, along
    // with a four-byte big-endian counter starting at one, is used
    // directly as the starting counter. For other nonce sizes, the counter
    // is computed by passing it through the GHASH function.
    gcmCount counter = default;
    if (len(nonce) == gcmStandardNonceSize) {
        copy(counter[..], nonce);
        counter[gcmBlockSize - 1] = 1;
    }
    else
 {
        ref array<byte> hash = ref heap(new array<byte>(16), out ptr<array<byte>> _addr_hash);
        g.paddedGHASH(_addr_hash, nonce);
        var lens = gcmLengths(0, uint64(len(nonce)) * 8);
        g.paddedGHASH(_addr_hash, lens[..]);
        copy(counter[..], hash[..]);
    }
    return counter;

}

// auth calculates GHASH(ciphertext, additionalData), masks the result with
// tagMask and writes the result to out.
private static void auth(this ptr<gcmAsm> _addr_g, slice<byte> @out, slice<byte> ciphertext, slice<byte> additionalData, ptr<array<byte>> _addr_tagMask) {
    ref gcmAsm g = ref _addr_g.val;
    ref array<byte> tagMask = ref _addr_tagMask.val;

    ref array<byte> hash = ref heap(new array<byte>(16), out ptr<array<byte>> _addr_hash);
    g.paddedGHASH(_addr_hash, additionalData);
    g.paddedGHASH(_addr_hash, ciphertext);
    var lens = gcmLengths(uint64(len(additionalData)) * 8, uint64(len(ciphertext)) * 8);
    g.paddedGHASH(_addr_hash, lens[..]);

    copy(out, hash[..]);
    foreach (var (i) in out) {
        out[i] ^= tagMask[i];
    }
}

// Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
// details.
private static slice<byte> Seal(this ptr<gcmAsm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func((_, panic, _) => {
    ref gcmAsm g = ref _addr_g.val;

    if (len(nonce) != g.nonceSize) {
        panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if (uint64(len(plaintext)) > ((1 << 32) - 2) * BlockSize) {
        panic("crypto/cipher: message too large for GCM");
    }
    var (ret, out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
    if (subtleoverlap.InexactOverlap(out[..(int)len(plaintext)], plaintext)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    ref var counter = ref heap(g.deriveCounter(nonce), out ptr<var> _addr_counter);

    ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);
    g.block.Encrypt(tagMask[..], counter[..]);
    counter.inc();

    array<byte> tagOut = new array<byte>(gcmTagSize);
    g.counterCrypt(out, plaintext, _addr_counter);
    g.auth(tagOut[..], out[..(int)len(plaintext)], data, _addr_tagMask);
    copy(out[(int)len(plaintext)..], tagOut[..]);

    return ret;

});

// Open authenticates and decrypts ciphertext. See the cipher.AEAD interface
// for details.
private static (slice<byte>, error) Open(this ptr<gcmAsm> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref gcmAsm g = ref _addr_g.val;

    if (len(nonce) != g.nonceSize) {
        panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if (g.tagSize < gcmMinimumTagSize) {
        panic("crypto/cipher: incorrect GCM tag size");
    }
    if (len(ciphertext) < g.tagSize) {
        return (null, error.As(errOpen)!);
    }
    if (uint64(len(ciphertext)) > ((1 << 32) - 2) * uint64(BlockSize) + uint64(g.tagSize)) {
        return (null, error.As(errOpen)!);
    }
    var tag = ciphertext[(int)len(ciphertext) - g.tagSize..];
    ciphertext = ciphertext[..(int)len(ciphertext) - g.tagSize];

    ref var counter = ref heap(g.deriveCounter(nonce), out ptr<var> _addr_counter);

    ref array<byte> tagMask = ref heap(new array<byte>(gcmBlockSize), out ptr<array<byte>> _addr_tagMask);
    g.block.Encrypt(tagMask[..], counter[..]);
    counter.inc();

    array<byte> expectedTag = new array<byte>(gcmTagSize);
    g.auth(expectedTag[..], ciphertext, data, _addr_tagMask);

    var (ret, out) = sliceForAppend(dst, len(ciphertext));
    if (subtleoverlap.InexactOverlap(out, ciphertext)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    if (subtle.ConstantTimeCompare(expectedTag[..(int)g.tagSize], tag) != 1) { 
        // The AESNI code decrypts and authenticates concurrently, and
        // so overwrites dst in the event of a tag mismatch. That
        // behavior is mimicked here in order to be consistent across
        // platforms.
        foreach (var (i) in out) {
            out[i] = 0;
        }        return (null, error.As(errOpen)!);

    }
    g.counterCrypt(out, ciphertext, _addr_counter);
    return (ret, error.As(null!)!);

});

// gcmKMA implements the cipher.AEAD interface using the KMA instruction. It should
// only be used if hasKMA is true.
private partial struct gcmKMA {
    public ref gcmAsm gcmAsm => ref gcmAsm_val;
}

// flags for the KMA instruction
private static readonly nint kmaHS = 1 << 10; // hash subkey supplied
private static readonly nint kmaLAAD = 1 << 9; // last series of additional authenticated data
private static readonly nint kmaLPC = 1 << 8; // last series of plaintext or ciphertext blocks
private static readonly nint kmaDecrypt = 1 << 7; // decrypt

// kmaGCM executes the encryption or decryption operation given by fn. The tag
// will be calculated and written to tag. cnt should contain the current
// counter state and will be overwritten with the updated counter state.
// TODO(mundaym): could pass in hash subkey
//go:noescape
private static void kmaGCM(code fn, slice<byte> key, slice<byte> dst, slice<byte> src, slice<byte> aad, ptr<array<byte>> tag, ptr<gcmCount> cnt);

// Seal encrypts and authenticates plaintext. See the cipher.AEAD interface for
// details.
private static slice<byte> Seal(this ptr<gcmKMA> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) => func((_, panic, _) => {
    ref gcmKMA g = ref _addr_g.val;

    if (len(nonce) != g.nonceSize) {>>MARKER:FUNCTION_kmaGCM_BLOCK_PREFIX<<
        panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if (uint64(len(plaintext)) > ((1 << 32) - 2) * BlockSize) {
        panic("crypto/cipher: message too large for GCM");
    }
    var (ret, out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
    if (subtleoverlap.InexactOverlap(out[..(int)len(plaintext)], plaintext)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    ref var counter = ref heap(g.deriveCounter(nonce), out ptr<var> _addr_counter);
    var fc = g.block.function | kmaLAAD | kmaLPC;

    ref array<byte> tag = ref heap(new array<byte>(gcmTagSize), out ptr<array<byte>> _addr_tag);
    kmaGCM(fc, g.block.key, out[..(int)len(plaintext)], plaintext, data, _addr_tag, _addr_counter);
    copy(out[(int)len(plaintext)..], tag[..]);

    return ret;

});

// Open authenticates and decrypts ciphertext. See the cipher.AEAD interface
// for details.
private static (slice<byte>, error) Open(this ptr<gcmKMA> _addr_g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref gcmKMA g = ref _addr_g.val;

    if (len(nonce) != g.nonceSize) {
        panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if (len(ciphertext) < g.tagSize) {
        return (null, error.As(errOpen)!);
    }
    if (uint64(len(ciphertext)) > ((1 << 32) - 2) * uint64(BlockSize) + uint64(g.tagSize)) {
        return (null, error.As(errOpen)!);
    }
    var tag = ciphertext[(int)len(ciphertext) - g.tagSize..];
    ciphertext = ciphertext[..(int)len(ciphertext) - g.tagSize];
    var (ret, out) = sliceForAppend(dst, len(ciphertext));
    if (subtleoverlap.InexactOverlap(out, ciphertext)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    if (g.tagSize < gcmMinimumTagSize) {
        panic("crypto/cipher: incorrect GCM tag size");
    }
    ref var counter = ref heap(g.deriveCounter(nonce), out ptr<var> _addr_counter);
    var fc = g.block.function | kmaLAAD | kmaLPC | kmaDecrypt;

    ref array<byte> expectedTag = ref heap(new array<byte>(gcmTagSize), out ptr<array<byte>> _addr_expectedTag);
    kmaGCM(fc, g.block.key, out[..(int)len(ciphertext)], ciphertext, data, _addr_expectedTag, _addr_counter);

    if (subtle.ConstantTimeCompare(expectedTag[..(int)g.tagSize], tag) != 1) { 
        // The AESNI code decrypts and authenticates concurrently, and
        // so overwrites dst in the event of a tag mismatch. That
        // behavior is mimicked here in order to be consistent across
        // platforms.
        foreach (var (i) in out) {
            out[i] = 0;
        }        return (null, error.As(errOpen)!);

    }
    return (ret, error.As(null!)!);

});

} // end aes_package
