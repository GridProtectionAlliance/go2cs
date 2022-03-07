// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 || !gc || purego
// +build !amd64 !gc purego

// package chacha20poly1305 -- go2cs converted at 2022 March 06 23:36:33 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305_noasm.go


namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20poly1305_package {

private static slice<byte> seal(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    ref chacha20poly1305 c = ref _addr_c.val;

    return c.sealGeneric(dst, nonce, plaintext, additionalData);
}

private static (slice<byte>, error) open(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref chacha20poly1305 c = ref _addr_c.val;

    return c.openGeneric(dst, nonce, ciphertext, additionalData);
}

} // end chacha20poly1305_package
