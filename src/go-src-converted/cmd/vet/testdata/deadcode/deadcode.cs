// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the dead code checker.

// package deadcode -- go2cs converted at 2020 October 08 04:58:36 UTC
// import "cmd/vet/testdata/deadcode" ==> using deadcode = go.cmd.vet.testdata.deadcode_package
// Original source: C:\Go\src\cmd\vet\testdata\deadcode\deadcode.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class deadcode_package
    {
        private static long _()
        {
            print(1L);
            return 2L;
            println(); // ERROR "unreachable code"
            return 3L;

        }
    }
}}}}
