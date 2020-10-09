// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64 s390x ppc64le

// package sha256 -- go2cs converted at 2020 October 09 04:54:40 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Go\src\crypto\sha256\sha256block_decl.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha256_package
    {
        //go:noescape
        private static void block(ptr<digest> dig, slice<byte> p)
;
    }
}}
