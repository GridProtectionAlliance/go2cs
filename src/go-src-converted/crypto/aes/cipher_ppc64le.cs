// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2022 March 06 22:18:13 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\cipher_ppc64le.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;

namespace go.crypto;

public static partial class aes_package {

    // defined in asm_ppc64le.s

    //go:noescape
private static nint setEncryptKeyAsm(ptr<byte> key, nint keylen, ptr<uint> enc);

//go:noescape
private static nint setDecryptKeyAsm(ptr<byte> key, nint keylen, ptr<uint> dec);

//go:noescape
private static nint doEncryptKeyAsm(ptr<byte> key, nint keylen, ptr<uint> dec);

//go:noescape
private static void encryptBlockAsm(ptr<byte> dst, ptr<byte> src, ptr<uint> enc);

//go:noescape
private static void decryptBlockAsm(ptr<byte> dst, ptr<byte> src, ptr<uint> dec);

private partial struct aesCipherAsm {
    public ref aesCipher aesCipher => ref aesCipher_val;
}

private static (cipher.Block, error) newCipher(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;

    nint n = 64; // size is fixed for all and round value is stored inside it too
    ref aesCipherAsm c = ref heap(new aesCipherAsm(aesCipher{make([]uint32,n),make([]uint32,n)}), out ptr<aesCipherAsm> _addr_c);
    var k = len(key);

    nint ret = 0;
    ret += setEncryptKeyAsm(_addr_key[0], k * 8, _addr_c.enc[0]);
    ret += setDecryptKeyAsm(_addr_key[0], k * 8, _addr_c.dec[0]);

    if (ret > 0) {>>MARKER:FUNCTION_decryptBlockAsm_BLOCK_PREFIX<<
        return (null, error.As(KeySizeError(k))!);
    }
    return (_addr_c, error.As(null!)!);

}

private static nint BlockSize(this ptr<aesCipherAsm> _addr_c) {
    ref aesCipherAsm c = ref _addr_c.val;

    return BlockSize;
}

private static void Encrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref aesCipherAsm c = ref _addr_c.val;

    if (len(src) < BlockSize) {>>MARKER:FUNCTION_encryptBlockAsm_BLOCK_PREFIX<<
        panic("crypto/aes: input not full block");
    }
    if (len(dst) < BlockSize) {>>MARKER:FUNCTION_doEncryptKeyAsm_BLOCK_PREFIX<<
        panic("crypto/aes: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {>>MARKER:FUNCTION_setDecryptKeyAsm_BLOCK_PREFIX<<
        panic("crypto/aes: invalid buffer overlap");
    }
    encryptBlockAsm(_addr_dst[0], _addr_src[0], _addr_c.enc[0]);

});

private static void Decrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref aesCipherAsm c = ref _addr_c.val;

    if (len(src) < BlockSize) {>>MARKER:FUNCTION_setEncryptKeyAsm_BLOCK_PREFIX<<
        panic("crypto/aes: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/aes: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/aes: invalid buffer overlap");
    }
    decryptBlockAsm(_addr_dst[0], _addr_src[0], _addr_c.dec[0]);

});

// expandKey is used by BenchmarkExpand to ensure that the asm implementation
// of key expansion is used for the benchmark when it is available.
private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec) {
    setEncryptKeyAsm(_addr_key[0], len(key) * 8, _addr_enc[0]);
    setDecryptKeyAsm(_addr_key[0], len(key) * 8, _addr_dec[0]);
}

} // end aes_package
