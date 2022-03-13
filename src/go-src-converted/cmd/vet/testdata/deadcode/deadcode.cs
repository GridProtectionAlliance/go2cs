// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the dead code checker.

// package deadcode -- go2cs converted at 2022 March 13 06:42:52 UTC
// import "cmd/vet/testdata/deadcode" ==> using deadcode = go.cmd.vet.testdata.deadcode_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\deadcode\deadcode.go
namespace go.cmd.vet.testdata;

public static partial class deadcode_package {

private static nint _() {
    print(1);
    return 2;
    println(); // ERROR "unreachable code"
    return 3;
}

} // end deadcode_package
