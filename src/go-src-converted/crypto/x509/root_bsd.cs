// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd openbsd

// package x509 -- go2cs converted at 2020 August 29 08:31:44 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_bsd.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // Possible certificate files; stop after finding one.
        private static @string certFiles = new slice<@string>(new @string[] { "/usr/local/etc/ssl/cert.pem", "/etc/ssl/cert.pem", "/usr/local/share/certs/ca-root-nss.crt", "/etc/openssl/certs/ca-certificates.crt" });
    }
}}
