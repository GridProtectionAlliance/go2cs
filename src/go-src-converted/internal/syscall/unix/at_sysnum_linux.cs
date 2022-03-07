// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:53 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_sysnum_linux.go
using syscall = go.syscall_package;

namespace go.@internal.syscall;

public static partial class unix_package {

private static readonly System.UIntPtr unlinkatTrap = syscall.SYS_UNLINKAT;

private static readonly System.UIntPtr openatTrap = syscall.SYS_OPENAT;



public static readonly nuint AT_REMOVEDIR = 0x200;

public static readonly nuint AT_SYMLINK_NOFOLLOW = 0x100;


} // end unix_package
