// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!386,!s390x,!ppc64le,!arm64

// package sha256 -- go2cs converted at 2020 October 09 04:54:40 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Go\src\crypto\sha256\sha256block_generic.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha256_package
    {
        private static var block = blockGeneric;
    }
}}
