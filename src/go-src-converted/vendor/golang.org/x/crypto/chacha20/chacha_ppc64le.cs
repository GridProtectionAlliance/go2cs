// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc && !purego
// +build gc,!purego

// package chacha20 -- go2cs converted at 2022 March 06 23:36:32 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_ppc64le.go


namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20_package {

private static readonly nint bufSize = 256;

//go:noescape


//go:noescape
private static void chaCha20_ctr32_vsx(ptr<byte> @out, ptr<byte> inp, nint len, ptr<array<uint>> key, ptr<uint> counter);

private static void xorKeyStreamBlocks(this ptr<Cipher> _addr_c, slice<byte> dst, slice<byte> src) {
    ref Cipher c = ref _addr_c.val;

    chaCha20_ctr32_vsx(_addr_dst[0], _addr_src[0], len(src), _addr_c.key, _addr_c.counter);
}

} // end chacha20_package
