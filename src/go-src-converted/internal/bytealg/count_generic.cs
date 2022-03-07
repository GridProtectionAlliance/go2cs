// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !arm && !arm64 && !ppc64le && !ppc64 && !riscv64 && !s390x
// +build !amd64,!arm,!arm64,!ppc64le,!ppc64,!riscv64,!s390x

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\count_generic.go


namespace go.@internal;

public static partial class bytealg_package {

public static nint Count(slice<byte> b, byte c) {
    nint n = 0;
    foreach (var (_, x) in b) {
        if (x == c) {
            n++;
        }
    }    return n;

}

public static nint CountString(@string s, byte c) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == c) {
            n++;
        }
    }
    return n;

}

} // end bytealg_package
