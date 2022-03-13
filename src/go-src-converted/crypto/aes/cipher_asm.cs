// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || arm64
// +build amd64 arm64

// package aes -- go2cs converted at 2022 March 13 05:32:27 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\cipher_asm.go
namespace go.crypto;

using cipher = crypto.cipher_package;
using subtle = crypto.@internal.subtle_package;
using cpu = @internal.cpu_package;


// defined in asm_*.s

//go:noescape

public static partial class aes_package {

private static void encryptBlockAsm(nint nr, ptr<uint> xk, ptr<byte> dst, ptr<byte> src);

//go:noescape
private static void decryptBlockAsm(nint nr, ptr<uint> xk, ptr<byte> dst, ptr<byte> src);

//go:noescape
private static void expandKeyAsm(nint nr, ptr<byte> key, ptr<uint> enc, ptr<uint> dec);

private partial struct aesCipherAsm {
    public ref aesCipher aesCipher => ref aesCipher_val;
}

private static var supportsAES = cpu.X86.HasAES || cpu.ARM64.HasAES;
private static var supportsGFMUL = cpu.X86.HasPCLMULQDQ || cpu.ARM64.HasPMULL;

private static (cipher.Block, error) newCipher(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;

    if (!supportsAES) {>>MARKER:FUNCTION_expandKeyAsm_BLOCK_PREFIX<<
        return newCipherGeneric(key);
    }
    var n = len(key) + 28;
    ref aesCipherAsm c = ref heap(new aesCipherAsm(aesCipher{make([]uint32,n),make([]uint32,n)}), out ptr<aesCipherAsm> _addr_c);
    nint rounds = default;
    switch (len(key)) {
        case 128 / 8: 
            rounds = 10;
            break;
        case 192 / 8: 
            rounds = 12;
            break;
        case 256 / 8: 
            rounds = 14;
            break;
    }

    expandKeyAsm(rounds, _addr_key[0], _addr_c.enc[0], _addr_c.dec[0]);
    if (supportsAES && supportsGFMUL) {>>MARKER:FUNCTION_decryptBlockAsm_BLOCK_PREFIX<<
        return (addr(new aesCipherGCM(c)), error.As(null!)!);
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
    if (len(dst) < BlockSize) {
        panic("crypto/aes: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/aes: invalid buffer overlap");
    }
    encryptBlockAsm(len(c.enc) / 4 - 1, _addr_c.enc[0], _addr_dst[0], _addr_src[0]);
});

private static void Decrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref aesCipherAsm c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/aes: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/aes: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/aes: invalid buffer overlap");
    }
    decryptBlockAsm(len(c.dec) / 4 - 1, _addr_c.dec[0], _addr_dst[0], _addr_src[0]);
});

// expandKey is used by BenchmarkExpand to ensure that the asm implementation
// of key expansion is used for the benchmark when it is available.
private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec) {
    if (supportsAES) {
        nint rounds = 10; // rounds needed for AES128
        switch (len(key)) {
            case 192 / 8: 
                rounds = 12;
                break;
            case 256 / 8: 
                rounds = 14;
                break;
        }
        expandKeyAsm(rounds, _addr_key[0], _addr_enc[0], _addr_dec[0]);
    }
    else
 {
        expandKeyGo(key, enc, dec);
    }
}

} // end aes_package
