// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the code to check canonical methods.

// package method -- go2cs converted at 2020 October 09 06:05:09 UTC
// import "cmd/vet/testdata/method" ==> using method = go.cmd.vet.testdata.method_package
// Original source: C:\Go\src\cmd\vet\testdata\method\method.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class method_package
    {
        public partial struct MethodTest // : long
        {
        }

        private static void Scan(this ptr<MethodTest> _addr_t, fmt.ScanState x, byte c)
        {
            ref MethodTest t = ref _addr_t.val;
 // ERROR "should have signature Scan\(fmt\.ScanState, rune\) error"
        }
    }
}}}}
