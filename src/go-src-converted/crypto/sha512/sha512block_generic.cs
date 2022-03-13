// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !s390x && !ppc64le
// +build !amd64,!s390x,!ppc64le

// package sha512 -- go2cs converted at 2022 March 13 05:32:21 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Program Files\Go\src\crypto\sha512\sha512block_generic.go
namespace go.crypto;

public static partial class sha512_package {

private static void block(ptr<digest> _addr_dig, slice<byte> p) {
    ref digest dig = ref _addr_dig.val;

    blockGeneric(dig, p);
}

} // end sha512_package
