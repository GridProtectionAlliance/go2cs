// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips64 || mips64le
// +build mips64 mips64le

// package unix -- go2cs converted at 2022 March 13 05:27:49 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\sysnum_linux_mips64x.go
namespace go.@internal.syscall;

public static partial class unix_package {

private static readonly System.UIntPtr getrandomTrap = 5313;
private static readonly System.UIntPtr copyFileRangeTrap = 5320;

} // end unix_package
