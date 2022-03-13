// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !s390x && !ppc64le && !arm64
// +build !amd64,!s390x,!ppc64le,!arm64

// package crc32 -- go2cs converted at 2022 March 13 05:28:57 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Program Files\Go\src\hash\crc32\crc32_otherarch.go
namespace go.hash;

public static partial class crc32_package {

private static bool archAvailableIEEE() {
    return false;
}
private static void archInitIEEE() => func((_, panic, _) => {
    panic("not available");
});
private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, _) => {
    panic("not available");
});

private static bool archAvailableCastagnoli() {
    return false;
}
private static void archInitCastagnoli() => func((_, panic, _) => {
    panic("not available");
});
private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, _) => {
    panic("not available");
});

} // end crc32_package
