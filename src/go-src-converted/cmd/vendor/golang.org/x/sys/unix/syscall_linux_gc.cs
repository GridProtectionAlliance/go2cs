// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,!gccgo

// package unix -- go2cs converted at 2020 October 09 05:56:48 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_gc.go

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
        // SyscallNoError may be used instead of Syscall for syscalls that don't fail.
        public static (System.UIntPtr, System.UIntPtr) SyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;

        // RawSyscallNoError may be used instead of RawSyscall for syscalls that don't
        // fail.
        public static (System.UIntPtr, System.UIntPtr) RawSyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
    }
}}}}}}
