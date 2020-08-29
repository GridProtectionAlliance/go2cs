// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue15517

// package p -- go2cs converted at 2020 August 29 10:09:14 UTC
// import "go/internal/gcimporter.p" ==> using p = go.go.@internal.gcimporter.p_package
// Original source: C:\Go\src\go\internal\gcimporter\testdata\p.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class p_package
    {
        public static readonly long C = 0L;



        public static long V = default;

        public static void F()
        {
        }
    }
}}}
