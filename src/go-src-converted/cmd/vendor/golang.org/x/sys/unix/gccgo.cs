// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gccgo
// +build !aix

// package unix -- go2cs converted at 2020 October 08 04:46:19 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\gccgo.go
using syscall = go.syscall_package;
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
        // We can't use the gc-syntax .s files for gccgo. On the plus side
        // much of the functionality can be written directly in Go.

        //extern gccgoRealSyscallNoError
        private static System.UIntPtr realSyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;

        //extern gccgoRealSyscall
        private static (System.UIntPtr, System.UIntPtr) realSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;

        public static (System.UIntPtr, System.UIntPtr) SyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;

            syscall.Entersyscall();
            var r = realSyscallNoError(trap, a1, a2, a3, 0L, 0L, 0L, 0L, 0L, 0L);
            syscall.Exitsyscall();
            return (r, 0L);
        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            syscall.Errno err = default;

            syscall.Entersyscall();
            var (r, errno) = realSyscall(trap, a1, a2, a3, 0L, 0L, 0L, 0L, 0L, 0L);
            syscall.Exitsyscall();
            return (r, 0L, syscall.Errno(errno));
        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            syscall.Errno err = default;

            syscall.Entersyscall();
            var (r, errno) = realSyscall(trap, a1, a2, a3, a4, a5, a6, 0L, 0L, 0L);
            syscall.Exitsyscall();
            return (r, 0L, syscall.Errno(errno));
        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            syscall.Errno err = default;

            syscall.Entersyscall();
            var (r, errno) = realSyscall(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9);
            syscall.Exitsyscall();
            return (r, 0L, syscall.Errno(errno));
        }

        public static (System.UIntPtr, System.UIntPtr) RawSyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;

            var r = realSyscallNoError(trap, a1, a2, a3, 0L, 0L, 0L, 0L, 0L, 0L);
            return (r, 0L);
        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            syscall.Errno err = default;

            var (r, errno) = realSyscall(trap, a1, a2, a3, 0L, 0L, 0L, 0L, 0L, 0L);
            return (r, 0L, syscall.Errno(errno));
        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            syscall.Errno err = default;

            var (r, errno) = realSyscall(trap, a1, a2, a3, a4, a5, a6, 0L, 0L, 0L);
            return (r, 0L, syscall.Errno(errno));
        }
    }
}}}}}}
