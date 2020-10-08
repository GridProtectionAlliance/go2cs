// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 mips64 mips64le ppc64 ppc64le s390x

// package unix -- go2cs converted at 2020 October 08 03:31:56 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at_sysnum_newfstatat_linux.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        private static readonly System.UIntPtr fstatatTrap = (System.UIntPtr)syscall.SYS_NEWFSTATAT;

    }
}}}
