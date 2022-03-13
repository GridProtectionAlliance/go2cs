// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly
// +build dragonfly

// package cgo -- go2cs converted at 2022 March 13 05:27:33 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\dragonfly.go
namespace go.runtime;

using _@unsafe_ = @unsafe_package;

public static partial class cgo_package { // for go:linkname

// Supply environ and __progname, because we don't
// link against the standard DragonFly crt0.o and the
// libc dynamic library needs them.

//go:linkname _environ environ
//go:linkname _progname __progname

private static System.UIntPtr _environ = default;
private static System.UIntPtr _progname = default;

} // end cgo_package
