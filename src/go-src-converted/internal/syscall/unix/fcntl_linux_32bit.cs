// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// On 32-bit Linux systems, use SYS_FCNTL64.
// If you change the build tags here, see syscall/flock_linux_32bit.go.

// +build linux,386 linux,arm linux,mips linux,mipsle

// package unix -- go2cs converted at 2020 October 08 03:31:58 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\fcntl_linux_32bit.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        private static void init()
        {
            FcntlSyscall = syscall.SYS_FCNTL64;
        }
    }
}}}
