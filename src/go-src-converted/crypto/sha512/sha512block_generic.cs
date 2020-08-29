// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x,!ppc64le

// package sha512 -- go2cs converted at 2020 August 29 08:29:50 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Go\src\crypto\sha512\sha512block_generic.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha512_package
    {
        private static var block = blockGeneric;
    }
}}
