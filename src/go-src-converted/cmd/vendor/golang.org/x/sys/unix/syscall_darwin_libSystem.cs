// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,go1.12

// package unix -- go2cs converted at 2020 October 08 04:46:59 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin_libSystem.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // Implemented in the runtime package (runtime/sys_darwin.go)
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall9(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
; // 32-bit only
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_rawSyscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_rawSyscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscallPtr(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;

        //go:linkname syscall_syscall syscall.syscall
        //go:linkname syscall_syscall6 syscall.syscall6
        //go:linkname syscall_syscall6X syscall.syscall6X
        //go:linkname syscall_syscall9 syscall.syscall9
        //go:linkname syscall_rawSyscall syscall.rawSyscall
        //go:linkname syscall_rawSyscall6 syscall.rawSyscall6
        //go:linkname syscall_syscallPtr syscall.syscallPtr

        // Find the entry point for f. See comments in runtime/proc.go for the
        // function of the same name.
        //go:nosplit
        private static System.UIntPtr funcPC(Action f)
        {
            return new ptr<ptr<ptr<ptr<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_f));
        }
    }
}}}}}}
