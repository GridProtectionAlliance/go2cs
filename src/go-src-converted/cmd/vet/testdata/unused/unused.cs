// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the unusedresult checker.

// package unused -- go2cs converted at 2022 March 13 06:43:18 UTC
// import "cmd/vet/testdata/unused" ==> using unused = go.cmd.vet.testdata.unused_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\unused\unused.go
namespace go.cmd.vet.testdata;

using fmt = fmt_package;

public static partial class unused_package {

private static void _() {
    fmt.Errorf(""); // ERROR "result of fmt.Errorf call not used"
}

} // end unused_package
