// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2022 March 13 05:32:26 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\cipher.go
namespace go.crypto;

using cipher = crypto.cipher_package;
using subtle = crypto.@internal.subtle_package;
using strconv = strconv_package;


// The AES block size in bytes.

public static partial class aes_package {

public static readonly nint BlockSize = 16;

// A cipher is an instance of AES encryption using a particular key.


// A cipher is an instance of AES encryption using a particular key.
private partial struct aesCipher {
    public slice<uint> enc;
    public slice<uint> dec;
}

public partial struct KeySizeError { // : nint
}

public static @string Error(this KeySizeError k) {
    return "crypto/aes: invalid key size " + strconv.Itoa(int(k));
}

// NewCipher creates and returns a new cipher.Block.
// The key argument should be the AES key,
// either 16, 24, or 32 bytes to select
// AES-128, AES-192, or AES-256.
public static (cipher.Block, error) NewCipher(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;

    var k = len(key);
    switch (k) {
        case 16: 

        case 24: 

        case 32: 
            break;
            break;
        default: 
            return (null, error.As(KeySizeError(k))!);
            break;
    }
    return newCipher(key);
}

// newCipherGeneric creates and returns a new cipher.Block
// implemented in pure Go.
private static (cipher.Block, error) newCipherGeneric(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;

    var n = len(key) + 28;
    ref aesCipher c = ref heap(new aesCipher(make([]uint32,n),make([]uint32,n)), out ptr<aesCipher> _addr_c);
    expandKeyGo(key, c.enc, c.dec);
    return (_addr_c, error.As(null!)!);
}

private static nint BlockSize(this ptr<aesCipher> _addr_c) {
    ref aesCipher c = ref _addr_c.val;

    return BlockSize;
}

private static void Encrypt(this ptr<aesCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref aesCipher c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/aes: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/aes: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/aes: invalid buffer overlap");
    }
    encryptBlockGo(c.enc, dst, src);
});

private static void Decrypt(this ptr<aesCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref aesCipher c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/aes: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/aes: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/aes: invalid buffer overlap");
    }
    decryptBlockGo(c.dec, dst, src);
});

} // end aes_package
