// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x,!ppc64le

// package sha512 -- go2cs converted at 2020 October 08 03:35:43 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Go\src\crypto\sha512\sha512block_generic.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha512_package
    {
        private static void block(ptr<digest> _addr_dig, slice<byte> p)
        {
            ref digest dig = ref _addr_dig.val;

            blockGeneric(dig, p);
        }
    }
}}
