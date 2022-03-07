// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && gc && 386
// +build linux,gc,386

// package unix -- go2cs converted at 2022 March 06 23:27:07 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_gc_386.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Underlying system call writes to newoffset via pointer.
    // Implemented in assembly to avoid allocation.
private static (long, syscall.Errno) seek(nint fd, long offset, nint whence);

private static (nint, syscall.Errno) socketcall(nint call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5);
private static (nint, syscall.Errno) rawsocketcall(nint call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5);

} // end unix_package
