// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Minimal copy of x/sys/unix so the cpu package can make a
// system call on AIX without depending on x/sys/unix.
// (See golang.org/issue/32102)

// +build aix,ppc64
// +build !gccgo

// package cpu -- go2cs converted at 2020 October 09 06:07:55 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\syscall_aix_ppc64_gc.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        //go:cgo_import_dynamic libc_getsystemcfg getsystemcfg "libc.a/shr_64.o"

        //go:linkname libc_getsystemcfg libc_getsystemcfg
        private partial struct syscallFunc // : System.UIntPtr
        {
        }

        private static syscallFunc libc_getsystemcfg = default;

        private partial struct errno // : syscall.Errno
        {
        }

        // Implemented in runtime/syscall_aix.go.
        private static (System.UIntPtr, System.UIntPtr, errno) rawSyscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, errno) syscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        private static (System.UIntPtr, errno) callgetsystemcfg(long label)
        {
            System.UIntPtr r1 = default;
            errno e1 = default;

            r1, _, e1 = syscall6(uintptr(@unsafe.Pointer(_addr_libc_getsystemcfg)), 1L, uintptr(label), 0L, 0L, 0L, 0L, 0L);
            return ;
        }
    }
}}}}}
