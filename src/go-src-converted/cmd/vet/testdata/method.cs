// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the canonical method checker.

// This file contains the code to check canonical methods.

// package testdata -- go2cs converted at 2020 August 29 10:10:33 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\method.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public partial struct MethodTest // : long
        {
        }

        private static void Scan(this ref MethodTest t, fmt.ScanState x, byte c)
        { // ERROR "should have signature Scan"
        }

        public partial interface MethodTestInterface
        {
            byte ReadByte(); // ERROR "should have signature ReadByte"
        }
    }
}}}
