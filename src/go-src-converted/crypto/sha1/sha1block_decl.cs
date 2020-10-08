// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm 386 s390x

// package sha1 -- go2cs converted at 2020 October 08 03:36:42 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1block_decl.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        //go:noescape
        private static void block(ptr<digest> dig, slice<byte> p)
;
    }
}}
