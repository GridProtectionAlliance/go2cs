// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:11 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux_s390x.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var _SYS_dup = SYS_DUP2;
        private static readonly var _SYS_setgroups = SYS_SETGROUPS;

        //sys    Dup2(oldfd int, newfd int) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatfs(fd int, buf *Statfs_t) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sysnb    Getegid() (egid int)
        //sysnb    Geteuid() (euid int)
        //sysnb    Getgid() (gid int)
        //sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = SYS_GETRLIMIT
        //sysnb    Getuid() (uid int)
        //sysnb    InotifyInit() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
        //sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
        //sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
        //sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
        //sys    Setfsgid(gid int) (err error)
        //sys    Setfsuid(uid int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setresgid(rgid int, egid int, sgid int) (err error)
        //sysnb    Setresuid(ruid int, euid int, suid int) (err error)
        //sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Statfs(path string, buf *Statfs_t) (err error)
        //sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error) = SYS_SYNC_FILE_RANGE
        //sys    Truncate(path string, length int64) (err error)
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
        //sysnb    setgroups(n int, list *_Gid_t) (err error)

        //sysnb    Gettimeofday(tv *Timeval) (err error)

        public static (Time_t, error) Time(ref Time_t t)
        {
            Timeval tv = default;
            err = Gettimeofday(ref tv);
            if (err != null)
            {
                return (0L, err);
            }
            if (t != null)
            {
                t.Value = Time_t(tv.Sec);
            }
            return (Time_t(tv.Sec), null);
        }

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:usec);
        }

        public static error Pipe(slice<long> p)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            array<_C_int> pp = new array<_C_int>(2L);
            err = pipe2(ref pp, 0L);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return;
        }

        //sysnb pipe2(p *[2]_C_int, flags int) (err error)

        public static error Pipe2(slice<long> p, long flags)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            array<_C_int> pp = new array<_C_int>(2L);
            err = pipe2(ref pp, flags);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return;
        }

        // Linux on s390x uses the old mmap interface, which requires arguments to be passed in a struct.
        // mmap2 also requires arguments to be passed in a struct; it is currently not exposed in <asm/unistd.h>.
        private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, long prot, long flags, long fd, long offset)
        {
            array<System.UIntPtr> mmap_args = new array<System.UIntPtr>(new System.UIntPtr[] { addr, length, uintptr(prot), uintptr(flags), uintptr(fd), uintptr(offset) });
            var (r0, _, e1) = Syscall(SYS_MMAP, uintptr(@unsafe.Pointer(ref mmap_args[0L])), 0L, 0L);
            xaddr = uintptr(r0);
            if (e1 != 0L)
            {
                err = errnoErr(e1);
            }
            return;
        }

        // On s390x Linux, all the socket calls go through an extra indirection.
        // The arguments to the underlying system call are the number below
        // and a pointer to an array of uintptr.  We hide the pointer in the
        // socketcall assembly to avoid allocation on every system call.

 
        // see linux/net.h
        private static readonly long _SOCKET = 1L;
        private static readonly long _BIND = 2L;
        private static readonly long _CONNECT = 3L;
        private static readonly long _LISTEN = 4L;
        private static readonly long _ACCEPT = 5L;
        private static readonly long _GETSOCKNAME = 6L;
        private static readonly long _GETPEERNAME = 7L;
        private static readonly long _SOCKETPAIR = 8L;
        private static readonly long _SEND = 9L;
        private static readonly long _RECV = 10L;
        private static readonly long _SENDTO = 11L;
        private static readonly long _RECVFROM = 12L;
        private static readonly long _SHUTDOWN = 13L;
        private static readonly long _SETSOCKOPT = 14L;
        private static readonly long _GETSOCKOPT = 15L;
        private static readonly long _SENDMSG = 16L;
        private static readonly long _RECVMSG = 17L;
        private static readonly long _ACCEPT4 = 18L;
        private static readonly long _RECVMMSG = 19L;
        private static readonly long _SENDMMSG = 20L;

        private static (long, Errno) socketcall(long call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
;
        private static (long, Errno) rawsocketcall(long call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
;

        private static (long, error) accept(long s, ref RawSockaddrAny rsa, ref _Socklen addrlen)
        {
            var (fd, e) = socketcall(_ACCEPT, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0L, 0L, 0L);
            if (e != 0L)
            {>>MARKER:FUNCTION_rawsocketcall_BLOCK_PREFIX<<
                err = e;
            }
            return;
        }

        private static (long, error) accept4(long s, ref RawSockaddrAny rsa, ref _Socklen addrlen, long flags)
        {
            var (fd, e) = socketcall(_ACCEPT4, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), uintptr(flags), 0L, 0L);
            if (e != 0L)
            {>>MARKER:FUNCTION_socketcall_BLOCK_PREFIX<<
                err = e;
            }
            return;
        }

        private static error getsockname(long s, ref RawSockaddrAny rsa, ref _Socklen addrlen)
        {
            var (_, e) = rawsocketcall(_GETSOCKNAME, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error getpeername(long s, ref RawSockaddrAny rsa, ref _Socklen addrlen)
        {
            var (_, e) = rawsocketcall(_GETPEERNAME, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error socketpair(long domain, long typ, long flags, ref array<int> fd)
        {
            var (_, e) = rawsocketcall(_SOCKETPAIR, uintptr(domain), uintptr(typ), uintptr(flags), uintptr(@unsafe.Pointer(fd)), 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error bind(long s, unsafe.Pointer addr, _Socklen addrlen)
        {
            var (_, e) = socketcall(_BIND, uintptr(s), uintptr(addr), uintptr(addrlen), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error connect(long s, unsafe.Pointer addr, _Socklen addrlen)
        {
            var (_, e) = socketcall(_CONNECT, uintptr(s), uintptr(addr), uintptr(addrlen), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static (long, error) socket(long domain, long typ, long proto)
        {
            var (fd, e) = rawsocketcall(_SOCKET, uintptr(domain), uintptr(typ), uintptr(proto), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error getsockopt(long s, long level, long name, unsafe.Pointer val, ref _Socklen vallen)
        {
            var (_, e) = socketcall(_GETSOCKOPT, uintptr(s), uintptr(level), uintptr(name), uintptr(val), uintptr(@unsafe.Pointer(vallen)), 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error setsockopt(long s, long level, long name, unsafe.Pointer val, System.UIntPtr vallen)
        {
            var (_, e) = socketcall(_SETSOCKOPT, uintptr(s), uintptr(level), uintptr(name), uintptr(val), vallen, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static (long, error) recvfrom(long s, slice<byte> p, long flags, ref RawSockaddrAny from, ref _Socklen fromlen)
        {
            System.UIntPtr @base = default;
            if (len(p) > 0L)
            {
                base = uintptr(@unsafe.Pointer(ref p[0L]));
            }
            var (n, e) = socketcall(_RECVFROM, uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(@unsafe.Pointer(from)), uintptr(@unsafe.Pointer(fromlen)));
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static error sendto(long s, slice<byte> p, long flags, unsafe.Pointer to, _Socklen addrlen)
        {
            System.UIntPtr @base = default;
            if (len(p) > 0L)
            {
                base = uintptr(@unsafe.Pointer(ref p[0L]));
            }
            var (_, e) = socketcall(_SENDTO, uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(to), uintptr(addrlen));
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static (long, error) recvmsg(long s, ref Msghdr msg, long flags)
        {
            var (n, e) = socketcall(_RECVMSG, uintptr(s), uintptr(@unsafe.Pointer(msg)), uintptr(flags), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static (long, error) sendmsg(long s, ref Msghdr msg, long flags)
        {
            var (n, e) = socketcall(_SENDMSG, uintptr(s), uintptr(@unsafe.Pointer(msg)), uintptr(flags), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        public static error Listen(long s, long n)
        {
            var (_, e) = socketcall(_LISTEN, uintptr(s), uintptr(n), 0L, 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        public static error Shutdown(long s, long how)
        {
            var (_, e) = socketcall(_SHUTDOWN, uintptr(s), uintptr(how), 0L, 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static ulong PC(this ref PtraceRegs r)
        {
            return r.Psw.Addr;
        }

        private static void SetPC(this ref PtraceRegs r, ulong pc)
        {
            r.Psw.Addr = pc;

        }

        private static void SetLen(this ref Iovec iov, long length)
        {
            iov.Len = uint64(length);
        }

        private static void SetControllen(this ref Msghdr msghdr, long length)
        {
            msghdr.Controllen = uint64(length);
        }

        private static void SetLen(this ref Cmsghdr cmsg, long length)
        {
            cmsg.Len = uint64(length);
        }

        private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1) => func((_, panic, __) =>
        {
            panic("not implemented");
        });
    }
}
