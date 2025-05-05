// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Cipher block chaining (CBC) mode.
// CBC provides confidentiality by xoring (chaining) each plaintext block
// with the previous ciphertext block before applying the block cipher.
// See NIST SP 800-38A, pp 10-11
namespace go.crypto;

using bytes = bytes_package;
using alias = crypto.@internal.alias_package;
using subtle = crypto.subtle_package;
using crypto.@internal;

partial class cipher_package {

[GoType] partial struct cbc {
    internal Block b;
    internal nint blockSize;
    internal slice<byte> iv;
    internal slice<byte> tmp;
}

internal static ж<cbc> newCBC(Block b, slice<byte> iv) {
    return Ꮡ(new cbc(
        b: b,
        blockSize: b.BlockSize(),
        iv: bytes.Clone(iv),
        tmp: new slice<byte>(b.BlockSize())
    ));
}

[GoType("struct{b crypto.cipher.Block; blockSize int; iv <>byte; tmp <>byte}")] partial struct cbcEncrypter;

// cbcEncAble is an interface implemented by ciphers that have a specific
// optimized implementation of CBC encryption, like crypto/aes.
// NewCBCEncrypter will check for this interface and return the specific
// BlockMode if found.
[GoType] partial interface cbcEncAble {
    BlockMode NewCBCEncrypter(slice<byte> iv);
}

// NewCBCEncrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size.
public static BlockMode NewCBCEncrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCEncrypter: IV length must equal block size");
    }
    {
        var (cbc, ok) = b._<cbcEncAble>(ᐧ); if (ok) {
            return cbc.NewCBCEncrypter(iv);
        }
    }
    return ~((ж<cbcEncrypter>)(newCBC(b, iv)?.val ?? default!));
}

// newCBCGenericEncrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size. This always returns the generic non-asm encrypter for use
// in fuzz testing.
internal static BlockMode newCBCGenericEncrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCEncrypter: IV length must equal block size");
    }
    return ~((ж<cbcEncrypter>)(newCBC(b, iv)?.val ?? default!));
}

[GoRecv] internal static nint BlockSize(this ref cbcEncrypter x) {
    return x.blockSize;
}

[GoRecv] internal static void CryptBlocks(this ref cbcEncrypter x, slice<byte> dst, slice<byte> src) {
    if (len(src) % x.blockSize != 0) {
        throw panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst[..(int)(len(src))], src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    var iv = x.iv;
    while (len(src) > 0) {
        // Write the xor to dst, then encrypt in place.
        subtle.XORBytes(dst[..(int)(x.blockSize)], src[..(int)(x.blockSize)], iv);
        x.b.Encrypt(dst[..(int)(x.blockSize)], dst[..(int)(x.blockSize)]);
        // Move to the next block with this block as the next iv.
        iv = dst[..(int)(x.blockSize)];
        src = src[(int)(x.blockSize)..];
        dst = dst[(int)(x.blockSize)..];
    }
    // Save the iv for the next CryptBlocks call.
    copy(x.iv, iv);
}

[GoRecv] internal static void SetIV(this ref cbcEncrypter x, slice<byte> iv) {
    if (len(iv) != len(x.iv)) {
        throw panic("cipher: incorrect length IV");
    }
    copy(x.iv, iv);
}

[GoType("struct{b crypto.cipher.Block; blockSize int; iv <>byte; tmp <>byte}")] partial struct cbcDecrypter;

// cbcDecAble is an interface implemented by ciphers that have a specific
// optimized implementation of CBC decryption, like crypto/aes.
// NewCBCDecrypter will check for this interface and return the specific
// BlockMode if found.
[GoType] partial interface cbcDecAble {
    BlockMode NewCBCDecrypter(slice<byte> iv);
}

// NewCBCDecrypter returns a BlockMode which decrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size and must match the iv used to encrypt the data.
public static BlockMode NewCBCDecrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCDecrypter: IV length must equal block size");
    }
    {
        var (cbc, ok) = b._<cbcDecAble>(ᐧ); if (ok) {
            return cbc.NewCBCDecrypter(iv);
        }
    }
    return ~((ж<cbcDecrypter>)(newCBC(b, iv)?.val ?? default!));
}

// newCBCGenericDecrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size. This always returns the generic non-asm decrypter for use in
// fuzz testing.
internal static BlockMode newCBCGenericDecrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCDecrypter: IV length must equal block size");
    }
    return ~((ж<cbcDecrypter>)(newCBC(b, iv)?.val ?? default!));
}

[GoRecv] internal static nint BlockSize(this ref cbcDecrypter x) {
    return x.blockSize;
}

[GoRecv] internal static void CryptBlocks(this ref cbcDecrypter x, slice<byte> dst, slice<byte> src) {
    if (len(src) % x.blockSize != 0) {
        throw panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst[..(int)(len(src))], src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    if (len(src) == 0) {
        return;
    }
    // For each block, we need to xor the decrypted data with the previous block's ciphertext (the iv).
    // To avoid making a copy each time, we loop over the blocks BACKWARDS.
    nint end = len(src);
    nint start = end - x.blockSize;
    nint prev = start - x.blockSize;
    // Copy the last block of ciphertext in preparation as the new iv.
    copy(x.tmp, src[(int)(start)..(int)(end)]);
    // Loop over all but the first block.
    while (start > 0) {
        x.b.Decrypt(dst[(int)(start)..(int)(end)], src[(int)(start)..(int)(end)]);
        subtle.XORBytes(dst[(int)(start)..(int)(end)], dst[(int)(start)..(int)(end)], src[(int)(prev)..(int)(start)]);
        end = start;
        start = prev;
        prev -= x.blockSize;
    }
    // The first block is special because it uses the saved iv.
    x.b.Decrypt(dst[(int)(start)..(int)(end)], src[(int)(start)..(int)(end)]);
    subtle.XORBytes(dst[(int)(start)..(int)(end)], dst[(int)(start)..(int)(end)], x.iv);
    // Set the new iv to the first block we copied earlier.
    (x.iv, x.tmp) = (x.tmp, x.iv);
}

[GoRecv] internal static void SetIV(this ref cbcDecrypter x, slice<byte> iv) {
    if (len(iv) != len(x.iv)) {
        throw panic("cipher: incorrect length IV");
    }
    copy(x.iv, iv);
}

} // end cipher_package
