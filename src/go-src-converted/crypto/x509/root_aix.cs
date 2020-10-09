// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 October 09 04:54:52 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_aix.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // Possible certificate files; stop after finding one.
        private static @string certFiles = new slice<@string>(new @string[] { "/var/ssl/certs/ca-bundle.crt" });
    }
}}
