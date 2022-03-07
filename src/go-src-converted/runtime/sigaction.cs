// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (linux && !amd64 && !arm64 && !ppc64le) || (freebsd && !amd64)
// +build linux,!amd64,!arm64,!ppc64le freebsd,!amd64

// package runtime -- go2cs converted at 2022 March 06 22:11:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sigaction.go


namespace go;

public static partial class runtime_package {

    // This version is used on Linux and FreeBSD systems on which we don't
    // use cgo to call the C version of sigaction.

    //go:nosplit
    //go:nowritebarrierrec
private static void sigaction(uint sig, ptr<sigactiont> _addr_@new, ptr<sigactiont> _addr_old) {
    ref sigactiont @new = ref _addr_@new.val;
    ref sigactiont old = ref _addr_old.val;

    sysSigaction(sig, new, old);
}

} // end runtime_package
