// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cipher implements standard block cipher modes that can be wrapped
// around low-level block cipher implementations.
// See https://csrc.nist.gov/groups/ST/toolkit/BCM/current_modes.html
// and NIST Special Publication 800-38A.
// package cipher -- go2cs converted at 2022 March 06 22:18:08 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Program Files\Go\src\crypto\cipher\cipher.go


namespace go.crypto;

public static partial class cipher_package {

    // A Block represents an implementation of block cipher
    // using a given key. It provides the capability to encrypt
    // or decrypt individual blocks. The mode implementations
    // extend that capability to streams of blocks.
public partial interface Block {
    nint BlockSize(); // Encrypt encrypts the first block in src into dst.
// Dst and src must overlap entirely or not at all.
    nint Encrypt(slice<byte> dst, slice<byte> src); // Decrypt decrypts the first block in src into dst.
// Dst and src must overlap entirely or not at all.
    nint Decrypt(slice<byte> dst, slice<byte> src);
}

// A Stream represents a stream cipher.
public partial interface Stream {
    void XORKeyStream(slice<byte> dst, slice<byte> src);
}

// A BlockMode represents a block cipher running in a block-based mode (CBC,
// ECB etc).
public partial interface BlockMode {
    nint BlockSize(); // CryptBlocks encrypts or decrypts a number of blocks. The length of
// src must be a multiple of the block size. Dst and src must overlap
// entirely or not at all.
//
// If len(dst) < len(src), CryptBlocks should panic. It is acceptable
// to pass a dst bigger than src, and in that case, CryptBlocks will
// only update dst[:len(src)] and will not touch the rest of dst.
//
// Multiple calls to CryptBlocks behave as if the concatenation of
// the src buffers was passed in a single run. That is, BlockMode
// maintains state and does not reset at each CryptBlocks call.
    nint CryptBlocks(slice<byte> dst, slice<byte> src);
}

// Utility routines

private static slice<byte> dup(slice<byte> p) {
    var q = make_slice<byte>(len(p));
    copy(q, p);
    return q;
}

} // end cipher_package
