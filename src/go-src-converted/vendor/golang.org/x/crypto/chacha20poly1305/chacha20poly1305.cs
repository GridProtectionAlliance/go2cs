// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha20poly1305 implements the ChaCha20-Poly1305 AEAD and its
// extended nonce variant XChaCha20-Poly1305, as specified in RFC 8439 and
// draft-irtf-cfrg-xchacha-01.
// package chacha20poly1305 -- go2cs converted at 2022 March 06 23:36:32 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305.go
// import "golang.org/x/crypto/chacha20poly1305"

using cipher = go.crypto.cipher_package;
using errors = go.errors_package;

namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20poly1305_package {

 
// KeySize is the size of the key used by this AEAD, in bytes.
public static readonly nint KeySize = 32; 

// NonceSize is the size of the nonce used with the standard variant of this
// AEAD, in bytes.
//
// Note that this is too short to be safely generated at random if the same
// key is reused more than 2³² times.
public static readonly nint NonceSize = 12; 

// NonceSizeX is the size of the nonce used with the XChaCha20-Poly1305
// variant of this AEAD, in bytes.
public static readonly nint NonceSizeX = 24;


private partial struct chacha20poly1305 {
    public array<byte> key;
}

// New returns a ChaCha20-Poly1305 AEAD that uses the given 256-bit key.
public static (cipher.AEAD, error) New(slice<byte> key) {
    cipher.AEAD _p0 = default;
    error _p0 = default!;

    if (len(key) != KeySize) {
        return (null, error.As(errors.New("chacha20poly1305: bad key length"))!);
    }
    ptr<chacha20poly1305> ret = @new<chacha20poly1305>();
    copy(ret.key[..], key);
    return (ret, error.As(null!)!);

}

private static nint NonceSize(this ptr<chacha20poly1305> _addr_c) {
    ref chacha20poly1305 c = ref _addr_c.val;

    return NonceSize;
}

private static nint Overhead(this ptr<chacha20poly1305> _addr_c) {
    ref chacha20poly1305 c = ref _addr_c.val;

    return 16;
}

private static slice<byte> Seal(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func((_, panic, _) => {
    ref chacha20poly1305 c = ref _addr_c.val;

    if (len(nonce) != NonceSize) {
        panic("chacha20poly1305: bad nonce length passed to Seal");
    }
    if (uint64(len(plaintext)) > (1 << 38) - 64) {
        panic("chacha20poly1305: plaintext too large");
    }
    return c.seal(dst, nonce, plaintext, additionalData);

});

private static var errOpen = errors.New("chacha20poly1305: message authentication failed");

private static (slice<byte>, error) Open(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref chacha20poly1305 c = ref _addr_c.val;

    if (len(nonce) != NonceSize) {
        panic("chacha20poly1305: bad nonce length passed to Open");
    }
    if (len(ciphertext) < 16) {
        return (null, error.As(errOpen)!);
    }
    if (uint64(len(ciphertext)) > (1 << 38) - 48) {
        panic("chacha20poly1305: ciphertext too large");
    }
    return c.open(dst, nonce, ciphertext, additionalData);

});

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

} // end chacha20poly1305_package
