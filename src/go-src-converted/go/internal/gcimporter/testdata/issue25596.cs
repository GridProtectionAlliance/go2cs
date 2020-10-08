// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue25596 -- go2cs converted at 2020 October 08 04:56:12 UTC
// import "go/internal/gcimporter.issue25596" ==> using issue25596 = go.go.@internal.gcimporter.issue25596_package
// Original source: C:\Go\src\go\internal\gcimporter\testdata\issue25596.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class issue25596_package
    {
        public partial interface E
        {
            T M();
        }

        public partial interface T : E
        {
        }
    }
}}}
