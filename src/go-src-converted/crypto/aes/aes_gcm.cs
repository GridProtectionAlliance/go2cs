// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (amd64 || arm64) && !purego
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using alias = go.crypto.@internal.alias_package;
using subtle = go.crypto.subtle_package;
using errors = errors_package;
using go.crypto;
using go.crypto.@internal;

partial class aes_package {

// The following functions are defined in gcm_*.s.

//go:noescape
internal static partial void gcmAesInit(ж<array<byte>> productTable, slice<uint32> ks);

//go:noescape
internal static partial void gcmAesData(ж<array<byte>> productTable, slice<byte> data, ж<array<byte>> T);

//go:noescape
internal static partial void gcmAesEnc(ж<array<byte>> productTable, slice<byte> dst, slice<byte> src, ж<array<byte>> ctr, ж<array<byte>> T, slice<uint32> ks);

//go:noescape
internal static partial void gcmAesDec(ж<array<byte>> productTable, slice<byte> dst, slice<byte> src, ж<array<byte>> ctr, ж<array<byte>> T, slice<uint32> ks);

//go:noescape
internal static partial void gcmAesFinish(ж<array<byte>> productTable, ж<array<byte>> tagMask, ж<array<byte>> T, uint64 pLen, uint64 dLen);

internal static readonly UntypedInt gcmBlockSize = 16;
internal static readonly UntypedInt gcmTagSize = 16;
internal static readonly UntypedInt gcmMinimumTagSize = 12; // NIST SP 800-38D recommends tags with 12 or more bytes.
internal static readonly UntypedInt gcmStandardNonceSize = 12;

internal static error errOpen = errors.New("cipher: message authentication failed"u8);

// Assert that aesCipherGCM implements the gcmAble interface.
internal static gcmAble _ᴛ1ʗ = new aesCipherGCMжgcmAble((ж<aesCipherGCM>)(default!));

// NewGCM returns the AES cipher wrapped in Galois Counter Mode. This is only
// called by [crypto/cipher.NewGCM] via the gcmAble interface.
[GoRecv] internal static (cipher.AEAD, error) NewGCM(this ref aesCipherGCM c, nint nonceSize, nint tagSize) {
    var g = Ꮡ(new gcmAsm(ks: c.enc[..(int)(c.l)], nonceSize: nonceSize, tagSize: tagSize));
    gcmAesInit(g.of(gcmAsm.ᏑproductTable), (~g).ks);
    return (new gcmAsmжAEAD(g), default!);
}

[GoType] partial struct gcmAsm {
    // ks is the key schedule, the length of which depends on the size of
    // the AES key.
    internal slice<uint32> ks;
    // productTable contains pre-computed multiples of the binary-field
    // element used in GHASH.
    internal array<byte> productTable = new(256);
    // nonceSize contains the expected size of the nonce, in bytes.
    internal nint nonceSize;
    // tagSize contains the size of the tag, in bytes.
    internal nint tagSize;
}

[GoRecv] internal static nint NonceSize(this ref gcmAsm g) {
    return g.nonceSize;
}

[GoRecv] internal static nint Overhead(this ref gcmAsm g) {
    return g.tagSize;
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

// Seal encrypts and authenticates plaintext. See the [cipher.AEAD] interface for
// details.
internal static slice<byte> Seal(this ж<gcmAsm> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.Value;

    if (len(nonce) != g.nonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if ((uint64)len(plaintext) > (4294967294L) * ΔBlockSize) {
        throw panic("crypto/cipher: message too large for GCM");
    }
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    if (len(nonce) == gcmStandardNonceSize){
        // Init counter to nonce||1
        copy(counter[..], nonce);
        counter[gcmBlockSize - 1] = 1;
    } else {
        // Otherwise counter = GHASH(nonce)
        gcmAesData(Ꮡg.of(gcmAsm.ᏑproductTable), nonce, Ꮡcounter);
        gcmAesFinish(Ꮡg.of(gcmAsm.ᏑproductTable), ᏑtagMask, Ꮡcounter, (uint64)len(nonce), (uint64)0);
    }
    encryptBlockAsm(len(g.ks) / 4 - 1, Ꮡ(g.ks[0]), ᏑtagMask.at<byte>(0), Ꮡcounter.at<byte>(0));
    ref var tagOut = ref heap(new array<byte>(16), out var ᏑtagOut);
    gcmAesData(Ꮡg.of(gcmAsm.ᏑproductTable), data, ᏑtagOut);
    var (ret, @out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
    if (alias.InexactOverlap(@out[..(int)(len(plaintext))], plaintext)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    if (len(plaintext) > 0) {
        gcmAesEnc(Ꮡg.of(gcmAsm.ᏑproductTable), @out, plaintext, Ꮡcounter, ᏑtagOut, g.ks);
    }
    gcmAesFinish(Ꮡg.of(gcmAsm.ᏑproductTable), ᏑtagMask, ᏑtagOut, (uint64)len(plaintext), (uint64)len(data));
    copy(@out[(int)(len(plaintext))..], tagOut[..]);
    return ret;
}

// Open authenticates and decrypts ciphertext. See the [cipher.AEAD] interface
// for details.
internal static (slice<byte>, error) Open(this ж<gcmAsm> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    ref var g = ref Ꮡg.Value;

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
    if ((uint64)len(ciphertext) > (uint64)(((4294967296L) - 2) * (uint64)ΔBlockSize) + (uint64)g.tagSize) {
        return (default!, errOpen);
    }
    var tag = ciphertext[(int)(len(ciphertext) - g.tagSize)..];
    ciphertext = ciphertext[..(int)(len(ciphertext) - g.tagSize)];
    // See GCM spec, section 7.1.
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    if (len(nonce) == gcmStandardNonceSize){
        // Init counter to nonce||1
        copy(counter[..], nonce);
        counter[gcmBlockSize - 1] = 1;
    } else {
        // Otherwise counter = GHASH(nonce)
        gcmAesData(Ꮡg.of(gcmAsm.ᏑproductTable), nonce, Ꮡcounter);
        gcmAesFinish(Ꮡg.of(gcmAsm.ᏑproductTable), ᏑtagMask, Ꮡcounter, (uint64)len(nonce), (uint64)0);
    }
    encryptBlockAsm(len(g.ks) / 4 - 1, Ꮡ(g.ks[0]), ᏑtagMask.at<byte>(0), Ꮡcounter.at<byte>(0));
    ref var expectedTag = ref heap(new array<byte>(16), out var ᏑexpectedTag);
    gcmAesData(Ꮡg.of(gcmAsm.ᏑproductTable), data, ᏑexpectedTag);
    var (ret, @out) = sliceForAppend(dst, len(ciphertext));
    if (alias.InexactOverlap(@out, ciphertext)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    if (len(ciphertext) > 0) {
        gcmAesDec(Ꮡg.of(gcmAsm.ᏑproductTable), @out, ciphertext, Ꮡcounter, ᏑexpectedTag, g.ks);
    }
    gcmAesFinish(Ꮡg.of(gcmAsm.ᏑproductTable), ᏑtagMask, ᏑexpectedTag, (uint64)len(ciphertext), (uint64)len(data));
    if (subtle.ConstantTimeCompare(expectedTag[..(int)(g.tagSize)], tag) != 1) {
        clear(@out);
        return (default!, errOpen);
    }
    return (ret, default!);
}

} // end aes_package
