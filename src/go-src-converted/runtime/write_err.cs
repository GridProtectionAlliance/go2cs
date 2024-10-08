// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !android
// +build !android

// package runtime -- go2cs converted at 2022 March 13 05:27:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\write_err.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

private static void writeErr(slice<byte> b) {
    write(2, @unsafe.Pointer(_addr_b[0]), int32(len(b)));
}

} // end runtime_package
