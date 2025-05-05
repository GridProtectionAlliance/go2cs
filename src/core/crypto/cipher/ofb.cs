// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// OFB (Output Feedback) Mode.
namespace go.crypto;

using alias = crypto.@internal.alias_package;
using subtle = crypto.subtle_package;
using crypto.@internal;

partial class cipher_package {

[GoType] partial struct ofb {
    internal Block b;
    internal slice<byte> cipher;
    internal slice<byte> @out;
    internal nint outUsed;
}

// NewOFB returns a [Stream] that encrypts or decrypts using the block cipher b
// in output feedback mode. The initialization vector iv's length must be equal
// to b's block size.
public static Stream NewOFB(Block b, slice<byte> iv) {
    nint blockSize = b.BlockSize();
    if (len(iv) != blockSize) {
        throw panic("cipher.NewOFB: IV length must equal block size");
    }
    nint bufSize = streamBufferSize;
    if (bufSize < blockSize) {
        bufSize = blockSize;
    }
    var x = Ꮡ(new ofb(
        b: b,
        cipher: new slice<byte>(blockSize),
        @out: new slice<byte>(0, bufSize),
        outUsed: 0
    ));
    copy((~x).cipher, iv);
    return ~x;
}

[GoRecv] internal static void refill(this ref ofb x) {
    nint bs = x.b.BlockSize();
    nint remain = len(x.@out) - x.outUsed;
    if (remain > x.outUsed) {
        return;
    }
    copy(x.@out, x.@out[(int)(x.outUsed)..]);
    x.@out = x.@out[..(int)(cap(x.@out))];
    while (remain < len(x.@out) - bs) {
        x.b.Encrypt(x.cipher, x.cipher);
        copy(x.@out[(int)(remain)..], x.cipher);
        remain += bs;
    }
    x.@out = x.@out[..(int)(remain)];
    x.outUsed = 0;
}

[GoRecv] internal static void XORKeyStream(this ref ofb x, slice<byte> dst, slice<byte> src) {
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst[..(int)(len(src))], src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    while (len(src) > 0) {
        if (x.outUsed >= len(x.@out) - x.b.BlockSize()) {
            x.refill();
        }
        nint n = subtle.XORBytes(dst, src, x.@out[(int)(x.outUsed)..]);
        dst = dst[(int)(n)..];
        src = src[(int)(n)..];
        x.outUsed += n;
    }
}

} // end cipher_package
