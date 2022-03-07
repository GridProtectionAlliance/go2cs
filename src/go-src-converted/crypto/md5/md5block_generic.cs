// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !386 && !arm && !ppc64le && !ppc64 && !s390x && !arm64
// +build !amd64,!386,!arm,!ppc64le,!ppc64,!s390x,!arm64

// package md5 -- go2cs converted at 2022 March 06 22:19:24 UTC
// import "crypto/md5" ==> using md5 = go.crypto.md5_package
// Original source: C:\Program Files\Go\src\crypto\md5\md5block_generic.go


namespace go.crypto;

public static partial class md5_package {

private static readonly var haveAsm = false;



private static var block = blockGeneric;

} // end md5_package
