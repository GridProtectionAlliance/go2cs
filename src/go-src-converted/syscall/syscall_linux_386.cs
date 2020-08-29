// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO(rsc): Rewrite all nn(SP) references into name+(nn-8)(FP)
// so that go vet can check that they are correct.

// package syscall -- go2cs converted at 2020 August 29 08:38:04 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux_386.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var _SYS_dup = SYS_DUP2;
        private static readonly var _SYS_setgroups = SYS_SETGROUPS32;

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:int32(sec),Nsec:int32(nsec));
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:int32(sec),Usec:int32(usec));
        }

        //sysnb    pipe(p *[2]_C_int) (err error)

        public static error Pipe(slice<long> p)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            array<_C_int> pp = new array<_C_int>(2L);
            err = pipe(ref pp);
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

        // 64-bit file system and 32-bit uid calls
        // (386 default is 32-bit file system and 16-bit uid).
        //sys    Dup2(oldfd int, newfd int) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error) = SYS_FCHOWN32
        //sys    Fstat(fd int, stat *Stat_t) (err error) = SYS_FSTAT64
        //sys    Ftruncate(fd int, length int64) (err error) = SYS_FTRUNCATE64
        //sysnb    Getegid() (egid int) = SYS_GETEGID32
        //sysnb    Geteuid() (euid int) = SYS_GETEUID32
        //sysnb    Getgid() (gid int) = SYS_GETGID32
        //sysnb    Getuid() (uid int) = SYS_GETUID32
        //sysnb    InotifyInit() (fd int, err error)
        //sys    Ioperm(from int, num int, on int) (err error)
        //sys    Iopl(level int) (err error)
        //sys    Lchown(path string, uid int, gid int) (err error) = SYS_LCHOWN32
        //sys    Lstat(path string, stat *Stat_t) (err error) = SYS_LSTAT64
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
        //sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error) = SYS_SENDFILE64
        //sys    Setfsgid(gid int) (err error) = SYS_SETFSGID32
        //sys    Setfsuid(uid int) (err error) = SYS_SETFSUID32
        //sysnb    Setregid(rgid int, egid int) (err error) = SYS_SETREGID32
        //sysnb    Setresgid(rgid int, egid int, sgid int) (err error) = SYS_SETRESGID32
        //sysnb    Setresuid(ruid int, euid int, suid int) (err error) = SYS_SETRESUID32
        //sysnb    Setreuid(ruid int, euid int) (err error) = SYS_SETREUID32
        //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int, err error)
        //sys    Stat(path string, stat *Stat_t) (err error) = SYS_STAT64
        //sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error)
        //sys    Truncate(path string, length int64) (err error) = SYS_TRUNCATE64
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error) = SYS_GETGROUPS32
        //sysnb    setgroups(n int, list *_Gid_t) (err error) = SYS_SETGROUPS32
        //sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error) = SYS__NEWSELECT

        //sys    mmap2(addr uintptr, length uintptr, prot int, flags int, fd int, pageOffset uintptr) (xaddr uintptr, err error)

        private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, long prot, long flags, long fd, long offset)
        {
            var page = uintptr(offset / 4096L);
            if (offset != int64(page) * 4096L)
            {
                return (0L, EINVAL);
            }
            return mmap2(addr, length, prot, flags, fd, page);
        }

        private partial struct rlimit32
        {
            public uint Cur;
            public uint Max;
        }

        //sysnb getrlimit(resource int, rlim *rlimit32) (err error) = SYS_GETRLIMIT

        private static readonly var rlimInf32 = ~uint32(0L);

        private static readonly var rlimInf64 = ~uint64(0L);



        public static error Getrlimit(long resource, ref Rlimit rlim)
        {
            err = prlimit(0L, resource, null, rlim);
            if (err != ENOSYS)
            {
                return error.As(err);
            }
            rlimit32 rl = new rlimit32();
            err = getrlimit(resource, ref rl);
            if (err != null)
            {
                return;
            }
            if (rl.Cur == rlimInf32)
            {
                rlim.Cur = rlimInf64;
            }
            else
            {
                rlim.Cur = uint64(rl.Cur);
            }
            if (rl.Max == rlimInf32)
            {
                rlim.Max = rlimInf64;
            }
            else
            {
                rlim.Max = uint64(rl.Max);
            }
            return;
        }

        //sysnb setrlimit(resource int, rlim *rlimit32) (err error) = SYS_SETRLIMIT

        public static error Setrlimit(long resource, ref Rlimit rlim)
        {
            err = prlimit(0L, resource, rlim, null);
            if (err != ENOSYS)
            {
                return error.As(err);
            }
            rlimit32 rl = new rlimit32();
            if (rlim.Cur == rlimInf64)
            {
                rl.Cur = rlimInf32;
            }
            else if (rlim.Cur < uint64(rlimInf32))
            {
                rl.Cur = uint32(rlim.Cur);
            }
            else
            {
                return error.As(EINVAL);
            }
            if (rlim.Max == rlimInf64)
            {
                rl.Max = rlimInf32;
            }
            else if (rlim.Max < uint64(rlimInf32))
            {
                rl.Max = uint32(rlim.Max);
            }
            else
            {
                return error.As(EINVAL);
            }
            return error.As(setrlimit(resource, ref rl));
        }

        // Underlying system call writes to newoffset via pointer.
        // Implemented in assembly to avoid allocation.
        private static (long, Errno) seek(long fd, long offset, long whence)
;

        public static (long, error) Seek(long fd, long offset, long whence)
        {
            var (newoffset, errno) = seek(fd, offset, whence);
            if (errno != 0L)
            {>>MARKER:FUNCTION_seek_BLOCK_PREFIX<<
                return (0L, errno);
            }
            return (newoffset, null);
        }

        // Vsyscalls on amd64.
        //sysnb    Gettimeofday(tv *Timeval) (err error)
        //sysnb    Time(t *Time_t) (tt Time_t, err error)

        // On x86 Linux, all the socket calls go through an extra indirection,
        // I think because the 5-register system call interface can't handle
        // the 6-argument calls like sendto and recvfrom. Instead the
        // arguments to the underlying system call are the number below
        // and a pointer to an array of uintptr. We hide the pointer in the
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

        public static error Fstatfs(long fd, ref Statfs_t buf)
        {
            var (_, _, e) = Syscall(SYS_FSTATFS64, uintptr(fd), @unsafe.Sizeof(buf.Value), uintptr(@unsafe.Pointer(buf)));
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        public static error Statfs(@string path, ref Statfs_t buf)
        {
            var (pathp, err) = BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            var (_, _, e) = Syscall(SYS_STATFS64, uintptr(@unsafe.Pointer(pathp)), @unsafe.Sizeof(buf.Value), uintptr(@unsafe.Pointer(buf)));
            if (e != 0L)
            {
                err = e;
            }
            return;
        }

        private static ulong PC(this ref PtraceRegs r)
        {
            return uint64(uint32(r.Eip));
        }

        private static void SetPC(this ref PtraceRegs r, ulong pc)
        {
            r.Eip = int32(pc);

        }

        private static void SetLen(this ref Iovec iov, long length)
        {
            iov.Len = uint32(length);
        }

        private static void SetControllen(this ref Msghdr msghdr, long length)
        {
            msghdr.Controllen = uint32(length);
        }

        private static void SetLen(this ref Cmsghdr cmsg, long length)
        {
            cmsg.Len = uint32(length);
        }

        private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1) => func((_, panic, __) =>
        {
            panic("not implemented");
        });
    }
}
