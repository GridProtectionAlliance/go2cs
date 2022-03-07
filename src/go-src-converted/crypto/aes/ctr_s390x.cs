// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2022 March 06 22:18:15 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\ctr_s390x.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using binary = go.encoding.binary_package;

namespace go.crypto;

public static partial class aes_package {

    // Assert that aesCipherAsm implements the ctrAble interface.
private static ctrAble _ = (aesCipherAsm.val)(null);

// xorBytes xors the contents of a and b and places the resulting values into
// dst. If a and b are not the same length then the number of bytes processed
// will be equal to the length of shorter of the two. Returns the number
// of bytes processed.
//go:noescape
private static nint xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b);

// streamBufferSize is the number of bytes of encrypted counter values to cache.
private static readonly nint streamBufferSize = 32 * BlockSize;



private partial struct aesctr {
    public ptr<aesCipherAsm> block; // block cipher
    public array<ulong> ctr; // next value of the counter (big endian)
    public slice<byte> buffer; // buffer for the encrypted counter values
    public array<byte> storage; // array backing buffer slice
}

// NewCTR returns a Stream which encrypts/decrypts using the AES block
// cipher in counter mode. The length of iv must be the same as BlockSize.
private static cipher.Stream NewCTR(this ptr<aesCipherAsm> _addr_c, slice<byte> iv) => func((_, panic, _) => {
    ref aesCipherAsm c = ref _addr_c.val;

    if (len(iv) != BlockSize) {>>MARKER:FUNCTION_xorBytes_BLOCK_PREFIX<<
        panic("cipher.NewCTR: IV length must equal block size");
    }
    ref aesctr ac = ref heap(out ptr<aesctr> _addr_ac);
    ac.block = c;
    ac.ctr[0] = binary.BigEndian.Uint64(iv[(int)0..]); // high bits
    ac.ctr[1] = binary.BigEndian.Uint64(iv[(int)8..]); // low bits
    ac.buffer = ac.storage[..(int)0];
    return _addr_ac;

});

private static void refill(this ptr<aesctr> _addr_c) {
    ref aesctr c = ref _addr_c.val;
 
    // Fill up the buffer with an incrementing count.
    c.buffer = c.storage[..(int)streamBufferSize];
    var c0 = c.ctr[0];
    var c1 = c.ctr[1];
    {
        nint i = 0;

        while (i < streamBufferSize) {
            binary.BigEndian.PutUint64(c.buffer[(int)i + 0..], c0);
            binary.BigEndian.PutUint64(c.buffer[(int)i + 8..], c1); 

            // Increment in big endian: c0 is high, c1 is low.
            c1++;
            if (c1 == 0) { 
                // add carry
                c0++;
            i += 16;
            }

        }
    }
    (c.ctr[0], c.ctr[1]) = (c0, c1);    cryptBlocks(c.block.function, _addr_c.block.key[0], _addr_c.buffer[0], _addr_c.buffer[0], streamBufferSize);

}

private static void XORKeyStream(this ptr<aesctr> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref aesctr c = ref _addr_c.val;

    if (len(dst) < len(src)) {
        panic("crypto/cipher: output smaller than input");
    }
    if (subtle.InexactOverlap(dst[..(int)len(src)], src)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    while (len(src) > 0) {
        if (len(c.buffer) == 0) {
            c.refill();
        }
        var n = xorBytes(dst, src, c.buffer);
        c.buffer = c.buffer[(int)n..];
        src = src[(int)n..];
        dst = dst[(int)n..];

    }

});

} // end aes_package
