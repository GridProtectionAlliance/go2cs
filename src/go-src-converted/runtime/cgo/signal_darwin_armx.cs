// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin
// +build arm arm64

// package cgo -- go2cs converted at 2020 August 29 08:21:41 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Go\src\runtime\cgo\signal_darwin_armx.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class cgo_package
    {
        //go:cgo_export_static xx_cgo_panicmem xx_cgo_panicmem
        private static void xx_cgo_panicmem()
;
    }
}}
