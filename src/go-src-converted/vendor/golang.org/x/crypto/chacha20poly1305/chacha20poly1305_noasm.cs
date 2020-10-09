// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64 gccgo purego

// package chacha20poly1305 -- go2cs converted at 2020 October 09 06:06:17 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305_noasm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20poly1305_package
    {
        private static slice<byte> seal(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            ref chacha20poly1305 c = ref _addr_c.val;

            return c.sealGeneric(dst, nonce, plaintext, additionalData);
        }

        private static (slice<byte>, error) open(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref chacha20poly1305 c = ref _addr_c.val;

            return c.openGeneric(dst, nonce, ciphertext, additionalData);
        }
    }
}}}}}
