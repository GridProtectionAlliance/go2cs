// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64 !go1.7 gccgo appengine

// package chacha20poly1305 -- go2cs converted at 2020 August 29 10:11:13 UTC
// import "vendor/golang_org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang_org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\chacha20poly1305\chacha20poly1305_noasm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class chacha20poly1305_package
    {
        private static slice<byte> seal(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            return c.sealGeneric(dst, nonce, plaintext, additionalData);
        }

        private static (slice<byte>, error) open(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData)
        {
            return c.openGeneric(dst, nonce, ciphertext, additionalData);
        }
    }
}}}}}
