// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,amd64

// package cgo -- go2cs converted at 2020 August 29 08:21:41 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Go\src\runtime\cgo\mmap.go
// Import "unsafe" because we use go:linkname.
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class cgo_package
    {
        // When using cgo, call the C library for mmap, so that we call into
        // any sanitizer interceptors. This supports using the memory
        // sanitizer with Go programs. The memory sanitizer only applies to
        // C/C++ code; this permits that code to see the Go code as normal
        // program addresses that have been initialized.

        // To support interceptors that look for both mmap and munmap,
        // also call the C library for munmap.

        //go:cgo_import_static x_cgo_mmap
        //go:linkname x_cgo_mmap x_cgo_mmap
        //go:linkname _cgo_mmap _cgo_mmap
        private static byte x_cgo_mmap = default;
        private static var _cgo_mmap = ref x_cgo_mmap;

        //go:cgo_import_static x_cgo_munmap
        //go:linkname x_cgo_munmap x_cgo_munmap
        //go:linkname _cgo_munmap _cgo_munmap
        private static byte x_cgo_munmap = default;
        private static var _cgo_munmap = ref x_cgo_munmap;
    }
}}
