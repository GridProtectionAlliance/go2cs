// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd
// +build openbsd

// package cgo -- go2cs converted at 2022 March 13 05:27:33 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\openbsd.go
namespace go.runtime;

using _@unsafe_ = @unsafe_package;

public static partial class cgo_package { // for go:linkname

// Supply __guard_local because we don't link against the standard
// OpenBSD crt0.o and the libc dynamic library needs it.

//go:linkname _guard_local __guard_local

private static System.UIntPtr _guard_local = default;

// This is normally marked as hidden and placed in the
// .openbsd.randomdata section.
//go:cgo_export_dynamic __guard_local __guard_local

} // end cgo_package
