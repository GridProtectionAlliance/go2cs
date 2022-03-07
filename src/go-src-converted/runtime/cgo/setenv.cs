// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package cgo -- go2cs converted at 2022 March 06 22:12:30 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\setenv.go
using _@unsafe_ = go.@unsafe_package;

namespace go.runtime;

public static partial class cgo_package {
 // for go:linkname

    //go:cgo_import_static x_cgo_setenv
    //go:linkname x_cgo_setenv x_cgo_setenv
    //go:linkname _cgo_setenv runtime._cgo_setenv
private static byte x_cgo_setenv = default;
private static var _cgo_setenv = _addr_x_cgo_setenv;

//go:cgo_import_static x_cgo_unsetenv
//go:linkname x_cgo_unsetenv x_cgo_unsetenv
//go:linkname _cgo_unsetenv runtime._cgo_unsetenv
private static byte x_cgo_unsetenv = default;
private static var _cgo_unsetenv = _addr_x_cgo_unsetenv;

} // end cgo_package
