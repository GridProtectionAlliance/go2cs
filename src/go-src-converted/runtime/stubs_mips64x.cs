// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips64 || mips64le
// +build mips64 mips64le

// package runtime -- go2cs converted at 2022 March 13 05:27:11 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs_mips64x.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

// Called from assembly only; declared for go vet.
private static void load_g();
private static void save_g();

//go:noescape
private static void asmcgocall_no_g(unsafe.Pointer fn, unsafe.Pointer arg);

} // end runtime_package
