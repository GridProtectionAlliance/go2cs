// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,!amd64,!arm64 freebsd,!amd64

// package runtime -- go2cs converted at 2020 October 08 03:22:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sigaction.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // This version is used on Linux and FreeBSD systems on which we don't
        // use cgo to call the C version of sigaction.

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaction(uint sig, ptr<sigactiont> _addr_@new, ptr<sigactiont> _addr_old)
        {
            ref sigactiont @new = ref _addr_@new.val;
            ref sigactiont old = ref _addr_old.val;

            sysSigaction(sig, new, old);
        }
    }
}
