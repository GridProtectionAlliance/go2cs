// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test the cgo checker on a file that doesn't use cgo.

// package c -- go2cs converted at 2022 March 06 23:33:53 UTC
// import "golang.org/x/tools/go/analysis/passes/cgocall/testdata/src/c" ==> using c = go.golang.org.x.tools.go.analysis.passes.cgocall.testdata.src.c_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\cgocall\testdata\src\c\c.go
using @unsafe = go.@unsafe_package;
using System;


namespace go.golang.org.x.tools.go.analysis.passes.cgocall.testdata.src;

public static partial class c_package {

    // Passing a pointer (via the slice), but C isn't cgo.
private static var _ = C.f(@unsafe.Pointer(@new<int>()));

public static var C = default;

} // end c_package
