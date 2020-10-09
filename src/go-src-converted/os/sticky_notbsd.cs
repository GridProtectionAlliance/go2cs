// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !aix
// +build !darwin
// +build !dragonfly
// +build !freebsd
// +build !js !wasm
// +build !netbsd
// +build !openbsd
// +build !solaris

// package os -- go2cs converted at 2020 October 09 05:07:27 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sticky_notbsd.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static readonly var supportsCreateWithStickyBit = true;

    }
}
