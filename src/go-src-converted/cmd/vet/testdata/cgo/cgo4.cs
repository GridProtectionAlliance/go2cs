// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test the cgo checker on a file that doesn't use cgo, but has an
// import named "C".

// package testdata -- go2cs converted at 2020 August 29 10:10:41 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\cgo\cgo4.go
using C = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        private static var _ = C.Println(new ptr<ref p>(p.Value.Value));

        // Passing a pointer (via a slice), but C is fmt, not cgo.
        private static var _ = C.Println(new slice<long>(new long[] { 3 }));
    }
}}}
