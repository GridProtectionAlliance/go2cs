// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm,linux

// package unix -- go2cs converted at 2020 October 08 04:47:29 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_arm.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
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
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp); 
            // Try pipe2 first for Android O, then try pipe for kernel 2.6.23.
            err = pipe2(_addr_pp, 0L);
            if (err == ENOSYS)
            {
                err = pipe(_addr_pp);
            }

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

        // Underlying system call writes to newoffset via pointer.
        // Implemented in assembly to avoid allocation.
        private static (long, syscall.Errno) seek(long fd, long offset, long whence)
;

        public static (long, error) Seek(long fd, long offset, long whence)
        {
            long newoffset = default;
            error err = default!;

            var (newoffset, errno) = seek(fd, offset, whence);
            if (errno != 0L)
            {>>MARKER:FUNCTION_seek_BLOCK_PREFIX<<
                return (0L, error.As(errno)!);
            }

            return (newoffset, error.As(null!)!);

        }

        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)
        //sys    accept4(s int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (fd int, err error)
        //sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error) = SYS_GETGROUPS32
        //sysnb    setgroups(n int, list *_Gid_t) (err error) = SYS_SETGROUPS32
        //sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
        //sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
        //sysnb    socket(domain int, typ int, proto int) (fd int, err error)
        //sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sysnb    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
        //sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
        //sysnb    socketpair(domain int, typ int, flags int, fd *[2]int32) (err error)
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error)
        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error)

        // 64-bit file system and 32-bit uid calls
        // (16-bit uid calls are not always supported in newer kernels)
        //sys    dup2(oldfd int, newfd int) (err error)
        //sysnb    EpollCreate(size int) (fd int, err error)
        //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
        //sys    Fchown(fd int, uid int, gid int) (err error) = SYS_FCHOWN32
        //sys    Fstat(fd int, stat *Stat_t) (err error) = SYS_FSTAT64
        //sys    Fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_FSTATAT64
        //sysnb    Getegid() (egid int) = SYS_GETEGID32
        //sysnb    Geteuid() (euid int) = SYS_GETEUID32
        //sysnb    Getgid() (gid int) = SYS_GETGID32
        //sysnb    Getuid() (uid int) = SYS_GETUID32
        //sysnb    InotifyInit() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error) = SYS_LCHOWN32
        //sys    Listen(s int, n int) (err error)
        //sys    Lstat(path string, stat *Stat_t) (err error) = SYS_LSTAT64
        //sys    Pause() (err error)
        //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
        //sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error) = SYS_SENDFILE64
        //sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error) = SYS__NEWSELECT
        //sys    setfsgid(gid int) (prev int, err error) = SYS_SETFSGID32
        //sys    setfsuid(uid int) (prev int, err error) = SYS_SETFSUID32
        //sysnb    Setregid(rgid int, egid int) (err error) = SYS_SETREGID32
        //sysnb    Setresgid(rgid int, egid int, sgid int) (err error) = SYS_SETRESGID32
        //sysnb    Setresuid(ruid int, euid int, suid int) (err error) = SYS_SETRESUID32
        //sysnb    Setreuid(ruid int, euid int) (err error) = SYS_SETREUID32
        //sys    Shutdown(fd int, how int) (err error)
        //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int, err error)
        //sys    Stat(path string, stat *Stat_t) (err error) = SYS_STAT64
        //sys    Ustat(dev int, ubuf *Ustat_t) (err error)

        //sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
        //sysnb    Gettimeofday(tv *Timeval) (err error)

        public static (Time_t, error) Time(ptr<Time_t> _addr_t)
        {
            Time_t _p0 = default;
            error _p0 = default!;
            ref Time_t t = ref _addr_t.val;

            ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
            var err = Gettimeofday(_addr_tv);
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

        public static error Utime(@string path, ptr<Utimbuf> _addr_buf)
        {
            ref Utimbuf buf = ref _addr_buf.val;

            Timeval tv = new slice<Timeval>(new Timeval[] { {Sec:buf.Actime}, {Sec:buf.Modtime} });
            return error.As(Utimes(path, tv))!;
        }

        //sys    utimes(path string, times *[2]Timeval) (err error)

        //sys   Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
        //sys   Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
        //sys    Truncate(path string, length int64) (err error) = SYS_TRUNCATE64
        //sys    Ftruncate(fd int, length int64) (err error) = SYS_FTRUNCATE64

        public static error Fadvise(long fd, long offset, long length, long advice)
        {
            error err = default!;

            var (_, _, e1) = Syscall6(SYS_ARM_FADVISE64_64, uintptr(fd), uintptr(advice), uintptr(offset), uintptr(offset >> (int)(32L)), uintptr(length), uintptr(length >> (int)(32L)));
            if (e1 != 0L)
            {
                err = errnoErr(e1);
            }

            return ;

        }

        //sys    mmap2(addr uintptr, length uintptr, prot int, flags int, fd int, pageOffset uintptr) (xaddr uintptr, err error)

        public static error Fstatfs(long fd, ptr<Statfs_t> _addr_buf)
        {
            error err = default!;
            ref Statfs_t buf = ref _addr_buf.val;

            var (_, _, e) = Syscall(SYS_FSTATFS64, uintptr(fd), @unsafe.Sizeof(buf), uintptr(@unsafe.Pointer(buf)));
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        public static error Statfs(@string path, ptr<Statfs_t> _addr_buf)
        {
            error err = default!;
            ref Statfs_t buf = ref _addr_buf.val;

            var (pathp, err) = BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (_, _, e) = Syscall(SYS_STATFS64, uintptr(@unsafe.Pointer(pathp)), @unsafe.Sizeof(buf), uintptr(@unsafe.Pointer(buf)));
            if (e != 0L)
            {
                err = e;
            }

            return ;

        }

        private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, long prot, long flags, long fd, long offset)
        {
            System.UIntPtr xaddr = default;
            error err = default!;

            var page = uintptr(offset / 4096L);
            if (offset != int64(page) * 4096L)
            {
                return (0L, error.As(EINVAL)!);
            }

            return mmap2(addr, length, prot, flags, fd, page);

        }

        private partial struct rlimit32
        {
            public uint Cur;
            public uint Max;
        }

        //sysnb getrlimit(resource int, rlim *rlimit32) (err error) = SYS_UGETRLIMIT

        private static readonly var rlimInf32 = (var)~uint32(0L);

        private static readonly var rlimInf64 = (var)~uint64(0L);



        public static error Getrlimit(long resource, ptr<Rlimit> _addr_rlim)
        {
            error err = default!;
            ref Rlimit rlim = ref _addr_rlim.val;

            err = prlimit(0L, resource, null, rlim);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            }

            ref rlimit32 rl = ref heap(new rlimit32(), out ptr<rlimit32> _addr_rl);
            err = getrlimit(resource, _addr_rl);
            if (err != null)
            {
                return ;
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

            return ;

        }

        //sysnb setrlimit(resource int, rlim *rlimit32) (err error) = SYS_SETRLIMIT

        public static error Setrlimit(long resource, ptr<Rlimit> _addr_rlim)
        {
            error err = default!;
            ref Rlimit rlim = ref _addr_rlim.val;

            err = prlimit(0L, resource, rlim, null);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            }

            ref rlimit32 rl = ref heap(new rlimit32(), out ptr<rlimit32> _addr_rl);
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
                return error.As(EINVAL)!;
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
                return error.As(EINVAL)!;
            }

            return error.As(setrlimit(resource, _addr_rl))!;

        }

        private static ulong PC(this ptr<PtraceRegs> _addr_r)
        {
            ref PtraceRegs r = ref _addr_r.val;

            return uint64(r.Uregs[15L]);
        }

        private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc)
        {
            ref PtraceRegs r = ref _addr_r.val;

            r.Uregs[15L] = uint32(pc);
        }

        private static void SetLen(this ptr<Iovec> _addr_iov, long length)
        {
            ref Iovec iov = ref _addr_iov.val;

            iov.Len = uint32(length);
        }

        private static void SetControllen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Controllen = uint32(length);
        }

        private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Iovlen = uint32(length);
        }

        private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, long length)
        {
            ref Cmsghdr cmsg = ref _addr_cmsg.val;

            cmsg.Len = uint32(length);
        }

        //sys    poll(fds *PollFd, nfds int, timeout int) (n int, err error)

        public static (long, error) Poll(slice<PollFd> fds, long timeout)
        {
            long n = default;
            error err = default!;

            if (len(fds) == 0L)
            {
                return poll(null, 0L, timeout);
            }

            return poll(_addr_fds[0L], len(fds), timeout);

        }

        //sys    armSyncFileRange(fd int, flags int, off int64, n int64) (err error) = SYS_ARM_SYNC_FILE_RANGE

        public static error SyncFileRange(long fd, long off, long n, long flags)
        { 
            // The sync_file_range and arm_sync_file_range syscalls differ only in the
            // order of their arguments.
            return error.As(armSyncFileRange(fd, flags, off, n))!;

        }

        //sys    kexecFileLoad(kernelFd int, initrdFd int, cmdlineLen int, cmdline string, flags int) (err error)

        public static error KexecFileLoad(long kernelFd, long initrdFd, @string cmdline, long flags)
        {
            var cmdlineLen = len(cmdline);
            if (cmdlineLen > 0L)
            { 
                // Account for the additional NULL byte added by
                // BytePtrFromString in kexecFileLoad. The kexec_file_load
                // syscall expects a NULL-terminated string.
                cmdlineLen++;

            }

            return error.As(kexecFileLoad(kernelFd, initrdFd, cmdlineLen, cmdline, flags))!;

        }
    }
}}}}}}
