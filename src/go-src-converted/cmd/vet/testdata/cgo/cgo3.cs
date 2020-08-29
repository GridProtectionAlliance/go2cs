// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Used by TestVetVerbose to test that vet -v doesn't fail because it
// can't find "C".

// package testdata -- go2cs converted at 2020 August 29 10:10:41 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\cgo\cgo3.go
using C = go.C_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void F()
        {
        }
    }
}}}
