// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2022 March 06 22:18:14 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\cipher_s390x.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using cpu = go.@internal.cpu_package;

namespace go.crypto;

public static partial class aes_package {

private partial struct code { // : nint
}

// Function codes for the cipher message family of instructions.
private static readonly code aes128 = 18;
private static readonly nint aes192 = 19;
private static readonly nint aes256 = 20;


private partial struct aesCipherAsm {
    public code function; // code for cipher message instruction
    public slice<byte> key; // key (128, 192 or 256 bits)
    public array<byte> storage; // array backing key slice
}

// cryptBlocks invokes the cipher message (KM) instruction with
// the given function code. This is equivalent to AES in ECB
// mode. The length must be a multiple of BlockSize (16).
//go:noescape
private static void cryptBlocks(code c, ptr<byte> key, ptr<byte> dst, ptr<byte> src, nint length);

private static (cipher.Block, error) newCipher(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;
 
    // The aesCipherAsm type implements the cbcEncAble, cbcDecAble,
    // ctrAble and gcmAble interfaces. We therefore need to check
    // for all the features required to implement these modes.
    // Keep in sync with crypto/tls/common.go.
    if (!(cpu.S390X.HasAES && cpu.S390X.HasAESCBC && cpu.S390X.HasAESCTR && (cpu.S390X.HasGHASH || cpu.S390X.HasAESGCM))) {>>MARKER:FUNCTION_cryptBlocks_BLOCK_PREFIX<<
        return newCipherGeneric(key);
    }
    code function = default;
    switch (len(key)) {
        case 128 / 8: 
            function = aes128;
            break;
        case 192 / 8: 
            function = aes192;
            break;
        case 256 / 8: 
            function = aes256;
            break;
        default: 
            return (null, error.As(KeySizeError(len(key)))!);
            break;
    }

    ref aesCipherAsm c = ref heap(out ptr<aesCipherAsm> _addr_c);
    c.function = function;
    c.key = c.storage[..(int)len(key)];
    copy(c.key, key);
    return (_addr_c, error.As(null!)!);

}

private static nint BlockSize(this ptr<aesCipherAsm> _addr_c) {
    ref aesCipherAsm c = ref _addr_c.val;

    return BlockSize;
}

private static void Encrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
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
    cryptBlocks(c.function, _addr_c.key[0], _addr_dst[0], _addr_src[0], BlockSize);

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
    cryptBlocks(c.function + 128, _addr_c.key[0], _addr_dst[0], _addr_src[0], BlockSize);

});

// expandKey is used by BenchmarkExpand. cipher message (KM) does not need key
// expansion so there is no assembly equivalent.
private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec) {
    expandKeyGo(key, enc, dec);
}

} // end aes_package
