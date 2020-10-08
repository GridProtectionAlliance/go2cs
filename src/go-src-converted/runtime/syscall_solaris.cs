// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:23:54 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\syscall_solaris.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static libcFunc libc_chdir = default;        private static libcFunc libc_chroot = default;        private static libcFunc libc_close = default;        private static libcFunc libc_execve = default;        private static libcFunc libc_fcntl = default;        private static libcFunc libc_forkx = default;        private static libcFunc libc_gethostname = default;        private static libcFunc libc_getpid = default;        private static libcFunc libc_ioctl = default;        private static libcFunc libc_setgid = default;        private static libcFunc libc_setgroups = default;        private static libcFunc libc_setsid = default;        private static libcFunc libc_setuid = default;        private static libcFunc libc_setpgid = default;        private static libcFunc libc_syscall = default;        private static libcFunc libc_wait4 = default;


        //go:linkname pipe1x runtime.pipe1
        private static libcFunc pipe1x = default; // name to take addr of pipe1

        private static void pipe1()
; // declared for vet; do NOT call

        // Many of these are exported via linkname to assembly in the syscall
        // package.

        //go:nosplit
        //go:linkname syscall_sysvicall6
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_sysvicall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:fn,n:nargs,args:uintptr(unsafe.Pointer(&a1)),), out ptr<libcall> _addr_call);
            entersyscallblock();
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            exitsyscall();
            return (call.r1, call.r2, call.err);
        }

        //go:nosplit
        //go:linkname syscall_rawsysvicall6
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawsysvicall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:fn,n:nargs,args:uintptr(unsafe.Pointer(&a1)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return (call.r1, call.r2, call.err);
        }

        // TODO(aram): Once we remove all instances of C calling sysvicallN, make
        // sysvicallN return errors and replace the body of the following functions
        // with calls to sysvicallN.

        //go:nosplit
        //go:linkname syscall_chdir
        private static System.UIntPtr syscall_chdir(System.UIntPtr path)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_chdir)),n:1,args:uintptr(unsafe.Pointer(&path)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        //go:nosplit
        //go:linkname syscall_chroot
        private static System.UIntPtr syscall_chroot(System.UIntPtr path)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_chroot)),n:1,args:uintptr(unsafe.Pointer(&path)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        // like close, but must not split stack, for forkx.
        //go:nosplit
        //go:linkname syscall_close
        private static int syscall_close(int fd)
        {
            return int32(sysvicall1(_addr_libc_close, uintptr(fd)));
        }

        private static readonly ulong _F_DUP2FD = (ulong)0x9UL;

        //go:nosplit
        //go:linkname syscall_dup2


        //go:nosplit
        //go:linkname syscall_dup2
        private static (System.UIntPtr, System.UIntPtr) syscall_dup2(System.UIntPtr oldfd, System.UIntPtr newfd)
        {
            System.UIntPtr val = default;
            System.UIntPtr err = default;

            return syscall_fcntl(oldfd, _F_DUP2FD, newfd);
        }

        //go:nosplit
        //go:linkname syscall_execve
        private static System.UIntPtr syscall_execve(System.UIntPtr path, System.UIntPtr argv, System.UIntPtr envp)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_execve)),n:3,args:uintptr(unsafe.Pointer(&path)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        // like exit, but must not split stack, for forkx.
        //go:nosplit
        //go:linkname syscall_exit
        private static void syscall_exit(System.UIntPtr code)
        {
            sysvicall1(_addr_libc_exit, code);
        }

        //go:nosplit
        //go:linkname syscall_fcntl
        private static (System.UIntPtr, System.UIntPtr) syscall_fcntl(System.UIntPtr fd, System.UIntPtr cmd, System.UIntPtr arg)
        {
            System.UIntPtr val = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_fcntl)),n:3,args:uintptr(unsafe.Pointer(&fd)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return (call.r1, call.err);
        }

        //go:nosplit
        //go:linkname syscall_forkx
        private static (System.UIntPtr, System.UIntPtr) syscall_forkx(System.UIntPtr flags)
        {
            System.UIntPtr pid = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_forkx)),n:1,args:uintptr(unsafe.Pointer(&flags)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            if (int(call.r1) != -1L)
            {>>MARKER:FUNCTION_pipe1_BLOCK_PREFIX<<
                call.err = 0L;
            }

            return (call.r1, call.err);

        }

        //go:linkname syscall_gethostname
        private static (@string, System.UIntPtr) syscall_gethostname()
        {
            @string name = default;
            System.UIntPtr err = default;

            ptr<var> cname = @new<[_MAXHOSTNAMELEN]byte>();
            array<System.UIntPtr> args = new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(unsafe.Pointer(&cname[0])), _MAXHOSTNAMELEN });
            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_gethostname)),n:2,args:uintptr(unsafe.Pointer(&args[0])),), out ptr<libcall> _addr_call);
            entersyscallblock();
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            exitsyscall();
            if (call.r1 != 0L)
            {
                return ("", call.err);
            }

            cname[_MAXHOSTNAMELEN - 1L] = 0L;
            return (gostringnocopy(_addr_cname[0L]), 0L);

        }

        //go:nosplit
        //go:linkname syscall_getpid
        private static (System.UIntPtr, System.UIntPtr) syscall_getpid()
        {
            System.UIntPtr pid = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_getpid)),n:0,args:uintptr(unsafe.Pointer(&libc_getpid)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return (call.r1, call.err);
        }

        //go:nosplit
        //go:linkname syscall_ioctl
        private static System.UIntPtr syscall_ioctl(System.UIntPtr fd, System.UIntPtr req, System.UIntPtr arg)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_ioctl)),n:3,args:uintptr(unsafe.Pointer(&fd)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        //go:linkname syscall_pipe
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_pipe()
        {
            System.UIntPtr r = default;
            System.UIntPtr w = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&pipe1x)),n:0,args:uintptr(unsafe.Pointer(&pipe1x)),), out ptr<libcall> _addr_call);
            entersyscallblock();
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            exitsyscall();
            return (call.r1, call.r2, call.err);
        }

        // This is syscall.RawSyscall, it exists to satisfy some build dependency,
        // but it doesn't work.
        //
        //go:linkname syscall_rawsyscall
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawsyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) => func((_, panic, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            panic("RawSyscall not available on Solaris");
        });

        // This is syscall.RawSyscall6, it exists to avoid a linker error because
        // syscall.RawSyscall6 is already declared. See golang.org/issue/24357
        //
        //go:linkname syscall_rawsyscall6
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawsyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) => func((_, panic, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            panic("RawSyscall6 not available on Solaris");
        });

        //go:nosplit
        //go:linkname syscall_setgid
        private static System.UIntPtr syscall_setgid(System.UIntPtr gid)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_setgid)),n:1,args:uintptr(unsafe.Pointer(&gid)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        //go:nosplit
        //go:linkname syscall_setgroups
        private static System.UIntPtr syscall_setgroups(System.UIntPtr ngid, System.UIntPtr gid)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_setgroups)),n:2,args:uintptr(unsafe.Pointer(&ngid)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        //go:nosplit
        //go:linkname syscall_setsid
        private static (System.UIntPtr, System.UIntPtr) syscall_setsid()
        {
            System.UIntPtr pid = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_setsid)),n:0,args:uintptr(unsafe.Pointer(&libc_setsid)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return (call.r1, call.err);
        }

        //go:nosplit
        //go:linkname syscall_setuid
        private static System.UIntPtr syscall_setuid(System.UIntPtr uid)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_setuid)),n:1,args:uintptr(unsafe.Pointer(&uid)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        //go:nosplit
        //go:linkname syscall_setpgid
        private static System.UIntPtr syscall_setpgid(System.UIntPtr pid, System.UIntPtr pgid)
        {
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_setpgid)),n:2,args:uintptr(unsafe.Pointer(&pid)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return call.err;
        }

        //go:linkname syscall_syscall
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_syscall)),n:4,args:uintptr(unsafe.Pointer(&trap)),), out ptr<libcall> _addr_call);
            entersyscallblock();
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            exitsyscall();
            return (call.r1, call.r2, call.err);
        }

        //go:linkname syscall_wait4
        private static (long, System.UIntPtr) syscall_wait4(System.UIntPtr pid, ptr<uint> _addr_wstatus, System.UIntPtr options, unsafe.Pointer rusage)
        {
            long wpid = default;
            System.UIntPtr err = default;
            ref uint wstatus = ref _addr_wstatus.val;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_wait4)),n:4,args:uintptr(unsafe.Pointer(&pid)),), out ptr<libcall> _addr_call);
            entersyscallblock();
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            exitsyscall();
            return (int(call.r1), call.err);
        }

        //go:nosplit
        //go:linkname syscall_write
        private static (System.UIntPtr, System.UIntPtr) syscall_write(System.UIntPtr fd, System.UIntPtr buf, System.UIntPtr nbyte)
        {
            System.UIntPtr n = default;
            System.UIntPtr err = default;

            ref libcall call = ref heap(new libcall(fn:uintptr(unsafe.Pointer(&libc_write)),n:3,args:uintptr(unsafe.Pointer(&fd)),), out ptr<libcall> _addr_call);
            asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_call));
            return (call.r1, call.err);
        }
    }
}
