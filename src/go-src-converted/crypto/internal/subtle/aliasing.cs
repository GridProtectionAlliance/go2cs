// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !appengine
// +build !appengine

// Package subtle implements functions that are often useful in cryptographic
// code but require careful thought to use correctly.
//
// This is a mirror of golang.org/x/crypto/internal/subtle.

// package subtle -- go2cs converted at 2022 March 13 05:30:38 UTC
// import "crypto/internal/subtle" ==> using subtle = go.crypto.@internal.subtle_package
// Original source: C:\Program Files\Go\src\crypto\internal\subtle\aliasing.go
namespace go.crypto.@internal;
// import "crypto/internal/subtle"

using @unsafe = @unsafe_package;

public static partial class subtle_package {

// AnyOverlap reports whether x and y share memory at any (not necessarily
// corresponding) index. The memory beyond the slice length is ignored.
public static bool AnyOverlap(slice<byte> x, slice<byte> y) {
    return len(x) > 0 && len(y) > 0 && uintptr(@unsafe.Pointer(_addr_x[0])) <= uintptr(@unsafe.Pointer(_addr_y[len(y) - 1])) && uintptr(@unsafe.Pointer(_addr_y[0])) <= uintptr(@unsafe.Pointer(_addr_x[len(x) - 1]));
}

// InexactOverlap reports whether x and y share memory at any non-corresponding
// index. The memory beyond the slice length is ignored. Note that x and y can
// have different lengths and still not have any inexact overlap.
//
// InexactOverlap can be used to implement the requirements of the crypto/cipher
// AEAD, Block, BlockMode and Stream interfaces.
public static bool InexactOverlap(slice<byte> x, slice<byte> y) {
    if (len(x) == 0 || len(y) == 0 || _addr_x[0] == _addr_y[0]) {
        return false;
    }
    return AnyOverlap(x, y);
}

} // end subtle_package
