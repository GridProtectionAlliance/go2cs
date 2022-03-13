// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the bool checker.

// package @bool -- go2cs converted at 2022 March 13 06:42:52 UTC
// import "cmd/vet/testdata/bool" ==> using @bool = go.cmd.vet.testdata.@bool_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\bool\bool.go
namespace go.cmd.vet.testdata;

using System;
public static partial class @bool_package {

private static void _() {
    Func<nint> f = default;    Func<nint> g = default;



    {
        var v = f();
        var w = g();

        if (v == w || v == w) { // ERROR "redundant or: v == w || v == w"
        }
    }
}

} // end @bool_package
