// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !386 && !amd64 && !s390x && !arm && !arm64 && !ppc64 && !ppc64le && !mips && !mipsle && !mips64 && !mips64le && !riscv64 && !wasm
// +build !386,!amd64,!s390x,!arm,!arm64,!ppc64,!ppc64le,!mips,!mipsle,!mips64,!mips64le,!riscv64,!wasm

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\indexbyte_generic.go


namespace go.@internal;

public static partial class bytealg_package {

public static nint IndexByte(slice<byte> b, byte c) {
    foreach (var (i, x) in b) {
        if (x == c) {
            return i;
        }
    }    return -1;

}

public static nint IndexByteString(@string s, byte c) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;

}

} // end bytealg_package
