// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,amd64

// package cgo -- go2cs converted at 2020 August 29 08:21:41 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Go\src\runtime\cgo\sigaction.go
// Import "unsafe" because we use go:linkname.
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class cgo_package
    {
        // When using cgo, call the C library for sigaction, so that we call into
        // any sanitizer interceptors. This supports using the memory
        // sanitizer with Go programs. The memory sanitizer only applies to
        // C/C++ code; this permits that code to see the Go runtime's existing signal
        // handlers when registering new signal handlers for the process.

        //go:cgo_import_static x_cgo_sigaction
        //go:linkname x_cgo_sigaction x_cgo_sigaction
        //go:linkname _cgo_sigaction _cgo_sigaction
        private static byte x_cgo_sigaction = default;
        private static var _cgo_sigaction = ref x_cgo_sigaction;
    }
}}
