// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !386 && !arm && !s390x && !arm64
// +build !amd64,!386,!arm,!s390x,!arm64

// package sha1 -- go2cs converted at 2022 March 13 05:34:23 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Program Files\Go\src\crypto\sha1\sha1block_generic.go
namespace go.crypto;

public static partial class sha1_package {

private static var block = blockGeneric;

} // end sha1_package
