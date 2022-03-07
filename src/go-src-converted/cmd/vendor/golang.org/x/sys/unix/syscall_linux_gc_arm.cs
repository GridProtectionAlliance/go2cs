// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm && gc && linux
// +build arm,gc,linux

// package unix -- go2cs converted at 2022 March 06 23:27:08 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_gc_arm.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Underlying system call writes to newoffset via pointer.
    // Implemented in assembly to avoid allocation.
private static (long, syscall.Errno) seek(nint fd, long offset, nint whence);

} // end unix_package
