// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:50 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\syscall_aix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // This file handles some syscalls from the syscall package
        // Especially, syscalls use during forkAndExecInChild which must not split the stack

        //go:cgo_import_dynamic libc_chdir chdir "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_chroot chroot "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_dup2 dup2 "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_execve execve "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_fcntl fcntl "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_fork fork "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_ioctl ioctl "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_setgid setgid "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_setgroups setgroups "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_setsid setsid "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_setuid setuid "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_setpgid setpgid "libc.a/shr_64.o"

        //go:linkname libc_chdir libc_chdir
        //go:linkname libc_chroot libc_chroot
        //go:linkname libc_dup2 libc_dup2
        //go:linkname libc_execve libc_execve
        //go:linkname libc_fcntl libc_fcntl
        //go:linkname libc_fork libc_fork
        //go:linkname libc_ioctl libc_ioctl
        //go:linkname libc_setgid libc_setgid
        //go:linkname libc_setgroups libc_setgroups
        //go:linkname libc_setsid libc_setsid
        //go:linkname libc_setuid libc_setuid
        //go:linkname libc_setpgid libc_setpgid
        private static libFunc libc_chdir = default;        private static libFunc libc_chroot = default;        private static libFunc libc_dup2 = default;        private static libFunc libc_execve = default;        private static libFunc libc_fcntl = default;        private static libFunc libc_fork = default;        private static libFunc libc_ioctl = default;        private static libFunc libc_setgid = default;        private static libFunc libc_setgroups = default;        private static libFunc libc_setsid = default;        private static libFunc libc_setuid = default;        private static libFunc libc_setpgid = default;


        // In syscall_syscall6 and syscall_rawsyscall6, r2 is always 0
        // as it's never used on AIX
        // TODO: remove r2 from zsyscall_aix_$GOARCH.go

        // Syscall is needed because some packages (like net) need it too.
        // The best way is to return EINVAL and let Golang handles its failure
        // If the syscall can't fail, this function can redirect it to a real syscall.
        //
        // This is exported via linkname to assembly in the syscall package.
        //
        //go:nosplit
        //go:linkname syscall_Syscall
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            return (0L, 0L, _EINVAL);
        }

        // This is syscall.RawSyscall, it exists to satisfy some build dependency,
        // but it doesn't work.
        //
        // This is exported via linkname to assembly in the syscall package.
        //
        //go:linkname syscall_RawSyscall
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) => func((_, panic, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            panic("RawSyscall not available on AIX");
        });

        // This is exported via linkname to assembly in the syscall package.
        //
        //go:nosplit
        //go:cgo_unsafe_args
        //go:linkname syscall_syscall6
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            ref libcall c = ref heap(new libcall(fn:fn,n:nargs,args:uintptr(unsafe.Pointer(&a1)),), out ptr<libcall> _addr_c);

            entersyscallblock();
            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));
            exitsyscall();
            return (c.r1, 0L, c.err);
        }

        // This is exported via linkname to assembly in the syscall package.
        //
        //go:nosplit
        //go:cgo_unsafe_args
        //go:linkname syscall_rawSyscall6
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            ref libcall c = ref heap(new libcall(fn:fn,n:nargs,args:uintptr(unsafe.Pointer(&a1)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            return (c.r1, 0L, c.err);
        }

        //go:linkname syscall_chdir syscall.chdir
        //go:nosplit
        private static System.UIntPtr syscall_chdir(System.UIntPtr path)
        {
            System.UIntPtr err = default;

            _, err = syscall1(_addr_libc_chdir, path);
            return ;
        }

        //go:linkname syscall_chroot1 syscall.chroot1
        //go:nosplit
        private static System.UIntPtr syscall_chroot1(System.UIntPtr path)
        {
            System.UIntPtr err = default;

            _, err = syscall1(_addr_libc_chroot, path);
            return ;
        }

        // like close, but must not split stack, for fork.
        //go:linkname syscall_close syscall.close
        //go:nosplit
        private static int syscall_close(int fd)
        {
            var (_, err) = syscall1(_addr_libc_close, uintptr(fd));
            return int32(err);
        }

        //go:linkname syscall_dup2child syscall.dup2child
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_dup2child(System.UIntPtr old, System.UIntPtr @new)
        {
            System.UIntPtr val = default;
            System.UIntPtr err = default;

            val, err = syscall2(_addr_libc_dup2, old, new);
            return ;
        }

        //go:linkname syscall_execve syscall.execve
        //go:nosplit
        private static System.UIntPtr syscall_execve(System.UIntPtr path, System.UIntPtr argv, System.UIntPtr envp)
        {
            System.UIntPtr err = default;

            _, err = syscall3(_addr_libc_execve, path, argv, envp);
            return ;
        }

        // like exit, but must not split stack, for fork.
        //go:linkname syscall_exit syscall.exit
        //go:nosplit
        private static void syscall_exit(System.UIntPtr code)
        {
            syscall1(_addr_libc_exit, code);
        }

        //go:linkname syscall_fcntl1 syscall.fcntl1
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_fcntl1(System.UIntPtr fd, System.UIntPtr cmd, System.UIntPtr arg)
        {
            System.UIntPtr val = default;
            System.UIntPtr err = default;

            val, err = syscall3(_addr_libc_fcntl, fd, cmd, arg);
            return ;
        }

        //go:linkname syscall_forkx syscall.forkx
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_forkx(System.UIntPtr flags)
        {
            System.UIntPtr pid = default;
            System.UIntPtr err = default;

            pid, err = syscall1(_addr_libc_fork, flags);
            return ;
        }

        //go:linkname syscall_getpid syscall.getpid
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_getpid()
        {
            System.UIntPtr pid = default;
            System.UIntPtr err = default;

            pid, err = syscall0(_addr_libc_getpid);
            return ;
        }

        //go:linkname syscall_ioctl syscall.ioctl
        //go:nosplit
        private static System.UIntPtr syscall_ioctl(System.UIntPtr fd, System.UIntPtr req, System.UIntPtr arg)
        {
            System.UIntPtr err = default;

            _, err = syscall3(_addr_libc_ioctl, fd, req, arg);
            return ;
        }

        //go:linkname syscall_setgid syscall.setgid
        //go:nosplit
        private static System.UIntPtr syscall_setgid(System.UIntPtr gid)
        {
            System.UIntPtr err = default;

            _, err = syscall1(_addr_libc_setgid, gid);
            return ;
        }

        //go:linkname syscall_setgroups1 syscall.setgroups1
        //go:nosplit
        private static System.UIntPtr syscall_setgroups1(System.UIntPtr ngid, System.UIntPtr gid)
        {
            System.UIntPtr err = default;

            _, err = syscall2(_addr_libc_setgroups, ngid, gid);
            return ;
        }

        //go:linkname syscall_setsid syscall.setsid
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_setsid()
        {
            System.UIntPtr pid = default;
            System.UIntPtr err = default;

            pid, err = syscall0(_addr_libc_setsid);
            return ;
        }

        //go:linkname syscall_setuid syscall.setuid
        //go:nosplit
        private static System.UIntPtr syscall_setuid(System.UIntPtr uid)
        {
            System.UIntPtr err = default;

            _, err = syscall1(_addr_libc_setuid, uid);
            return ;
        }

        //go:linkname syscall_setpgid syscall.setpgid
        //go:nosplit
        private static System.UIntPtr syscall_setpgid(System.UIntPtr pid, System.UIntPtr pgid)
        {
            System.UIntPtr err = default;

            _, err = syscall2(_addr_libc_setpgid, pid, pgid);
            return ;
        }

        //go:linkname syscall_write1 syscall.write1
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_write1(System.UIntPtr fd, System.UIntPtr buf, System.UIntPtr nbyte)
        {
            System.UIntPtr n = default;
            System.UIntPtr err = default;

            n, err = syscall3(_addr_libc_write, fd, buf, nbyte);
            return ;
        }
    }
}
