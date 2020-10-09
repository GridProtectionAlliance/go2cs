// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:50:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at_sysnum_darwin.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        public static readonly ulong AT_REMOVEDIR = (ulong)0x80UL;

        public static readonly ulong AT_SYMLINK_NOFOLLOW = (ulong)0x0020UL;

    }
}}}
