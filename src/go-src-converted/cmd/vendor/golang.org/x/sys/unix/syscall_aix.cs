// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix

// Aix system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package unix -- go2cs converted at 2020 October 08 04:46:39 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_aix.go
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
        /*
         * Wrapped
         */

        //sys    utimes(path string, times *[2]Timeval) (err error)
        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }
            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)
        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimensat(AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L))!;

        }

        public static error UtimesNanoAt(long dirfd, @string path, slice<Timespec> ts, long flags)
        {
            if (ts == null)
            {
                return error.As(utimensat(dirfd, path, null, flags))!;
            }

            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), flags))!;

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet4> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrInet4 sa = ref _addr_sa.val;

            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_INET;
            ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrInet4, error.As(null!)!);

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet6> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrInet6 sa = ref _addr_sa.val;

            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_INET6;
            ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrInet6, error.As(null!)!);

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrUnix> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrUnix sa = ref _addr_sa.val;

            var name = sa.Name;
            var n = len(name);
            if (n > len(sa.raw.Path))
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            if (n == len(sa.raw.Path) && name[0L] != '@')
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_UNIX;
            for (long i = 0L; i < n; i++)
            {
                sa.raw.Path[i] = uint8(name[i]);
            } 
            // length is family (uint16), name, NUL.
 
            // length is family (uint16), name, NUL.
            var sl = _Socklen(2L);
            if (n > 0L)
            {
                sl += _Socklen(n) + 1L;
            }

            if (sa.raw.Path[0L] == '@')
            {
                sa.raw.Path[0L] = 0L; 
                // Don't count trailing NUL for abstract address.
                sl--;

            }

            return (@unsafe.Pointer(_addr_sa.raw), sl, error.As(null!)!);

        }

        public static (Sockaddr, error) Getsockname(long fd)
        {
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            err = getsockname(fd, _addr_rsa, _addr_len);

            if (err != null)
            {
                return ;
            }

            return anyToSockaddr(fd, _addr_rsa);

        }

        //sys    getcwd(buf []byte) (err error)

        public static readonly var ImplementsGetwd = (var)true;



        public static (@string, error) Getwd()
        {
            @string ret = default;
            error err = default!;

            {
                var len = uint64(4096L);

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    var b = make_slice<byte>(len);
                    var err = getcwd(b);
                    if (err == null)
                    {
                        long i = 0L;
                        while (b[i] != 0L)
                        {
                            i++;
                    len *= 2L;
                        }

                        return (string(b[0L..i]), error.As(null!)!);

                    }

                    if (err != ERANGE)
                    {
                        return ("", error.As(err)!);
                    }

                }

            }

        }

        public static (long, error) Getcwd(slice<byte> buf)
        {
            long n = default;
            error err = default!;

            err = getcwd(buf);
            if (err == null)
            {
                long i = 0L;
                while (buf[i] != 0L)
                {
                    i++;
                }

                n = i + 1L;

            }

            return ;

        }

        public static (slice<long>, error) Getgroups()
        {
            slice<long> gids = default;
            error err = default!;

            var (n, err) = getgroups(0L, null);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (n == 0L)
            {
                return (null, error.As(null!)!);
            } 

            // Sanity check group count. Max is 16 on BSD.
            if (n < 0L || n > 1000L)
            {
                return (null, error.As(EINVAL)!);
            }

            var a = make_slice<_Gid_t>(n);
            n, err = getgroups(n, _addr_a[0L]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            gids = make_slice<long>(n);
            foreach (var (i, v) in a[0L..n])
            {
                gids[i] = int(v);
            }
            return ;

        }

        public static error Setgroups(slice<long> gids)
        {
            error err = default!;

            if (len(gids) == 0L)
            {
                return error.As(setgroups(0L, null))!;
            }

            var a = make_slice<_Gid_t>(len(gids));
            foreach (var (i, v) in gids)
            {
                a[i] = _Gid_t(v);
            }
            return error.As(setgroups(len(a), _addr_a[0L]))!;

        }

        /*
         * Socket
         */

        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)

        public static (long, Sockaddr, error) Accept(long fd)
        {
            long nfd = default;
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            nfd, err = accept(fd, _addr_rsa, _addr_len);
            if (nfd == -1L)
            {
                return ;
            }

            sa, err = anyToSockaddr(fd, _addr_rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }

            return ;

        }

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            long n = default;
            long oobn = default;
            long recvflags = default;
            Sockaddr from = default;
            error err = default!;
 
            // Recvmsg not implemented on AIX
            ptr<SockaddrUnix> sa = @new<SockaddrUnix>();
            return (-1L, -1L, -1L, sa, error.As(ENOSYS)!);

        }

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            error err = default!;

            _, err = SendmsgN(fd, p, oob, to, flags);
            return ;
        }

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            long n = default;
            error err = default!;
 
            // SendmsgN not implemented on AIX
            return (-1L, error.As(ENOSYS)!);

        }

        private static (Sockaddr, error) anyToSockaddr(long fd, ptr<RawSockaddrAny> _addr_rsa)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;



            if (rsa.Addr.Family == AF_UNIX) 
                var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
                ptr<SockaddrUnix> sa = @new<SockaddrUnix>(); 

                // Some versions of AIX have a bug in getsockname (see IV78655).
                // We can't rely on sa.Len being set correctly.
                var n = SizeofSockaddrUnix - 3L; // subtract leading Family, Len, terminating NUL.
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < n; i++)
                    {
                        if (pp.Path[i] == 0L)
                        {
                            n = i;
                            break;
                        }

                    }


                    i = i__prev1;
                }

                ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Path[0L]))[0L..n];
                sa.Name = string(bytes);
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_INET) 
                pp = (RawSockaddrInet4.val)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet4>();
                ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_INET6) 
                pp = (RawSockaddrInet6.val)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet6>();
                p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                sa.ZoneId = pp.Scope_id;
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, error.As(null!)!);
                        return (null, error.As(EAFNOSUPPORT)!);

        }

        public static error Gettimeofday(ptr<Timeval> _addr_tv)
        {
            error err = default!;
            ref Timeval tv = ref _addr_tv.val;

            err = gettimeofday(tv, null);
            return ;
        }

        public static (long, error) Sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            if (raceenabled)
            {
                raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            return sendfile(outfd, infd, _addr_offset, count);

        }

        // TODO
        private static (long, error) sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            return (-1L, error.As(ENOSYS)!);
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            var (reclen, ok) = direntReclen(buf);
            if (!ok)
            {
                return (0L, false);
            }

            return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);

        }

        //sys    getdirent(fd int, buf []byte) (n int, err error)
        public static (long, error) Getdents(long fd, slice<byte> buf)
        {
            long n = default;
            error err = default!;

            return getdirent(fd, buf);
        }

        //sys    wait4(pid Pid_t, status *_C_int, options int, rusage *Rusage) (wpid Pid_t, err error)
        public static (long, error) Wait4(long pid, ptr<WaitStatus> _addr_wstatus, long options, ptr<Rusage> _addr_rusage)
        {
            long wpid = default;
            error err = default!;
            ref WaitStatus wstatus = ref _addr_wstatus.val;
            ref Rusage rusage = ref _addr_rusage.val;

            ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
            Pid_t r = default;
            err = ERESTART; 
            // AIX wait4 may return with ERESTART errno, while the processus is still
            // active.
            while (err == ERESTART)
            {
                r, err = wait4(Pid_t(pid), _addr_status, options, rusage);
            }

            wpid = int(r);
            if (wstatus != null)
            {
                wstatus = WaitStatus(status);
            }

            return ;

        }

        /*
         * Wait
         */

        public partial struct WaitStatus // : uint
        {
        }

        public static bool Stopped(this WaitStatus w)
        {
            return w & 0x40UL != 0L;
        }
        public static Signal StopSignal(this WaitStatus w)
        {
            if (!w.Stopped())
            {
                return -1L;
            }

            return Signal(w >> (int)(8L)) & 0xFFUL;

        }

        public static bool Exited(this WaitStatus w)
        {
            return w & 0xFFUL == 0L;
        }
        public static long ExitStatus(this WaitStatus w)
        {
            if (!w.Exited())
            {
                return -1L;
            }

            return int((w >> (int)(8L)) & 0xFFUL);

        }

        public static bool Signaled(this WaitStatus w)
        {
            return w & 0x40UL == 0L && w & 0xFFUL != 0L;
        }
        public static Signal Signal(this WaitStatus w)
        {
            if (!w.Signaled())
            {
                return -1L;
            }

            return Signal(w >> (int)(16L)) & 0xFFUL;

        }

        public static bool Continued(this WaitStatus w)
        {
            return w & 0x01000000UL != 0L;
        }

        public static bool CoreDump(this WaitStatus w)
        {
            return w & 0x80UL == 0x80UL;
        }

        public static long TrapCause(this WaitStatus w)
        {
            return -1L;
        }

        //sys    ioctl(fd int, req uint, arg uintptr) (err error)

        // fcntl must never be called with cmd=F_DUP2FD because it doesn't work on AIX
        // There is no way to create a custom fcntl and to keep //sys fcntl easily,
        // Therefore, the programmer must call dup2 instead of fcntl in this case.

        // FcntlInt performs a fcntl syscall on fd with the provided command and argument.
        //sys    FcntlInt(fd uintptr, cmd int, arg int) (r int,err error) = fcntl

        // FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
        //sys    FcntlFlock(fd uintptr, cmd int, lk *Flock_t) (err error) = fcntl

        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)

        /*
         * Direct access
         */

        //sys    Acct(path string) (err error)
        //sys    Chdir(path string) (err error)
        //sys    Chroot(path string) (err error)
        //sys    Close(fd int) (err error)
        //sys    Dup(oldfd int) (fd int, err error)
        //sys    Exit(code int)
        //sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
        //sys    Fdatasync(fd int) (err error)
        //sys    Fsync(fd int) (err error)
        // readdir_r
        //sysnb    Getpgid(pid int) (pgid int, err error)

        //sys    Getpgrp() (pid int)

        //sysnb    Getpid() (pid int)
        //sysnb    Getppid() (ppid int)
        //sys    Getpriority(which int, who int) (prio int, err error)
        //sysnb    Getrusage(who int, rusage *Rusage) (err error)
        //sysnb    Getsid(pid int) (sid int, err error)
        //sysnb    Kill(pid int, sig Signal) (err error)
        //sys    Klogctl(typ int, buf []byte) (n int, err error) = syslog
        //sys    Mkdir(dirfd int, path string, mode uint32) (err error)
        //sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
        //sys    Mkfifo(path string, mode uint32) (err error)
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys   Open(path string, mode int, perm uint32) (fd int, err error) = open64
        //sys   Openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Readlink(path string, buf []byte) (n int, err error)
        //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
        //sys    Setdomainname(p []byte) (err error)
        //sys    Sethostname(p []byte) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tv *Timeval) (err error)

        //sys    Setuid(uid int) (err error)
        //sys    Setgid(uid int) (err error)

        //sys    Setpriority(which int, who int, prio int) (err error)
        //sys    Statx(dirfd int, path string, flags int, mask int, stat *Statx_t) (err error)
        //sys    Sync()
        //sysnb    Times(tms *Tms) (ticks uintptr, err error)
        //sysnb    Umask(mask int) (oldmask int)
        //sysnb    Uname(buf *Utsname) (err error)
        //sys   Unlink(path string) (err error)
        //sys   Unlinkat(dirfd int, path string, flags int) (err error)
        //sys    Ustat(dev int, ubuf *Ustat_t) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    readlen(fd int, p *byte, np int) (n int, err error) = read
        //sys    writelen(fd int, p *byte, np int) (n int, err error) = write

        //sys    Dup2(oldfd int, newfd int) (err error)
        //sys    Fadvise(fd int, offset int64, length int64, advice int) (err error) = posix_fadvise64
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    fstat(fd int, stat *Stat_t) (err error)
        //sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = fstatat
        //sys    Fstatfs(fd int, buf *Statfs_t) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sysnb    Getegid() (egid int)
        //sysnb    Geteuid() (euid int)
        //sysnb    Getgid() (gid int)
        //sysnb    Getuid() (uid int)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Listen(s int, n int) (err error)
        //sys    lstat(path string, stat *Stat_t) (err error)
        //sys    Pause() (err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = pread64
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = pwrite64
        //sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
        //sys    Pselect(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timespec, sigmask *Sigset_t) (n int, err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sys    Shutdown(fd int, how int) (err error)
        //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
        //sys    stat(path string, statptr *Stat_t) (err error)
        //sys    Statfs(path string, buf *Statfs_t) (err error)
        //sys    Truncate(path string, length int64) (err error)

        //sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
        //sysnb    setgroups(n int, list *_Gid_t) (err error)
        //sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
        //sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
        //sysnb    socket(domain int, typ int, proto int) (fd int, err error)
        //sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)
        //sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sysnb    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
        //sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)

        // In order to use msghdr structure with Control, Controllen, nrecvmsg and nsendmsg must be used.
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error) = nrecvmsg
        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = nsendmsg

        //sys    munmap(addr uintptr, length uintptr) (err error)

        private static ptr<mmapper> mapper = addr(new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,));

        public static (slice<byte>, error) Mmap(long fd, long offset, long length, long prot, long flags)
        {
            slice<byte> data = default;
            error err = default!;

            return mapper.Mmap(fd, offset, length, prot, flags);
        }

        public static error Munmap(slice<byte> b)
        {
            error err = default!;

            return error.As(mapper.Munmap(b))!;
        }

        //sys    Madvise(b []byte, advice int) (err error)
        //sys    Mprotect(b []byte, prot int) (err error)
        //sys    Mlock(b []byte) (err error)
        //sys    Mlockall(flags int) (err error)
        //sys    Msync(b []byte, flags int) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Munlockall() (err error)

        //sysnb pipe(p *[2]_C_int) (err error)

        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            err = pipe(_addr_pp);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

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

        //sys    gettimeofday(tv *Timeval, tzp *Timezone) (err error)
        //sysnb    Time(t *Time_t) (tt Time_t, err error)
        //sys    Utime(path string, buf *Utimbuf) (err error)

        //sys    Getsystemcfg(label int) (n uint64)

        //sys    umount(target string) (err error)
        public static error Unmount(@string target, long flags)
        {
            error err = default!;

            if (flags != 0L)
            { 
                // AIX doesn't have any flags for umount.
                return error.As(ENOSYS)!;

            }

            return error.As(umount(target))!;

        }
    }
}}}}}}
