// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (linux && amd64) || (freebsd && amd64) || (linux && arm64) || (linux && ppc64le)
// +build linux,amd64 freebsd,amd64 linux,arm64 linux,ppc64le

// package cgo -- go2cs converted at 2022 March 13 05:27:33 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\sigaction.go
namespace go.runtime;
// Import "unsafe" because we use go:linkname.

using _@unsafe_ = @unsafe_package;

public static partial class cgo_package {

// When using cgo, call the C library for sigaction, so that we call into
// any sanitizer interceptors. This supports using the sanitizers
// with Go programs. The thread and memory sanitizers only apply to
// C/C++ code; this permits that code to see the Go runtime's existing signal
// handlers when registering new signal handlers for the process.

//go:cgo_import_static x_cgo_sigaction
//go:linkname x_cgo_sigaction x_cgo_sigaction
//go:linkname _cgo_sigaction _cgo_sigaction
private static byte x_cgo_sigaction = default;
private static var _cgo_sigaction = _addr_x_cgo_sigaction;

} // end cgo_package
