// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips64 || mips64le
// +build mips64 mips64le

// package runtime -- go2cs converted at 2022 March 06 22:12:00 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs_mips64x.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // Called from assembly only; declared for go vet.
private static void load_g();
private static void save_g();

//go:noescape
private static void asmcgocall_no_g(unsafe.Pointer fn, unsafe.Pointer arg);

} // end runtime_package
