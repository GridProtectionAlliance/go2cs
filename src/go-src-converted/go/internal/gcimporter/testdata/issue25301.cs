// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue25301 -- go2cs converted at 2020 October 08 04:56:12 UTC
// import "go/internal/gcimporter.issue25301" ==> using issue25301 = go.go.@internal.gcimporter.issue25301_package
// Original source: C:\Go\src\go\internal\gcimporter\testdata\issue25301.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class issue25301_package
    {
        public partial interface A
        {
            void M();
        }
        public partial interface T : A
        {
        }
        public partial struct S
        {
        }
        public static void M(this S _p0)
        {
            println("m");
        }
    }
}}}
