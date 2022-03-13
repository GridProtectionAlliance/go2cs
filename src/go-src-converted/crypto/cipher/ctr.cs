// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Counter (CTR) mode.

// CTR converts a block cipher into a stream cipher by
// repeatedly encrypting an incrementing counter and
// xoring the resulting stream of data with the input.

// See NIST SP 800-38A, pp 13-15

// package cipher -- go2cs converted at 2022 March 13 05:32:21 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Program Files\Go\src\crypto\cipher\ctr.go
namespace go.crypto;

using subtle = crypto.@internal.subtle_package;

public static partial class cipher_package {

private partial struct ctr {
    public Block b;
    public slice<byte> ctr;
    public slice<byte> @out;
    public nint outUsed;
}

private static readonly nint streamBufferSize = 512;

// ctrAble is an interface implemented by ciphers that have a specific optimized
// implementation of CTR, like crypto/aes. NewCTR will check for this interface
// and return the specific Stream if found.


// ctrAble is an interface implemented by ciphers that have a specific optimized
// implementation of CTR, like crypto/aes. NewCTR will check for this interface
// and return the specific Stream if found.
private partial interface ctrAble {
    Stream NewCTR(slice<byte> iv);
}

// NewCTR returns a Stream which encrypts/decrypts using the given Block in
// counter mode. The length of iv must be the same as the Block's block size.
public static Stream NewCTR(Block block, slice<byte> iv) => func((_, panic, _) => {
    {
        ctrAble (ctr, ok) = ctrAble.As(block._<ctrAble>())!;

        if (ok) {
            return ctr.NewCTR(iv);
        }
    }
    if (len(iv) != block.BlockSize()) {
        panic("cipher.NewCTR: IV length must equal block size");
    }
    var bufSize = streamBufferSize;
    if (bufSize < block.BlockSize()) {
        bufSize = block.BlockSize();
    }
    return addr(new ctr(b:block,ctr:dup(iv),out:make([]byte,0,bufSize),outUsed:0,));
});

private static void refill(this ptr<ctr> _addr_x) {
    ref ctr x = ref _addr_x.val;

    var remain = len(x.@out) - x.outUsed;
    copy(x.@out, x.@out[(int)x.outUsed..]);
    x.@out = x.@out[..(int)cap(x.@out)];
    var bs = x.b.BlockSize();
    while (remain <= len(x.@out) - bs) {
        x.b.Encrypt(x.@out[(int)remain..], x.ctr);
        remain += bs; 

        // Increment counter
        for (var i = len(x.ctr) - 1; i >= 0; i--) {
            x.ctr[i]++;
            if (x.ctr[i] != 0) {
                break;
            }
        }
    }
    x.@out = x.@out[..(int)remain];
    x.outUsed = 0;
}

private static void XORKeyStream(this ptr<ctr> _addr_x, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref ctr x = ref _addr_x.val;

    if (len(dst) < len(src)) {
        panic("crypto/cipher: output smaller than input");
    }
    if (subtle.InexactOverlap(dst[..(int)len(src)], src)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    while (len(src) > 0) {
        if (x.outUsed >= len(x.@out) - x.b.BlockSize()) {
            x.refill();
        }
        var n = xorBytes(dst, src, x.@out[(int)x.outUsed..]);
        dst = dst[(int)n..];
        src = src[(int)n..];
        x.outUsed += n;
    }
});

} // end cipher_package
