// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test the cgo checker on a file that doesn't use cgo.

// package c -- go2cs converted at 2020 October 09 06:03:55 UTC
// import "golang.org/x/tools/go/analysis/passes/cgocall/testdata/src/c" ==> using c = go.golang.org.x.tools.go.analysis.passes.cgocall.testdata.src.c_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\cgocall\testdata\src\c\c.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

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
    public static partial class c_package
    {
        // Passing a pointer (via the slice), but C isn't cgo.
        private static var _ = C.f(@unsafe.Pointer(@new<int>()));

        public static var C = default;
    }
}}}}}}}}}}
