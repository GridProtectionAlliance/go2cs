// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package p -- go2cs converted at 2020 October 08 04:56:12 UTC
// import "go/internal/gcimporter.p" ==> using p = go.go.@internal.gcimporter.p_package
// Original source: C:\Go\src\go\internal\gcimporter\testdata\issue15920.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class p_package
    {
        // The underlying type of Error is the underlying type of error.
        // Make sure we can import this again without problems.
        public partial struct Error // : error
        {
        }

        public static Error F()
        {
            return null;
        }
    }
}}}
