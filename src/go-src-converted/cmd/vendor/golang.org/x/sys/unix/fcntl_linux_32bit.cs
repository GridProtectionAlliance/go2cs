// +build linux,386 linux,arm linux,mips linux,mipsle

// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 05:56:15 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\fcntl_linux_32bit.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        private static void init()
        { 
            // On 32-bit Linux systems, the fcntl syscall that matches Go's
            // Flock_t type is SYS_FCNTL64, not SYS_FCNTL.
            fcntl64Syscall = SYS_FCNTL64;

        }
    }
}}}}}}
