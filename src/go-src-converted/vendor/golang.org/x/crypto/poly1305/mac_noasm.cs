// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (!amd64 && !ppc64le && !s390x) || !gc || purego
// +build !amd64,!ppc64le,!s390x !gc purego

// package poly1305 -- go2cs converted at 2022 March 13 06:44:59 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\poly1305\mac_noasm.go
namespace go.vendor.golang.org.x.crypto;

public static partial class poly1305_package {

private partial struct mac {
    public ref macGeneric macGeneric => ref macGeneric_val;
}

} // end poly1305_package
