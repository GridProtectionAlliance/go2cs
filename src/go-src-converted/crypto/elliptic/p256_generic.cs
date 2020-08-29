// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x

// package elliptic -- go2cs converted at 2020 August 29 08:30:41 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\p256_generic.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        private static p256Curve p256 = default;

        private static void initP256Arch()
        { 
            // Use pure Go implementation.
            p256 = new p256Curve(p256Params);
        }
    }
}}
