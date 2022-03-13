// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Cipher block chaining (CBC) mode.

// CBC provides confidentiality by xoring (chaining) each plaintext block
// with the previous ciphertext block before applying the block cipher.

// See NIST SP 800-38A, pp 10-11

// package cipher -- go2cs converted at 2022 March 13 05:30:35 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Program Files\Go\src\crypto\cipher\cbc.go
namespace go.crypto;

using subtle = crypto.@internal.subtle_package;

public static partial class cipher_package {

private partial struct cbc {
    public Block b;
    public nint blockSize;
    public slice<byte> iv;
    public slice<byte> tmp;
}

private static ptr<cbc> newCBC(Block b, slice<byte> iv) {
    return addr(new cbc(b:b,blockSize:b.BlockSize(),iv:dup(iv),tmp:make([]byte,b.BlockSize()),));
}

private partial struct cbcEncrypter { // : cbc
}

// cbcEncAble is an interface implemented by ciphers that have a specific
// optimized implementation of CBC encryption, like crypto/aes.
// NewCBCEncrypter will check for this interface and return the specific
// BlockMode if found.
private partial interface cbcEncAble {
    BlockMode NewCBCEncrypter(slice<byte> iv);
}

// NewCBCEncrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size.
public static BlockMode NewCBCEncrypter(Block b, slice<byte> iv) => func((_, panic, _) => {
    if (len(iv) != b.BlockSize()) {
        panic("cipher.NewCBCEncrypter: IV length must equal block size");
    }
    {
        cbcEncAble (cbc, ok) = cbcEncAble.As(b._<cbcEncAble>())!;

        if (ok) {
            return cbc.NewCBCEncrypter(iv);
        }
    }
    return (cbcEncrypter.val)(newCBC(b, iv));
});

private static nint BlockSize(this ptr<cbcEncrypter> _addr_x) {
    ref cbcEncrypter x = ref _addr_x.val;

    return x.blockSize;
}

private static void CryptBlocks(this ptr<cbcEncrypter> _addr_x, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref cbcEncrypter x = ref _addr_x.val;

    if (len(src) % x.blockSize != 0) {
        panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        panic("crypto/cipher: output smaller than input");
    }
    if (subtle.InexactOverlap(dst[..(int)len(src)], src)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    var iv = x.iv;

    while (len(src) > 0) { 
        // Write the xor to dst, then encrypt in place.
        xorBytes(dst[..(int)x.blockSize], src[..(int)x.blockSize], iv);
        x.b.Encrypt(dst[..(int)x.blockSize], dst[..(int)x.blockSize]); 

        // Move to the next block with this block as the next iv.
        iv = dst[..(int)x.blockSize];
        src = src[(int)x.blockSize..];
        dst = dst[(int)x.blockSize..];
    } 

    // Save the iv for the next CryptBlocks call.
    copy(x.iv, iv);
});

private static void SetIV(this ptr<cbcEncrypter> _addr_x, slice<byte> iv) => func((_, panic, _) => {
    ref cbcEncrypter x = ref _addr_x.val;

    if (len(iv) != len(x.iv)) {
        panic("cipher: incorrect length IV");
    }
    copy(x.iv, iv);
});

private partial struct cbcDecrypter { // : cbc
}

// cbcDecAble is an interface implemented by ciphers that have a specific
// optimized implementation of CBC decryption, like crypto/aes.
// NewCBCDecrypter will check for this interface and return the specific
// BlockMode if found.
private partial interface cbcDecAble {
    BlockMode NewCBCDecrypter(slice<byte> iv);
}

// NewCBCDecrypter returns a BlockMode which decrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size and must match the iv used to encrypt the data.
public static BlockMode NewCBCDecrypter(Block b, slice<byte> iv) => func((_, panic, _) => {
    if (len(iv) != b.BlockSize()) {
        panic("cipher.NewCBCDecrypter: IV length must equal block size");
    }
    {
        cbcDecAble (cbc, ok) = cbcDecAble.As(b._<cbcDecAble>())!;

        if (ok) {
            return cbc.NewCBCDecrypter(iv);
        }
    }
    return (cbcDecrypter.val)(newCBC(b, iv));
});

private static nint BlockSize(this ptr<cbcDecrypter> _addr_x) {
    ref cbcDecrypter x = ref _addr_x.val;

    return x.blockSize;
}

private static void CryptBlocks(this ptr<cbcDecrypter> _addr_x, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref cbcDecrypter x = ref _addr_x.val;

    if (len(src) % x.blockSize != 0) {
        panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        panic("crypto/cipher: output smaller than input");
    }
    if (subtle.InexactOverlap(dst[..(int)len(src)], src)) {
        panic("crypto/cipher: invalid buffer overlap");
    }
    if (len(src) == 0) {
        return ;
    }
    var end = len(src);
    var start = end - x.blockSize;
    var prev = start - x.blockSize; 

    // Copy the last block of ciphertext in preparation as the new iv.
    copy(x.tmp, src[(int)start..(int)end]); 

    // Loop over all but the first block.
    while (start > 0) {
        x.b.Decrypt(dst[(int)start..(int)end], src[(int)start..(int)end]);
        xorBytes(dst[(int)start..(int)end], dst[(int)start..(int)end], src[(int)prev..(int)start]);

        end = start;
        start = prev;
        prev -= x.blockSize;
    } 

    // The first block is special because it uses the saved iv.
    x.b.Decrypt(dst[(int)start..(int)end], src[(int)start..(int)end]);
    xorBytes(dst[(int)start..(int)end], dst[(int)start..(int)end], x.iv); 

    // Set the new iv to the first block we copied earlier.
    (x.iv, x.tmp) = (x.tmp, x.iv);
});

private static void SetIV(this ptr<cbcDecrypter> _addr_x, slice<byte> iv) => func((_, panic, _) => {
    ref cbcDecrypter x = ref _addr_x.val;

    if (len(iv) != len(x.iv)) {
        panic("cipher: incorrect length IV");
    }
    copy(x.iv, iv);
});

} // end cipher_package
