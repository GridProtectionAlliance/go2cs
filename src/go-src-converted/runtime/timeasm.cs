// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Declarations for operating systems implementing time.now directly in assembly.

//go:build !faketime && (windows || (linux && amd64))
// +build !faketime
// +build windows linux,amd64

// package runtime -- go2cs converted at 2022 March 06 22:12:13 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\timeasm.go
using _@unsafe_ = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    //go:linkname time_now time.now
private static (long, int, long) time_now();

} // end runtime_package
