// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nointerface -- go2cs converted at 2020 October 08 04:56:22 UTC
// import "go/internal/gccgoimporter.nointerface" ==> using nointerface = go.go.@internal.gccgoimporter.nointerface_package
// Original source: C:\Go\src\go\internal\gccgoimporter\testdata\nointerface.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class nointerface_package
    {
        public partial struct I // : long
        {
        }

        //go:nointerface
        private static long Get(this ptr<I> _addr_p)
        {
            ref I p = ref _addr_p.val;

            return int(p.val);
        }

        private static void Set(this ptr<I> _addr_p, long v)
        {
            ref I p = ref _addr_p.val;

            p.val = I(v);
        }
    }
}}}
