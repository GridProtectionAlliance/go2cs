// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin linux

// package cgo -- go2cs converted at 2020 October 08 03:24:26 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Go\src\runtime\cgo\callbacks_traceback.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class cgo_package
    { // for go:linkname

        // Calls the traceback function passed to SetCgoTraceback.

        //go:cgo_import_static x_cgo_callers
        //go:linkname x_cgo_callers x_cgo_callers
        //go:linkname _cgo_callers _cgo_callers
        private static byte x_cgo_callers = default;
        private static var _cgo_callers = _addr_x_cgo_callers;
    }
}}
