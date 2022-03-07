// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !android && (386 || amd64 || arm || mips || mipsle || mips64 || mips64le || ppc64 || ppc64le || s390x)
// +build !android
// +build 386 amd64 arm mips mipsle mips64 mips64le ppc64 ppc64le s390x

// package syscall -- go2cs converted at 2022 March 06 22:26:55 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_dup2_linux.go


namespace go;

public static partial class syscall_package {

private static readonly var _SYS_dup = SYS_DUP2;


} // end syscall_package
