// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !s390x && !arm64 && !ppc64le
// +build !amd64,!s390x,!arm64,!ppc64le

// package elliptic -- go2cs converted at 2022 March 13 05:34:04 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\p256_generic.go
namespace go.crypto;

public static partial class elliptic_package {

private static p256Curve p256 = default;

private static void initP256Arch() { 
    // Use pure Go implementation.
    p256 = new p256Curve(p256Params);
}

} // end elliptic_package
