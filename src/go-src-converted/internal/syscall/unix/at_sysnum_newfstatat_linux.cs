// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || mips64 || mips64le || ppc64 || ppc64le || s390x
// +build amd64 mips64 mips64le ppc64 ppc64le s390x

// package unix -- go2cs converted at 2022 March 06 22:12:53 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_sysnum_newfstatat_linux.go
using syscall = go.syscall_package;

namespace go.@internal.syscall;

public static partial class unix_package {

private static readonly System.UIntPtr fstatatTrap = syscall.SYS_NEWFSTATAT;


} // end unix_package
