// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build netbsd

// package cgo -- go2cs converted at 2020 October 08 03:24:26 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Go\src\runtime\cgo\netbsd.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class cgo_package
    { // for go:linkname

        // Supply environ and __progname, because we don't
        // link against the standard NetBSD crt0.o and the
        // libc dynamic library needs them.

        //go:linkname _environ environ
        //go:linkname _progname __progname
        //go:linkname ___ps_strings __ps_strings
        private static System.UIntPtr _environ = default;
        private static System.UIntPtr _progname = default;
        private static System.UIntPtr ___ps_strings = default;
    }
}}
