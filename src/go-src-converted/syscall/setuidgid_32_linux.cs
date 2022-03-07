// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (386 || arm)
// +build linux
// +build 386 arm

// package syscall -- go2cs converted at 2022 March 06 22:26:46 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\setuidgid_32_linux.go


namespace go;

public static partial class syscall_package {

private static readonly var sys_GETEUID = SYS_GETEUID32;

private static readonly var sys_SETGID = SYS_SETGID32;
private static readonly var sys_SETUID = SYS_SETUID32;

private static readonly var sys_SETREGID = SYS_SETREGID32;
private static readonly var sys_SETREUID = SYS_SETREUID32;

private static readonly var sys_SETRESGID = SYS_SETRESGID32;
private static readonly var sys_SETRESUID = SYS_SETRESUID32;


} // end syscall_package
