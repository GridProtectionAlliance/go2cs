// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cipher implements standard block cipher modes that can be wrapped
// around low-level block cipher implementations.
// See https://csrc.nist.gov/groups/ST/toolkit/BCM/current_modes.html
// and NIST Special Publication 800-38A.
namespace go.crypto;

partial class cipher_package {

// A Block represents an implementation of block cipher
// using a given key. It provides the capability to encrypt
// or decrypt individual blocks. The mode implementations
// extend that capability to streams of blocks.
[GoType] partial interface Block {
    // BlockSize returns the cipher's block size.
    nint BlockSize();
    // Encrypt encrypts the first block in src into dst.
    // Dst and src must overlap entirely or not at all.
    void Encrypt(slice<byte> dst, slice<byte> src);
    // Decrypt decrypts the first block in src into dst.
    // Dst and src must overlap entirely or not at all.
    void Decrypt(slice<byte> dst, slice<byte> src);
}

// A Stream represents a stream cipher.
[GoType] partial interface Stream {
    // XORKeyStream XORs each byte in the given slice with a byte from the
    // cipher's key stream. Dst and src must overlap entirely or not at all.
    //
    // If len(dst) < len(src), XORKeyStream should panic. It is acceptable
    // to pass a dst bigger than src, and in that case, XORKeyStream will
    // only update dst[:len(src)] and will not touch the rest of dst.
    //
    // Multiple calls to XORKeyStream behave as if the concatenation of
    // the src buffers was passed in a single run. That is, Stream
    // maintains state and does not reset at each XORKeyStream call.
    void XORKeyStream(slice<byte> dst, slice<byte> src);
}

// A BlockMode represents a block cipher running in a block-based mode (CBC,
// ECB etc).
[GoType] partial interface BlockMode {
    // BlockSize returns the mode's block size.
    nint BlockSize();
    // CryptBlocks encrypts or decrypts a number of blocks. The length of
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
    void CryptBlocks(slice<byte> dst, slice<byte> src);
}

} // end cipher_package
