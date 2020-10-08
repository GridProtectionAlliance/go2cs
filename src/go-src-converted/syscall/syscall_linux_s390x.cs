// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 08 03:27:36 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux_s390x.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var _SYS_setgroups = (var)SYS_SETGROUPS;

        //sys    Dup2(oldfd int, newfd int) (err error)
        //sysnb    EpollCreate(size int) (fd int, err error)
        //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_NEWFSTATAT
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
        //sys    Pause() (err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
        //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
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
        //sys    Ustat(dev int, ubuf *Ustat_t) (err error)
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
        //sysnb    setgroups(n int, list *_Gid_t) (err error)

        //sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
        //sysnb    Gettimeofday(tv *Timeval) (err error)



        //sys    Dup2(oldfd int, newfd int) (err error)
        //sysnb    EpollCreate(size int) (fd int, err error)
        //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_NEWFSTATAT
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
        //sys    Pause() (err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
        //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
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
        //sys    Ustat(dev int, ubuf *Ustat_t) (err error)
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
        //sysnb    setgroups(n int, list *_Gid_t) (err error)

        //sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
        //sysnb    Gettimeofday(tv *Timeval) (err error)

        public static (Time_t, error) Time(ptr<Time_t> _addr_t)
        {
            Time_t tt = default;
            error err = default!;
            ref Time_t t = ref _addr_t.val;

            ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
            err = Gettimeofday(_addr_tv);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if (t != null)
            {
                t = Time_t(tv.Sec);
            }

            return (Time_t(tv.Sec), error.As(null!)!);

        }

        //sys    Utime(path string, buf *Utimbuf) (err error)
        //sys    utimes(path string, times *[2]Timeval) (err error)

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
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            err = pipe2(_addr_pp, 0L);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

        }

        //sysnb pipe2(p *[2]_C_int, flags int) (err error)

        public static error Pipe2(slice<long> p, long flags)
        {
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            err = pipe2(_addr_pp, flags);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

        }

        // Linux on s390x uses the old mmap interface, which requires arguments to be passed in a struct.
        // mmap2 also requires arguments to be passed in a struct; it is currently not exposed in <asm/unistd.h>.
        private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, long prot, long flags, long fd, long offset)
        {
            System.UIntPtr xaddr = default;
            error err = default!;

            array<System.UIntPtr> mmap_args = new array<System.UIntPtr>(new System.UIntPtr[] { addr, length, uintptr(prot), uintptr(flags), uintptr(fd), uintptr(offset) });
            var (r0, _, e1) = Syscall(SYS_MMAP, uintptr(@unsafe.Pointer(_addr_mmap_args[0L])), 0L, 0L);
            xaddr = uintptr(r0);
            if (e1 != 0L)
            {
                err = errnoErr(e1);
            }

            return ;

        }

        // On s390x Linux, all the socket calls go through an extra indirection.
        // The arguments to the underlying system call are the number below
        // and a pointer to an array of uintptr.  We hide the pointer in the
        // socketcall assembly to avoid allocation on every system call.

 
        // see linux/net.h
        private static readonly long _SOCKET = (long)1L;
        private static readonly long _BIND = (long)2L;
        private static readonly long _CONNECT = (long)3L;
        private static readonly long _LISTEN = (long)4L;
        private static readonly long _ACCEPT = (long)5L;
        private static readonly long _GETSOCKNAME = (long)6L;
        private static readonly long _GETPEERNAME = (long)7L;
        private static readonly long _SOCKETPAIR = (long)8L;
        private static readonly long _SEND = (long)9L;
        private static readonly long _RECV = (long)10L;
        private static readonly long _SENDTO = (long)11L;
        private static readonly long _RECVFROM = (long)12L;
        private static readonly long _SHUTDOWN = (long)13L;
        private static readonly long _SETSOCKOPT = (long)14L;
        private static readonly long _GETSOCKOPT = (long)15L;
        private static readonly long _SENDMSG = (long)16L;
        private static readonly long _RECVMSG = (long)17L;
        private static readonly long _ACCEPT4 = (long)18L;
        private static readonly long _RECVMMSG = (long)19L;
        private static readonly long _SENDMMSG = (long)20L;


        private static (long, Errno) socketcall(long call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
;
        private static (long, Errno) rawsocketcall(long call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
;

        private static (long, error) accept(long s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen)
        {
            long fd = default;
            error err = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;
            ref _Socklen addrlen = ref _addr_addrlen.val;

            var (fd, e) = socketcall(_ACCEPT, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0L, 0L, 0L);
            if (e != 0L)
            {>>MARKER:FUNCTION_rawsocketcall_BLOCK_PREFIX<<
                err = e;
            }

            return ;

        }

        private static (long, error) accept4(long s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen, long flags)
        {
            long fd = default;
            error err = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;
            ref _Socklen addrlen = ref _addr_addrlen.val;

            var (fd, e) = socketcall(_ACCEPT4, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), uintptr(flags), 0L, 0L);
            if (e != 0L)
            {>>MARKER:FUNCTION_socketcall_BLOCK_PREFIX<<
                err = e;
            }

            return ;

        }

        private static error getsockname(long s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen)
        {
            error err = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;
            ref _Socklen addrlen = ref _addr_addrlen.val;

            var (_, e) = rawsocketcall(_GETSOCKNAME, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error getpeername(long s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen)
        {
            error err = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;
            ref _Socklen addrlen = ref _addr_addrlen.val;

            var (_, e) = rawsocketcall(_GETPEERNAME, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error socketpair(long domain, long typ, long flags, ptr<array<int>> _addr_fd)
        {
            error err = default!;
            ref array<int> fd = ref _addr_fd.val;

            var (_, e) = rawsocketcall(_SOCKETPAIR, uintptr(domain), uintptr(typ), uintptr(flags), uintptr(@unsafe.Pointer(fd)), 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error bind(long s, unsafe.Pointer addr, _Socklen addrlen)
        {
            error err = default!;

            var (_, e) = socketcall(_BIND, uintptr(s), uintptr(addr), uintptr(addrlen), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error connect(long s, unsafe.Pointer addr, _Socklen addrlen)
        {
            error err = default!;

            var (_, e) = socketcall(_CONNECT, uintptr(s), uintptr(addr), uintptr(addrlen), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static (long, error) socket(long domain, long typ, long proto)
        {
            long fd = default;
            error err = default!;

            var (fd, e) = rawsocketcall(_SOCKET, uintptr(domain), uintptr(typ), uintptr(proto), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error getsockopt(long s, long level, long name, unsafe.Pointer val, ptr<_Socklen> _addr_vallen)
        {
            error err = default!;
            ref _Socklen vallen = ref _addr_vallen.val;

            var (_, e) = socketcall(_GETSOCKOPT, uintptr(s), uintptr(level), uintptr(name), uintptr(val), uintptr(@unsafe.Pointer(vallen)), 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error setsockopt(long s, long level, long name, unsafe.Pointer val, System.UIntPtr vallen)
        {
            error err = default!;

            var (_, e) = socketcall(_SETSOCKOPT, uintptr(s), uintptr(level), uintptr(name), uintptr(val), vallen, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static (long, error) recvfrom(long s, slice<byte> p, long flags, ptr<RawSockaddrAny> _addr_from, ptr<_Socklen> _addr_fromlen)
        {
            long n = default;
            error err = default!;
            ref RawSockaddrAny from = ref _addr_from.val;
            ref _Socklen fromlen = ref _addr_fromlen.val;

            System.UIntPtr @base = default;
            if (len(p) > 0L)
            {
                base = uintptr(@unsafe.Pointer(_addr_p[0L]));
            }

            var (n, e) = socketcall(_RECVFROM, uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(@unsafe.Pointer(from)), uintptr(@unsafe.Pointer(fromlen)));
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static error sendto(long s, slice<byte> p, long flags, unsafe.Pointer to, _Socklen addrlen)
        {
            error err = default!;

            System.UIntPtr @base = default;
            if (len(p) > 0L)
            {
                base = uintptr(@unsafe.Pointer(_addr_p[0L]));
            }

            var (_, e) = socketcall(_SENDTO, uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(to), uintptr(addrlen));
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static (long, error) recvmsg(long s, ptr<Msghdr> _addr_msg, long flags)
        {
            long n = default;
            error err = default!;
            ref Msghdr msg = ref _addr_msg.val;

            var (n, e) = socketcall(_RECVMSG, uintptr(s), uintptr(@unsafe.Pointer(msg)), uintptr(flags), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static (long, error) sendmsg(long s, ptr<Msghdr> _addr_msg, long flags)
        {
            long n = default;
            error err = default!;
            ref Msghdr msg = ref _addr_msg.val;

            var (n, e) = socketcall(_SENDMSG, uintptr(s), uintptr(@unsafe.Pointer(msg)), uintptr(flags), 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        public static error Listen(long s, long n)
        {
            error err = default!;

            var (_, e) = socketcall(_LISTEN, uintptr(s), uintptr(n), 0L, 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        public static error Shutdown(long s, long how)
        {
            error err = default!;

            var (_, e) = socketcall(_SHUTDOWN, uintptr(s), uintptr(how), 0L, 0L, 0L, 0L);
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static ulong PC(this ptr<PtraceRegs> _addr_r)
        {
            ref PtraceRegs r = ref _addr_r.val;

            return r.Psw.Addr;
        }

        private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc)
        {
            ref PtraceRegs r = ref _addr_r.val;

            r.Psw.Addr = pc;
        }

        private static void SetLen(this ptr<Iovec> _addr_iov, long length)
        {
            ref Iovec iov = ref _addr_iov.val;

            iov.Len = uint64(length);
        }

        private static void SetControllen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Controllen = uint64(length);
        }

        private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, long length)
        {
            ref Cmsghdr cmsg = ref _addr_cmsg.val;

            cmsg.Len = uint64(length);
        }

        private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1)
;
    }
}
