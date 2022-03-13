// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris) && gc && !ppc64le && !ppc64
// +build darwin dragonfly freebsd linux netbsd openbsd solaris
// +build gc
// +build !ppc64le
// +build !ppc64

// package unix -- go2cs converted at 2022 March 13 06:41:25 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_unix_gc.go
namespace go.cmd.vendor.golang.org.x.sys;

using syscall = syscall_package;

public static partial class unix_package {

public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
public static (System.UIntPtr, System.UIntPtr, syscall.Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, syscall.Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);

} // end unix_package
