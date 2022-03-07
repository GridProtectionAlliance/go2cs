// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64 || riscv64
// +build arm64 riscv64

// package unix -- go2cs converted at 2022 March 06 22:12:52 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_sysnum_fstatat_linux.go
using syscall = go.syscall_package;

namespace go.@internal.syscall;

public static partial class unix_package {

private static readonly System.UIntPtr fstatatTrap = syscall.SYS_FSTATAT;


} // end unix_package
