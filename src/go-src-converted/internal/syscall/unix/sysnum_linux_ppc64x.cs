// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build ppc64 || ppc64le
// +build ppc64 ppc64le

// package unix -- go2cs converted at 2022 March 06 22:12:56 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\sysnum_linux_ppc64x.go


namespace go.@internal.syscall;

public static partial class unix_package {

private static readonly System.UIntPtr getrandomTrap = 359;
private static readonly System.UIntPtr copyFileRangeTrap = 379;


} // end unix_package
