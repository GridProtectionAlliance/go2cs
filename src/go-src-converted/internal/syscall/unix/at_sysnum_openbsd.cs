// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:50:56 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at_sysnum_openbsd.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        private static readonly System.UIntPtr unlinkatTrap = (System.UIntPtr)syscall.SYS_UNLINKAT;

        private static readonly System.UIntPtr openatTrap = (System.UIntPtr)syscall.SYS_OPENAT;

        private static readonly System.UIntPtr fstatatTrap = (System.UIntPtr)syscall.SYS_FSTATAT;



        public static readonly ulong AT_REMOVEDIR = (ulong)0x08UL;

        public static readonly ulong AT_SYMLINK_NOFOLLOW = (ulong)0x02UL;

    }
}}}
