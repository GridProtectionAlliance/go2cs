// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !386 && !amd64 && !s390x && !arm && !arm64 && !ppc64 && !ppc64le && !mips && !mipsle && !wasm && !mips64 && !mips64le
// +build !386,!amd64,!s390x,!arm,!arm64,!ppc64,!ppc64le,!mips,!mipsle,!wasm,!mips64,!mips64le

// package bytealg -- go2cs converted at 2022 March 13 05:40:51 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\compare_generic.go
namespace go.@internal;

using _@unsafe_ = @unsafe_package;

public static partial class bytealg_package { // for go:linkname

public static nint Compare(slice<byte> a, slice<byte> b) {
    var l = len(a);
    if (len(b) < l) {
        l = len(b);
    }
    if (l == 0 || _addr_a[0] == _addr_b[0]) {
        goto samebytes;
    }
    for (nint i = 0; i < l; i++) {
        var c1 = a[i];
        var c2 = b[i];
        if (c1 < c2) {
            return -1;
        }
        if (c1 > c2) {
            return +1;
        }
    }
samebytes:
    if (len(a) < len(b)) {
        return -1;
    }
    if (len(a) > len(b)) {
        return +1;
    }
    return 0;
}

//go:linkname runtime_cmpstring runtime.cmpstring
private static nint runtime_cmpstring(@string a, @string b) {
    var l = len(a);
    if (len(b) < l) {
        l = len(b);
    }
    for (nint i = 0; i < l; i++) {
        var c1 = a[i];
        var c2 = b[i];
        if (c1 < c2) {
            return -1;
        }
        if (c1 > c2) {
            return +1;
        }
    }
    if (len(a) < len(b)) {
        return -1;
    }
    if (len(a) > len(b)) {
        return +1;
    }
    return 0;
}

} // end bytealg_package
