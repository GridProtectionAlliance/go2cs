// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Hashing algorithm inspired by
// wyhash: https://github.com/wangyi-fudan/wyhash

//go:build amd64 || arm64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x || wasm
// +build amd64 arm64 mips64 mips64le ppc64 ppc64le riscv64 s390x wasm

// package runtime -- go2cs converted at 2022 March 06 22:08:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\hash64.go
using math = go.runtime.@internal.math_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint m1 = 0xa0761d6478bd642f;
private static readonly nuint m2 = 0xe7037ed1a0b428db;
private static readonly nuint m3 = 0x8ebc6af09c88c6e3;
private static readonly nuint m4 = 0x589965cc75374cc3;
private static readonly nuint m5 = 0x1d8e4e27c47d124f;


private static System.UIntPtr memhashFallback(unsafe.Pointer p, System.UIntPtr seed, System.UIntPtr s) {
    System.UIntPtr a = default;    System.UIntPtr b = default;

    seed ^= hashkey[0] ^ m1;

    if (s == 0) 
        return seed;
    else if (s < 4) 
        a = uintptr(new ptr<ptr<ptr<byte>>>(p));
        a |= uintptr(new ptr<ptr<ptr<byte>>>(add(p, s >> 1))) << 8;
        a |= uintptr(new ptr<ptr<ptr<byte>>>(add(p, s - 1))) << 16;
    else if (s == 4) 
        a = r4(p);
        b = a;
    else if (s < 8) 
        a = r4(p);
        b = r4(add(p, s - 4));
    else if (s == 8) 
        a = r8(p);
        b = a;
    else if (s <= 16) 
        a = r8(p);
        b = r8(add(p, s - 8));
    else 
        var l = s;
        if (l > 48) {
            var seed1 = seed;
            var seed2 = seed;
            while (l > 48) {
                seed = mix(r8(p) ^ m2, r8(add(p, 8)) ^ seed);
                seed1 = mix(r8(add(p, 16)) ^ m3, r8(add(p, 24)) ^ seed1);
                seed2 = mix(r8(add(p, 32)) ^ m4, r8(add(p, 40)) ^ seed2);
                p = add(p, 48);
                l -= 48;
            }

            seed ^= seed1 ^ seed2;

        }
        while (l > 16) {
            seed = mix(r8(p) ^ m2, r8(add(p, 8)) ^ seed);
            p = add(p, 16);
            l -= 16;
        }
        a = r8(add(p, l - 16));
        b = r8(add(p, l - 8));
        return mix(m5 ^ s, mix(a ^ m2, b ^ seed));

}

private static System.UIntPtr memhash32Fallback(unsafe.Pointer p, System.UIntPtr seed) {
    var a = r4(p);
    return mix(m5 ^ 4, mix(a ^ m2, a ^ seed ^ hashkey[0] ^ m1));
}

private static System.UIntPtr memhash64Fallback(unsafe.Pointer p, System.UIntPtr seed) {
    var a = r8(p);
    return mix(m5 ^ 8, mix(a ^ m2, a ^ seed ^ hashkey[0] ^ m1));
}

private static System.UIntPtr mix(System.UIntPtr a, System.UIntPtr b) {
    var (hi, lo) = math.Mul64(uint64(a), uint64(b));
    return uintptr(hi ^ lo);
}

private static System.UIntPtr r4(unsafe.Pointer p) {
    return uintptr(readUnaligned32(p));
}

private static System.UIntPtr r8(unsafe.Pointer p) {
    return uintptr(readUnaligned64(p));
}

} // end runtime_package
