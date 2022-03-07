// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cipher -- go2cs converted at 2022 March 06 22:18:10 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Program Files\Go\src\crypto\cipher\xor_amd64.go


namespace go.crypto;

public static partial class cipher_package {

    // xorBytes xors the bytes in a and b. The destination should have enough
    // space, otherwise xorBytes will panic. Returns the number of bytes xor'd.
private static nint xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b) {
    var n = len(a);
    if (len(b) < n) {
        n = len(b);
    }
    if (n == 0) {
        return 0;
    }
    _ = dst[n - 1];
    xorBytesSSE2(_addr_dst[0], _addr_a[0], _addr_b[0], n); // amd64 must have SSE2
    return n;

}

private static void xorWords(slice<byte> dst, slice<byte> a, slice<byte> b) {
    xorBytes(dst, a, b);
}

//go:noescape
private static void xorBytesSSE2(ptr<byte> dst, ptr<byte> a, ptr<byte> b, nint n);

} // end cipher_package
