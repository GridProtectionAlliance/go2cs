// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 && solaris
// +build amd64,solaris

// package syscall -- go2cs converted at 2022 March 06 22:29:48 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\zsysnum_solaris_amd64.go


namespace go;

public static partial class syscall_package {

    // TODO(aram): remove these before Go 1.3.
public static readonly nint SYS_EXECVE = 59;
public static readonly nint SYS_FCNTL = 62;


} // end syscall_package
