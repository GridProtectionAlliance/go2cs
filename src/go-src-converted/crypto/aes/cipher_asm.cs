// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (amd64 || arm64 || ppc64 || ppc64le) && !purego
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using alias = go.crypto.@internal.alias_package;
using boring = go.crypto.@internal.boring_package;
using cpu = go.@internal.cpu_package;
using goarch = go.@internal.goarch_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;

partial class aes_package {

// defined in asm_*.s

//go:noescape
internal static partial void encryptBlockAsm(nint nr, ж<uint32> xk, ж<byte> dst, ж<byte> src);

//go:noescape
internal static partial void decryptBlockAsm(nint nr, ж<uint32> xk, ж<byte> dst, ж<byte> src);

//go:noescape
internal static partial void expandKeyAsm(nint nr, ж<byte> key, ж<uint32> enc, ж<uint32> dec);

[GoType] partial struct aesCipherAsm {
    internal partial ref aesCipher aesCipher { get; }
}

// aesCipherGCM implements crypto/cipher.gcmAble so that crypto/cipher.NewGCM
// will use the optimised implementation in aes_gcm.go when possible.
// Instances of this type only exist when hasGCMAsm returns true. Likewise,
// the gcmAble implementation is in aes_gcm.go.
[GoType] partial struct aesCipherGCM {
    internal partial ref aesCipherAsm aesCipherAsm { get; }
}

internal static bool supportsAES = cpu.X86.HasAES || cpu.ARM64.HasAES || goarch.IsPpc64 == 1 || goarch.IsPpc64le == 1;

internal static bool supportsGFMUL = cpu.X86.HasPCLMULQDQ || cpu.ARM64.HasPMULL;

internal static (cipher.Block, error) newCipher(slice<byte> key) {
    if (!supportsAES) {
        return newCipherGeneric(key);
    }
    // Note that under certain circumstances, we only return the inner aesCipherAsm.
    // This avoids an unnecessary allocation of the aesCipher struct.
    ref var c = ref heap<aesCipherGCM>(out var Ꮡc);
    c = new aesCipherGCM(new aesCipherAsm(new aesCipher(l: (uint8)(len(key) + 28))));
    nint rounds = default!;
    var exprᴛ1 = len(key);
    if (exprᴛ1 == 128 / 8) {
        rounds = 10;
    }
    else if (exprᴛ1 == 192 / 8) {
        rounds = 12;
    }
    else if (exprᴛ1 == 256 / 8) {
        rounds = 14;
    }
    else { /* default: */
        return (default!, ((KeySizeError)len(key)));
    }

    expandKeyAsm(rounds, Ꮡ(key, 0), Ꮡc.at(aesCipherGCM.Ꮡenc, 0), Ꮡc.at(aesCipherGCM.Ꮡdec, 0));
    if (supportsAES && supportsGFMUL) {
        return (new aesCipherGCMжBlock(Ꮡc), default!);
    }
    return (new aesCipherAsmжBlock(Ꮡc.of(aesCipherGCM.ᏑaesCipherAsm)), default!);
}

[GoRecv] internal static nint BlockSize(this ref aesCipherAsm c) {
    return ΔBlockSize;
}

[GoRecv] internal static void Encrypt(this ref aesCipherAsm c, slice<byte> dst, slice<byte> src) {
    boring.Unreachable();
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/aes: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/aes: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    encryptBlockAsm((nint)c.l / 4 - 1, Ꮡ(c.enc[0]), Ꮡ(dst, 0), Ꮡ(src, 0));
}

[GoRecv] internal static void Decrypt(this ref aesCipherAsm c, slice<byte> dst, slice<byte> src) {
    boring.Unreachable();
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/aes: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/aes: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    decryptBlockAsm((nint)c.l / 4 - 1, Ꮡ(c.dec[0]), Ꮡ(dst, 0), Ꮡ(src, 0));
}

// expandKey is used by BenchmarkExpand to ensure that the asm implementation
// of key expansion is used for the benchmark when it is available.
internal static void expandKey(slice<byte> key, slice<uint32> enc, slice<uint32> dec) {
    if (supportsAES){
        nint rounds = 10;
        // rounds needed for AES128
        var exprᴛ1 = len(key);
        if (exprᴛ1 == 192 / 8) {
            rounds = 12;
        }
        else if (exprᴛ1 == 256 / 8) {
            rounds = 14;
        }

        expandKeyAsm(rounds, Ꮡ(key, 0), Ꮡ(enc, 0), Ꮡ(dec, 0));
    } else {
        expandKeyGo(key, enc, dec);
    }
}

} // end aes_package
