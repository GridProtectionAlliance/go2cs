// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!386,!arm,!s390x,!arm64

// package sha1 -- go2cs converted at 2020 October 09 04:54:38 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1block_generic.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        private static var block = blockGeneric;
    }
}}
