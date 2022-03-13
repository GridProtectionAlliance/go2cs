// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lostcancel -- go2cs converted at 2022 March 13 06:42:52 UTC
// import "cmd/vet/testdata/lostcancel" ==> using lostcancel = go.cmd.vet.testdata.lostcancel_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\lostcancel\lostcancel.go
namespace go.cmd.vet.testdata;

using context = context_package;

public static partial class lostcancel_package {

private static void _() {
 // ERROR "the cancel function is not used on all paths \(possible context leak\)"
    if (false) {
        _ = cancel;
    }
} // ERROR "this return statement may be reached without using the cancel var defined on line 10"

} // end lostcancel_package
