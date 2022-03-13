// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (!arm64 && !s390x && !ppc64le) || (arm64 && !go1.11) || !gc || purego
// +build !arm64,!s390x,!ppc64le arm64,!go1.11 !gc purego

// package chacha20 -- go2cs converted at 2022 March 13 06:44:35 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_noasm.go
namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20_package {

private static readonly var bufSize = blockSize;



private static void xorKeyStreamBlocks(this ptr<Cipher> _addr_s, slice<byte> dst, slice<byte> src) {
    ref Cipher s = ref _addr_s.val;

    s.xorKeyStreamBlocksGeneric(dst, src);
}

} // end chacha20_package
