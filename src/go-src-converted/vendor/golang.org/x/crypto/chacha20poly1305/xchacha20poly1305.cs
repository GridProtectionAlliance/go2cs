// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package chacha20poly1305 -- go2cs converted at 2022 March 06 23:36:33 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\xchacha20poly1305.go
using cipher = go.crypto.cipher_package;
using errors = go.errors_package;

using chacha20 = go.golang.org.x.crypto.chacha20_package;

namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20poly1305_package {

private partial struct xchacha20poly1305 {
    public array<byte> key;
}

// NewX returns a XChaCha20-Poly1305 AEAD that uses the given 256-bit key.
//
// XChaCha20-Poly1305 is a ChaCha20-Poly1305 variant that takes a longer nonce,
// suitable to be generated randomly without risk of collisions. It should be
// preferred when nonce uniqueness cannot be trivially ensured, or whenever
// nonces are randomly generated.
public static (cipher.AEAD, error) NewX(slice<byte> key) {
    cipher.AEAD _p0 = default;
    error _p0 = default!;

    if (len(key) != KeySize) {
        return (null, error.As(errors.New("chacha20poly1305: bad key length"))!);
    }
    ptr<xchacha20poly1305> ret = @new<xchacha20poly1305>();
    copy(ret.key[..], key);
    return (ret, error.As(null!)!);

}

private static nint NonceSize(this ptr<xchacha20poly1305> _addr__p0) {
    ref xchacha20poly1305 _p0 = ref _addr__p0.val;

    return NonceSizeX;
}

private static nint Overhead(this ptr<xchacha20poly1305> _addr__p0) {
    ref xchacha20poly1305 _p0 = ref _addr__p0.val;

    return 16;
}

private static slice<byte> Seal(this ptr<xchacha20poly1305> _addr_x, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func((_, panic, _) => {
    ref xchacha20poly1305 x = ref _addr_x.val;

    if (len(nonce) != NonceSizeX) {
        panic("chacha20poly1305: bad nonce length passed to Seal");
    }
    if (uint64(len(plaintext)) > (1 << 38) - 64) {
        panic("chacha20poly1305: plaintext too large");
    }
    ptr<object> c = @new<chacha20poly1305>();
    var (hKey, _) = chacha20.HChaCha20(x.key[..], nonce[(int)0..(int)16]);
    copy(c.key[..], hKey); 

    // The first 4 bytes of the final nonce are unused counter space.
    var cNonce = make_slice<byte>(NonceSize);
    copy(cNonce[(int)4..(int)12], nonce[(int)16..(int)24]);

    return c.seal(dst, cNonce[..], plaintext, additionalData);

});

private static (slice<byte>, error) Open(this ptr<xchacha20poly1305> _addr_x, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref xchacha20poly1305 x = ref _addr_x.val;

    if (len(nonce) != NonceSizeX) {
        panic("chacha20poly1305: bad nonce length passed to Open");
    }
    if (len(ciphertext) < 16) {
        return (null, error.As(errOpen)!);
    }
    if (uint64(len(ciphertext)) > (1 << 38) - 48) {
        panic("chacha20poly1305: ciphertext too large");
    }
    ptr<object> c = @new<chacha20poly1305>();
    var (hKey, _) = chacha20.HChaCha20(x.key[..], nonce[(int)0..(int)16]);
    copy(c.key[..], hKey); 

    // The first 4 bytes of the final nonce are unused counter space.
    var cNonce = make_slice<byte>(NonceSize);
    copy(cNonce[(int)4..(int)12], nonce[(int)16..(int)24]);

    return c.open(dst, cNonce[..], ciphertext, additionalData);

});

} // end chacha20poly1305_package
