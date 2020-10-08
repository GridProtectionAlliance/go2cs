// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the buildtag checker.

// +builder // ERROR "possible malformed \+build comment"
// +build !ignore

// package testdata -- go2cs converted at 2020 October 08 04:58:36 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\buildtag\buildtag.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        // +build toolate // ERROR "build comment must appear before package clause and be followed by a blank line$"
        private static long _ = 3L;

        private static @string _ = "\n// +build notacomment\n";
    }
}}}
