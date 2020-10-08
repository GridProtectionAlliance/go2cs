// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 08 03:45:15 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sticky_bsd.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // According to sticky(8), neither open(2) nor mkdir(2) will create
        // a file with the sticky bit set.
        private static readonly var supportsCreateWithStickyBit = (var)false;

    }
}
