// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego
namespace go.hash;

using @unsafe = unsafe_package;

partial class maphash_package {

//go:linkname runtime_rand runtime.rand
internal static partial uint64 runtime_rand();

//go:linkname runtime_memhash runtime.memhash
//go:noescape
internal static partial uintptr runtime_memhash(@unsafe.Pointer p, uintptr seed, uintptr s);

internal static uint64 rthash(slice<byte> buf, uint64 seed) {
    if (len(buf) == 0) {
        return seed;
    }
    nint lenΔ1 = len(buf);
    // The runtime hasher only works on uintptr. For 64-bit
    // architectures, we use the hasher directly. Otherwise,
    // we use two parallel hashers on the lower and upper 32 bits.
    if (@unsafe.Sizeof((uintptr)0) == 8) {
        return (uint64)runtime_memhash(new @unsafe.Pointer(Ꮡ(buf, 0)), (uintptr)seed, (uintptr)lenΔ1);
    }
    var lo = runtime_memhash(new @unsafe.Pointer(Ꮡ(buf, 0)), (uintptr)seed, (uintptr)lenΔ1);
    var hi = runtime_memhash(new @unsafe.Pointer(Ꮡ(buf, 0)), (uintptr)((seed >> (int)(32))), (uintptr)lenΔ1);
    return (uint64)(((uint64)hi << (int)(32)) | (uint64)lo);
}

internal static uint64 rthashString(@string s, uint64 state) {
    var buf = @unsafe.Slice(@unsafe.StringData(s), len(s));
    return rthash(buf, state);
}

internal static uint64 randUint64() {
    return runtime_rand();
}

} // end maphash_package
