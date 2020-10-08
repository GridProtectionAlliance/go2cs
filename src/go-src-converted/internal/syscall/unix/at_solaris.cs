// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 08 03:31:51 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at_solaris.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        // Implemented as sysvicall6 in runtime/syscall_solaris.go.
        private static (System.UIntPtr, System.UIntPtr, syscall.Errno) syscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        //go:cgo_import_dynamic libc_fstatat fstatat "libc.so"
        //go:cgo_import_dynamic libc_openat openat "libc.so"
        //go:cgo_import_dynamic libc_unlinkat unlinkat "libc.so"

        public static readonly ulong AT_REMOVEDIR = (ulong)0x1UL;
        public static readonly ulong AT_SYMLINK_NOFOLLOW = (ulong)0x1000UL;

    }
}}}
