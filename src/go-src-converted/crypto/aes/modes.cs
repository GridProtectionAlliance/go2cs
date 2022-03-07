// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2022 March 06 22:18:17 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\modes.go
using cipher = go.crypto.cipher_package;

namespace go.crypto;

public static partial class aes_package {

    // gcmAble is implemented by cipher.Blocks that can provide an optimized
    // implementation of GCM through the AEAD interface.
    // See crypto/cipher/gcm.go.
private partial interface gcmAble {
    (cipher.AEAD, error) NewGCM(nint nonceSize, nint tagSize);
}

// cbcEncAble is implemented by cipher.Blocks that can provide an optimized
// implementation of CBC encryption through the cipher.BlockMode interface.
// See crypto/cipher/cbc.go.
private partial interface cbcEncAble {
    cipher.BlockMode NewCBCEncrypter(slice<byte> iv);
}

// cbcDecAble is implemented by cipher.Blocks that can provide an optimized
// implementation of CBC decryption through the cipher.BlockMode interface.
// See crypto/cipher/cbc.go.
private partial interface cbcDecAble {
    cipher.BlockMode NewCBCDecrypter(slice<byte> iv);
}

// ctrAble is implemented by cipher.Blocks that can provide an optimized
// implementation of CTR through the cipher.Stream interface.
// See crypto/cipher/ctr.go.
private partial interface ctrAble {
    cipher.Stream NewCTR(slice<byte> iv);
}

} // end aes_package
