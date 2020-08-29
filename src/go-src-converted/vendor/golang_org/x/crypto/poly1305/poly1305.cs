// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package poly1305 implements Poly1305 one-time message authentication code as specified in http://cr.yp.to/mac/poly1305-20050329.pdf.

Poly1305 is a fast, one-time authentication function. It is infeasible for an
attacker to generate an authenticator for a message without the key. However, a
key must only be used for a single message. Authenticating two different
messages with the same key allows an attacker to forge authenticators for other
messages with the same key.

Poly1305 was originally coupled with AES in order to make Poly1305-AES. AES was
used with a fixed key in order to generate one-time keys from an nonce.
However, in this package AES isn't used and the one-time key is specified
directly.
*/
// package poly1305 -- go2cs converted at 2020 August 29 10:11:36 UTC
// import "vendor/golang_org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang_org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\poly1305\poly1305.go
// import "golang.org/x/crypto/poly1305"

using subtle = go.crypto.subtle_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        // TagSize is the size, in bytes, of a poly1305 authenticator.
        public static readonly long TagSize = 16L;

        // Verify returns true if mac is a valid authenticator for m with the given
        // key.


        // Verify returns true if mac is a valid authenticator for m with the given
        // key.
        public static bool Verify(ref array<byte> mac, slice<byte> m, ref array<byte> key)
        {
            array<byte> tmp = new array<byte>(16L);
            Sum(ref tmp, m, key);
            return subtle.ConstantTimeCompare(tmp[..], mac[..]) == 1L;
        }
    }
}}}}}
