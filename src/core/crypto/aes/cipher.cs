// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using cipher = crypto.cipher_package;
using alias = crypto.@internal.alias_package;
using boring = crypto.@internal.boring_package;
using strconv = strconv_package;
using crypto.@internal;

partial class aes_package {

// The AES block size in bytes.
public static readonly UntypedInt ΔBlockSize = 16;

// A cipher is an instance of AES encryption using a particular key.
[GoType] partial struct aesCipher {
    internal uint8 l; // only this length of the enc and dec array is actually used
    internal array<uint32> enc = new(28 + 32);
    internal array<uint32> dec = new(28 + 32);
}

[GoType("num:nint")] partial struct KeySizeError;

public static @string Error(this KeySizeError k) {
    return "crypto/aes: invalid key size "u8 + strconv.Itoa(((nint)k));
}

// NewCipher creates and returns a new [cipher.Block].
// The key argument should be the AES key,
// either 16, 24, or 32 bytes to select
// AES-128, AES-192, or AES-256.
public static (cipher.Block, error) NewCipher(slice<byte> key) {
    nint k = len(key);
    switch (k) {
    default: {
        return (default!, ((KeySizeError)k));
    }
    case 16 or 24 or 32: {
        break;
        break;
    }}

    if (boring.Enabled) {
        return boring.NewAESCipher(key);
    }
    return newCipher(key);
}

// newCipherGeneric creates and returns a new cipher.Block
// implemented in pure Go.
internal static (cipher.Block, error) newCipherGeneric(slice<byte> key) {
    ref var c = ref heap<aesCipher>(out var Ꮡc);
    c = new aesCipher(l: ((uint8)(len(key) + 28)));
    expandKeyGo(key, c.enc[..(int)(c.l)], c.dec[..(int)(c.l)]);
    return (~Ꮡc, default!);
}

[GoRecv] internal static nint BlockSize(this ref aesCipher c) {
    return ΔBlockSize;
}

[GoRecv] internal static void Encrypt(this ref aesCipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/aes: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/aes: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    encryptBlockGo(c.enc[..(int)(c.l)], dst, src);
}

[GoRecv] internal static void Decrypt(this ref aesCipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/aes: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/aes: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    decryptBlockGo(c.dec[..(int)(c.l)], dst, src);
}

} // end aes_package
