// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !faketime
namespace go;

partial class syscall_package {

internal const bool faketime = false;

internal static nint faketimeWrite(nint fd, slice<byte> p) {
    // This should never be called since faketime is false.
    throw panic("not implemented");
}

} // end syscall_package
