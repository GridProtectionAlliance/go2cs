// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build openbsd

// package cgo -- go2cs converted at 2020 October 09 04:49:12 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Go\src\runtime\cgo\openbsd.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class cgo_package
    { // for go:linkname

        // Supply __guard_local because we don't link against the standard
        // OpenBSD crt0.o and the libc dynamic library needs it.

        //go:linkname _guard_local __guard_local
        private static System.UIntPtr _guard_local = default;

        // This is normally marked as hidden and placed in the
        // .openbsd.randomdata section.
        //go:cgo_export_dynamic __guard_local __guard_local
    }
}}
