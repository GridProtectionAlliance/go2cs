// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package a -- go2cs converted at 2020 October 09 06:03:55 UTC
// import "golang.org/x/tools/go/analysis/passes/cgocall/testdata/src/a" ==> using a = go.golang.org.x.tools.go.analysis.passes.cgocall.testdata.src.a_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\cgocall\testdata\src\a\cgo3.go
// The purpose of this inherited test is unclear.

using C = go.C_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace cgocall {
namespace testdata {
namespace src
{
    public static partial class a_package
    {
        private static readonly long x = (long)1L;



        private static long a = 1L;        private static long b = 2L;



        public static void F()
        {
        }

        public static bool FAD(long _p0, @string _p0)
        {
            C.malloc(3L);
            return true;
        }
    }
}}}}}}}}}}
