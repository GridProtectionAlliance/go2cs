// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build s390x || ppc64le
// +build s390x ppc64le

// package sha512 -- go2cs converted at 2022 March 06 22:18:08 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Program Files\Go\src\crypto\sha512\sha512block_decl.go


namespace go.crypto;

public static partial class sha512_package {

    //go:noescape
private static void block(ptr<digest> dig, slice<byte> p);

} // end sha512_package
