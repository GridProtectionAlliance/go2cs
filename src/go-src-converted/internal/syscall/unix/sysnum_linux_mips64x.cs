// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build mips64 mips64le

// package unix -- go2cs converted at 2020 October 08 03:32:05 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\sysnum_linux_mips64x.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        private static readonly System.UIntPtr getrandomTrap = (System.UIntPtr)5313L;
        private static readonly System.UIntPtr copyFileRangeTrap = (System.UIntPtr)5320L;

    }
}}}
