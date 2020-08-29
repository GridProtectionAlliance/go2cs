// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !cgo

// package x509 -- go2cs converted at 2020 August 29 08:31:54 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_nocgo_darwin.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        private static (ref CertPool, error) loadSystemRoots()
        {
            return execSecurityRoots();
        }
    }
}}
