// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using cipher = crypto.cipher_package;

partial class aes_package {

// gcmAble is implemented by cipher.Blocks that can provide an optimized
// implementation of GCM through the AEAD interface.
// See crypto/cipher/gcm.go.
[GoType] partial interface gcmAble {
    (cipher.AEAD, error) NewGCM(nint nonceSize, nint tagSize);
}

// cbcEncAble is implemented by cipher.Blocks that can provide an optimized
// implementation of CBC encryption through the cipher.BlockMode interface.
// See crypto/cipher/cbc.go.
[GoType] partial interface cbcEncAble {
    cipher.BlockMode NewCBCEncrypter(slice<byte> iv);
}

// cbcDecAble is implemented by cipher.Blocks that can provide an optimized
// implementation of CBC decryption through the cipher.BlockMode interface.
// See crypto/cipher/cbc.go.
[GoType] partial interface cbcDecAble {
    cipher.BlockMode NewCBCDecrypter(slice<byte> iv);
}

// ctrAble is implemented by cipher.Blocks that can provide an optimized
// implementation of CTR through the cipher.Stream interface.
// See crypto/cipher/ctr.go.
[GoType] partial interface ctrAble {
    cipher.Stream NewCTR(slice<byte> iv);
}

} // end aes_package
