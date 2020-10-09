// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux
// +build !darwin
// +build !dragonfly
// +build !freebsd
// +build !netbsd
// +build !openbsd !arm64
// +build !solaris

// package runtime -- go2cs converted at 2020 October 09 04:45:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\auxv_none.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv)
        {
            ref ptr<byte> argv = ref _addr_argv.val;

        }
    }
}
