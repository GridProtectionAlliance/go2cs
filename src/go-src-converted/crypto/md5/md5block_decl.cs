// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 386 arm ppc64le ppc64 s390x arm64

// package md5 -- go2cs converted at 2020 October 09 04:54:36 UTC
// import "crypto/md5" ==> using md5 = go.crypto.md5_package
// Original source: C:\Go\src\crypto\md5\md5block_decl.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class md5_package
    {
        private static readonly var haveAsm = true;

        //go:noescape



        //go:noescape

        private static void block(ptr<digest> dig, slice<byte> p)
;
    }
}}
