// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:21:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\syscall_solaris.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static libcFunc libc_chdir = default;        private static libcFunc libc_chroot = default;        private static libcFunc libc_execve = default;        private static libcFunc libc_fcntl = default;        private static libcFunc libc_forkx = default;        private static libcFunc libc_gethostname = default;        private static libcFunc libc_getpid = default;        private static libcFunc libc_ioctl = default;        private static libcFunc libc_pipe = default;        private static libcFunc libc_setgid = default;        private static libcFunc libc_setgroups = default;        private static libcFunc libc_setsid = default;        private static libcFunc libc_setuid = default;        private static libcFunc libc_setpgid = default;        private static libcFunc libc_syscall = default;        private static libcFunc libc_wait4 = default;        private static libcFunc pipe1 = default;

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_sysvicall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            libcall call = new libcall(fn:fn,n:nargs,args:uintptr(unsafe.Pointer(&a1)),);
            entersyscallblock(0L);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            exitsyscall(0L);
            return (call.r1, call.r2, call.err);
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawsysvicall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            libcall call = new libcall(fn:fn,n:nargs,args:uintptr(unsafe.Pointer(&a1)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return (call.r1, call.r2, call.err);
        }

        // TODO(aram): Once we remove all instances of C calling sysvicallN, make
        // sysvicallN return errors and replace the body of the following functions
        // with calls to sysvicallN.

        //go:nosplit
        private static System.UIntPtr syscall_chdir(System.UIntPtr path)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_chdir)),n:1,args:uintptr(unsafe.Pointer(&path)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        //go:nosplit
        private static System.UIntPtr syscall_chroot(System.UIntPtr path)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_chroot)),n:1,args:uintptr(unsafe.Pointer(&path)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        // like close, but must not split stack, for forkx.
        //go:nosplit
        private static int syscall_close(int fd)
        {
            return int32(sysvicall1(ref libc_close, uintptr(fd)));
        }

        //go:nosplit
        private static System.UIntPtr syscall_execve(System.UIntPtr path, System.UIntPtr argv, System.UIntPtr envp)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_execve)),n:3,args:uintptr(unsafe.Pointer(&path)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        // like exit, but must not split stack, for forkx.
        //go:nosplit
        private static void syscall_exit(System.UIntPtr code)
        {
            sysvicall1(ref libc_exit, code);
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_fcntl(System.UIntPtr fd, System.UIntPtr cmd, System.UIntPtr arg)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_fcntl)),n:3,args:uintptr(unsafe.Pointer(&fd)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return (call.r1, call.err);
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_forkx(System.UIntPtr flags)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_forkx)),n:1,args:uintptr(unsafe.Pointer(&flags)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return (call.r1, call.err);
        }

        private static (@string, System.UIntPtr) syscall_gethostname()
        {
            var cname = @new<array<byte>>();
            array<System.UIntPtr> args = new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(unsafe.Pointer(&cname[0])), _MAXHOSTNAMELEN });
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_gethostname)),n:2,args:uintptr(unsafe.Pointer(&args[0])),);
            entersyscallblock(0L);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            exitsyscall(0L);
            if (call.r1 != 0L)
            {
                return ("", call.err);
            }
            cname[_MAXHOSTNAMELEN - 1L] = 0L;
            return (gostringnocopy(ref cname[0L]), 0L);
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_getpid()
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_getpid)),n:0,args:uintptr(unsafe.Pointer(&libc_getpid)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return (call.r1, call.err);
        }

        //go:nosplit
        private static System.UIntPtr syscall_ioctl(System.UIntPtr fd, System.UIntPtr req, System.UIntPtr arg)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_ioctl)),n:3,args:uintptr(unsafe.Pointer(&fd)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_pipe()
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&pipe1)),n:0,args:uintptr(unsafe.Pointer(&pipe1)),);
            entersyscallblock(0L);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            exitsyscall(0L);
            return (call.r1, call.r2, call.err);
        }

        // This is syscall.RawSyscall, it exists to satisfy some build dependency,
        // but it doesn't work.
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawsyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) => func((_, panic, __) =>
        {
            panic("RawSyscall not available on Solaris");
        });

        //go:nosplit
        private static System.UIntPtr syscall_setgid(System.UIntPtr gid)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_setgid)),n:1,args:uintptr(unsafe.Pointer(&gid)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        //go:nosplit
        private static System.UIntPtr syscall_setgroups(System.UIntPtr ngid, System.UIntPtr gid)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_setgroups)),n:2,args:uintptr(unsafe.Pointer(&ngid)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_setsid()
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_setsid)),n:0,args:uintptr(unsafe.Pointer(&libc_setsid)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return (call.r1, call.err);
        }

        //go:nosplit
        private static System.UIntPtr syscall_setuid(System.UIntPtr uid)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_setuid)),n:1,args:uintptr(unsafe.Pointer(&uid)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        //go:nosplit
        private static System.UIntPtr syscall_setpgid(System.UIntPtr pid, System.UIntPtr pgid)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_setpgid)),n:2,args:uintptr(unsafe.Pointer(&pid)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return call.err;
        }

        // This is syscall.Syscall, it exists to satisfy some build dependency,
        // but it doesn't work correctly.
        //
        // DO NOT USE!
        //
        // TODO(aram): make this panic once we stop calling fcntl(2) in net using it.
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_syscall)),n:4,args:uintptr(unsafe.Pointer(&trap)),);
            entersyscallblock(0L);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            exitsyscall(0L);
            return (call.r1, call.r2, call.err);
        }

        private static (long, System.UIntPtr) syscall_wait4(System.UIntPtr pid, ref uint wstatus, System.UIntPtr options, unsafe.Pointer rusage)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_wait4)),n:4,args:uintptr(unsafe.Pointer(&pid)),);
            entersyscallblock(0L);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            exitsyscall(0L);
            return (int(call.r1), call.err);
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_write(System.UIntPtr fd, System.UIntPtr buf, System.UIntPtr nbyte)
        {
            libcall call = new libcall(fn:uintptr(unsafe.Pointer(&libc_write)),n:3,args:uintptr(unsafe.Pointer(&fd)),);
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref call));
            return (call.r1, call.err);
        }
    }
}
