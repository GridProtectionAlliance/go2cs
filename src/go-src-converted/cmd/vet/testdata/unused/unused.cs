// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the unusedresult checker.

// package unused -- go2cs converted at 2020 October 08 04:58:39 UTC
// import "cmd/vet/testdata/unused" ==> using unused = go.cmd.vet.testdata.unused_package
// Original source: C:\Go\src\cmd\vet\testdata\unused\unused.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class unused_package
    {
        private static void _()
        {
            fmt.Errorf(""); // ERROR "result of fmt.Errorf call not used"
        }
    }
}}}}
