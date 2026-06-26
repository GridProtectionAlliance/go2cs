// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Counter (CTR) mode.
// CTR converts a block cipher into a stream cipher by
// repeatedly encrypting an incrementing counter and
// xoring the resulting stream of data with the input.
// See NIST SP 800-38A, pp 13-15
namespace go.crypto;

using bytes = bytes_package;
using alias = crypto.@internal.alias_package;
using subtle = crypto.subtle_package;
using crypto.@internal;

partial class cipher_package {

[GoType] partial struct ctr {
    internal Block b;
    internal slice<byte> ctr;
    internal slice<byte> @out;
    internal nint outUsed;
}

internal static readonly UntypedInt streamBufferSize = 512;

// ctrAble is an interface implemented by ciphers that have a specific optimized
// implementation of CTR, like crypto/aes. NewCTR will check for this interface
// and return the specific Stream if found.
[GoType] partial interface ctrAble {
    Stream NewCTR(slice<byte> iv);
}

// NewCTR returns a [Stream] which encrypts/decrypts using the given [Block] in
// counter mode. The length of iv must be the same as the [Block]'s block size.
public static Stream NewCTR(Block block, slice<byte> iv) {
    {
        var (ctr, ok) = block._<ctrAble>(ᐧ); if (ok) {
            return ctr.NewCTR(iv);
        }
    }
    if (len(iv) != block.BlockSize()) {
        throw panic("cipher.NewCTR: IV length must equal block size");
    }
    nint bufSize = streamBufferSize;
    if (bufSize < block.BlockSize()) {
        bufSize = block.BlockSize();
    }
    return new ctr(
        b: block,
        ctr: bytes.Clone(iv),
        @out: new slice<byte>(0, bufSize),
        outUsed: 0
    );
}

[GoRecv] internal static void refill(this ref ctr x) {
    nint remain = len(x.@out) - x.outUsed;
    copy(x.@out, x.@out[(int)(x.outUsed)..]);
    x.@out = x.@out[..(int)(cap(x.@out))];
    nint bs = x.b.BlockSize();
    while (remain <= len(x.@out) - bs) {
        x.b.Encrypt(x.@out[(int)(remain)..], x.ctr);
        remain += bs;
        // Increment counter
        for (nint i = len(x.ctr) - 1; i >= 0; i--) {
            x.ctr[i]++;
            if (x.ctr[i] != 0) {
                break;
            }
        }
    }
    x.@out = x.@out[..(int)(remain)];
    x.outUsed = 0;
}

[GoRecv] internal static void XORKeyStream(this ref ctr x, slice<byte> dst, slice<byte> src) {
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
