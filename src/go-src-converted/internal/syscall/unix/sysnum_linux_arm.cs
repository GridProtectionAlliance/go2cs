// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:51:00 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\sysnum_linux_arm.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        private static readonly System.UIntPtr getrandomTrap = (System.UIntPtr)384L;
        private static readonly System.UIntPtr copyFileRangeTrap = (System.UIntPtr)391L;

    }
}}}
