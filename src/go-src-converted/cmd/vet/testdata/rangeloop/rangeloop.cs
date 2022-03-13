// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the rangeloop checker.

// package rangeloop -- go2cs converted at 2022 March 13 06:43:18 UTC
// import "cmd/vet/testdata/rangeloop" ==> using rangeloop = go.cmd.vet.testdata.rangeloop_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\rangeloop\rangeloop.go
namespace go.cmd.vet.testdata;

using System;
using System.Threading;
public static partial class rangeloop_package {

public static void RangeLoopTests() {
    slice<nint> s = default;
    foreach (var (i, v) in s) {
        go_(() => () => {
            println(i); // ERROR "loop variable i captured by func literal"
            println(v); // ERROR "loop variable v captured by func literal"
        }());
    }
}

} // end rangeloop_package
