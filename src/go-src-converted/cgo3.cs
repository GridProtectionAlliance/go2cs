// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package a -- go2cs converted at 2022 March 06 23:33:53 UTC
// import "golang.org/x/tools/go/analysis/passes/cgocall/testdata/src/a" ==> using a = go.golang.org.x.tools.go.analysis.passes.cgocall.testdata.src.a_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\cgocall\testdata\src\a\cgo3.go
// The purpose of this inherited test is unclear.

using C = go.C_package;

namespace go.golang.org.x.tools.go.analysis.passes.cgocall.testdata.src;

public static partial class a_package {

private static readonly nint x = 1;



private static nint a = 1;private static nint b = 2;



public static void F() {
}

public static bool FAD(nint _p0, @string _p0) {
    C.malloc(3);
    return true;
}

} // end a_package
