// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build arm64 riscv64

// package unix -- go2cs converted at 2020 October 09 04:51:00 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\sysnum_linux_generic.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        // This file is named "generic" because at a certain point Linux started
        // standardizing on system call numbers across architectures. So far this
        // means only arm64 and riscv64 use the standard numbers.
        private static readonly System.UIntPtr getrandomTrap = (System.UIntPtr)278L;
        private static readonly System.UIntPtr copyFileRangeTrap = (System.UIntPtr)285L;

    }
}}}
