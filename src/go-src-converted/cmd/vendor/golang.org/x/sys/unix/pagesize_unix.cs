// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// For Unix, get the pagesize from the runtime.

// package unix -- go2cs converted at 2022 March 06 23:26:37 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\pagesize_unix.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

public static nint Getpagesize() {
    return syscall.Getpagesize();
}

} // end unix_package
