// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:29 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\dropm_stub.go
namespace go;

using _@unsafe_ = @unsafe_package;

public static partial class main_package { // for go:linkname

// Defined in the runtime package.
//go:linkname runtime_getm_for_test runtime.getm
private static System.UIntPtr runtime_getm_for_test();

} // end main_package
