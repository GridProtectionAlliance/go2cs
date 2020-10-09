// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:50:54 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at_aix.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        //go:cgo_import_dynamic libc_fstatat fstatat "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_openat openat "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_unlinkat unlinkat "libc.a/shr_64.o"
        public static readonly ulong AT_REMOVEDIR = (ulong)0x1UL;
        public static readonly ulong AT_SYMLINK_NOFOLLOW = (ulong)0x1UL;

    }
}}}
