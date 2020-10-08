// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue15517

// package p -- go2cs converted at 2020 October 08 04:55:38 UTC
// import "golang.org/x/tools/go/internal/gcimporter.p" ==> using p = go.golang.org.x.tools.go.@internal.gcimporter.p_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\testdata\p.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class p_package
    {
        public static readonly long C = (long)0L;



        public static long V = default;

        public static void F()
        {
        }
    }
}}}}}}
