// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego

// Package alias implements memory aliasing tests.
namespace go.vendor.golang.org.x.crypto.@internal;

using @unsafe = unsafe_package;

partial class alias_package {

// AnyOverlap reports whether x and y share memory at any (not necessarily
// corresponding) index. The memory beyond the slice length is ignored.
public static bool AnyOverlap(slice<byte> x, slice<byte> y) {
    return len(x) > 0 && len(y) > 0 && ((uintptr)new @unsafe.Pointer(Ꮡ(x, 0))) <= ((uintptr)new @unsafe.Pointer(Ꮡ(y, len(y) - 1))) && ((uintptr)new @unsafe.Pointer(Ꮡ(y, 0))) <= ((uintptr)new @unsafe.Pointer(Ꮡ(x, len(x) - 1)));
}

// InexactOverlap reports whether x and y share memory at any non-corresponding
// index. The memory beyond the slice length is ignored. Note that x and y can
// have different lengths and still not have any inexact overlap.
//
// InexactOverlap can be used to implement the requirements of the crypto/cipher
// AEAD, Block, BlockMode and Stream interfaces.
public static bool InexactOverlap(slice<byte> x, slice<byte> y) {
    if (len(x) == 0 || len(y) == 0 || Ꮡ(x, 0) == Ꮡ(y, 0)) {
        return false;
    }
    return AnyOverlap(x, y);
}

} // end alias_package
