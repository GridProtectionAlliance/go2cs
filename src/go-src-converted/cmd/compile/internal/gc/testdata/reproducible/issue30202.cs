// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package p -- go2cs converted at 2020 October 08 04:32:05 UTC
// import "cmd/compile/internal/gc/testdata.p" ==> using p = go.cmd.compile.@internal.gc.testdata.p_package
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\reproducible\issue30202.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal {
namespace gc
{
    public static partial class p_package
    {
        public static long A(object x)
        {
            return x.X();
        }

        public static long B(object x)
        {
            return x.X();
        }
    }
}}}}}
