// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Solaris system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_solaris.go or syscall_unix.go.

// package syscall -- go2cs converted at 2020 October 09 05:01:56 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_solaris.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Implemented in asm_solaris_amd64.s.
        private static (System.UIntPtr, System.UIntPtr, Errno) rawSysvicall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) sysvicall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        public partial struct SockaddrDatalink
        {
            public ushort Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<sbyte> Data;
            public RawSockaddrDatalink raw;
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
            {>>MARKER:FUNCTION_sysvicall6_BLOCK_PREFIX<<
                return (0L, false);
            }

            return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);

        }

        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) pipe()
;

        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
                return error.As(EINVAL)!;
            }

            var (r0, w0, e1) = pipe();
            if (e1 != 0L)
            {>>MARKER:FUNCTION_rawSysvicall6_BLOCK_PREFIX<<
                err = Errno(e1);
            }

            p[0L] = int(r0);
            p[1L] = int(w0);
            return ;

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
            if (n >= len(sa.raw.Path))
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_UNIX;
            for (long i = 0L; i < n; i++)
            {
                sa.raw.Path[i] = int8(name[i]);
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

            return anyToSockaddr(_addr_rsa);

        }

        public static readonly var ImplementsGetwd = true;

        //sys    Getcwd(buf []byte) (n int, err error)



        //sys    Getcwd(buf []byte) (n int, err error)

        public static (@string, error) Getwd()
        {
            @string wd = default;
            error err = default!;

            array<byte> buf = new array<byte>(PathMax); 
            // Getcwd will return an error if it failed for any reason.
            _, err = Getcwd(buf[0L..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var n = clen(buf[..]);
            if (n < 1L)
            {
                return ("", error.As(EINVAL)!);
            }

            return (string(buf[..n]), error.As(null!)!);

        }

        /*
         * Wrapped
         */

        //sysnb    getgroups(ngid int, gid *_Gid_t) (n int, err error)
        //sysnb    setgroups(ngid int, gid *_Gid_t) (err error)

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

        public static (long, error) ReadDirent(long fd, slice<byte> buf)
        {
            long n = default;
            error err = default!;
 
            // Final argument is (basep *uintptr) and the syscall doesn't take nil.
            // TODO(rsc): Can we use a single global basep for all calls?
            return Getdents(fd, buf, @new<uintptr>());

        }

        // Wait status is 7 bits at bottom, either 0 (exited),
        // 0x7F (stopped), or a signal number that caused an exit.
        // The 0x80 bit is whether there was a core dump.
        // An extra number (exit code, signal causing a stop)
        // is in the high bits.

        public partial struct WaitStatus // : uint
        {
        }

        private static readonly ulong mask = (ulong)0x7FUL;
        private static readonly ulong core = (ulong)0x80UL;
        private static readonly long shift = (long)8L;

        private static readonly long exited = (long)0L;
        private static readonly ulong stopped = (ulong)0x7FUL;


        public static bool Exited(this WaitStatus w)
        {
            return w & mask == exited;
        }

        public static long ExitStatus(this WaitStatus w)
        {
            if (w & mask != exited)
            {
                return -1L;
            }

            return int(w >> (int)(shift));

        }

        public static bool Signaled(this WaitStatus w)
        {
            return w & mask != stopped && w & mask != 0L;
        }

        public static Signal Signal(this WaitStatus w)
        {
            var sig = Signal(w & mask);
            if (sig == stopped || sig == 0L)
            {
                return -1L;
            }

            return sig;

        }

        public static bool CoreDump(this WaitStatus w)
        {
            return w.Signaled() && w & core != 0L;
        }

        public static bool Stopped(this WaitStatus w)
        {
            return w & mask == stopped && Signal(w >> (int)(shift)) != SIGSTOP;
        }

        public static bool Continued(this WaitStatus w)
        {
            return w & mask == stopped && Signal(w >> (int)(shift)) == SIGSTOP;
        }

        public static Signal StopSignal(this WaitStatus w)
        {
            if (!w.Stopped())
            {
                return -1L;
            }

            return Signal(w >> (int)(shift)) & 0xFFUL;

        }

        public static long TrapCause(this WaitStatus w)
        {
            return -1L;
        }

        private static (System.UIntPtr, System.UIntPtr) wait4(System.UIntPtr pid, ptr<WaitStatus> wstatus, System.UIntPtr options, ptr<Rusage> rusage)
;

        public static (long, error) Wait4(long pid, ptr<WaitStatus> _addr_wstatus, long options, ptr<Rusage> _addr_rusage)
        {
            long wpid = default;
            error err = default!;
            ref WaitStatus wstatus = ref _addr_wstatus.val;
            ref Rusage rusage = ref _addr_rusage.val;

            var (r0, e1) = wait4(uintptr(pid), _addr_wstatus, uintptr(options), _addr_rusage);
            if (e1 != 0L)
            {>>MARKER:FUNCTION_wait4_BLOCK_PREFIX<<
                err = Errno(e1);
            }

            return (int(r0), error.As(err)!);

        }

        private static (@string, System.UIntPtr) gethostname()
;

        public static (@string, error) Gethostname()
        {
            @string name = default;
            error err = default!;

            var (name, e1) = gethostname();
            if (e1 != 0L)
            {>>MARKER:FUNCTION_gethostname_BLOCK_PREFIX<<
                err = Errno(e1);
            }

            return (name, error.As(err)!);

        }

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimensat(_AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L))!;

        }

        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)

        // FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
        public static error FcntlFlock(System.UIntPtr fd, long cmd, ptr<Flock_t> _addr_lk)
        {
            ref Flock_t lk = ref _addr_lk.val;

            var (_, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(_addr_libc_fcntl)), 3L, uintptr(fd), uintptr(cmd), uintptr(@unsafe.Pointer(lk)), 0L, 0L, 0L);
            if (e1 != 0L)
            {
                return error.As(e1)!;
            }

            return error.As(null!)!;

        }

        private static (Sockaddr, error) anyToSockaddr(ptr<RawSockaddrAny> _addr_rsa)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;


            if (rsa.Addr.Family == AF_UNIX) 
                var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
                ptr<SockaddrUnix> sa = @new<SockaddrUnix>(); 
                // Assume path ends at NUL.
                // This is not technically the Solaris semantics for
                // abstract Unix domain sockets -- they are supposed
                // to be uninterpreted fixed-size binary blobs -- but
                // everyone uses this convention.
                long n = 0L;
                while (n < len(pp.Path) && pp.Path[n] != 0L)
                {
                    n++;
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

                    for (long i = 0L; i < len(sa.Addr); i++)
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

        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error) = libsocket.accept

        public static (long, Sockaddr, error) Accept(long fd)
        {
            long nfd = default;
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            nfd, err = accept(fd, _addr_rsa, _addr_len);
            if (err != null)
            {
                return ;
            }

            sa, err = anyToSockaddr(_addr_rsa);
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

            ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            msg.Name = (byte.val)(@unsafe.Pointer(_addr_rsa));
            msg.Namelen = uint32(SizeofSockaddrAny);
            ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
            if (len(p) > 0L)
            {
                iov.Base = (int8.val)(@unsafe.Pointer(_addr_p[0L]));
                iov.SetLen(len(p));
            }

            ref sbyte dummy = ref heap(out ptr<sbyte> _addr_dummy);
            if (len(oob) > 0L)
            { 
                // receive at least one normal byte
                if (len(p) == 0L)
                {
                    _addr_iov.Base = _addr_dummy;
                    iov.Base = ref _addr_iov.Base.val;
                    iov.SetLen(1L);

                }

                msg.Accrights = (int8.val)(@unsafe.Pointer(_addr_oob[0L]));
                msg.Accrightslen = int32(len(oob));

            }

            _addr_msg.Iov = _addr_iov;
            msg.Iov = ref _addr_msg.Iov.val;
            msg.Iovlen = 1L;
            n, err = recvmsg(fd, _addr_msg, flags);

            if (err != null)
            {
                return ;
            }

            oobn = int(msg.Accrightslen); 
            // source address is only specified if the socket is unconnected
            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(_addr_rsa);
            }

            return ;

        }

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            error err = default!;

            _, err = SendmsgN(fd, p, oob, to, flags);
            return ;
        }

        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = libsocket.__xnet_sendmsg

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            long n = default;
            error err = default!;

            unsafe.Pointer ptr = default;
            _Socklen salen = default;
            if (to != null)
            {
                ptr, salen, err = to.sockaddr();
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
            msg.Name = (byte.val)(@unsafe.Pointer(ptr));
            msg.Namelen = uint32(salen);
            ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
            if (len(p) > 0L)
            {
                iov.Base = (int8.val)(@unsafe.Pointer(_addr_p[0L]));
                iov.SetLen(len(p));
            }

            ref sbyte dummy = ref heap(out ptr<sbyte> _addr_dummy);
            if (len(oob) > 0L)
            { 
                // send at least one normal byte
                if (len(p) == 0L)
                {
                    _addr_iov.Base = _addr_dummy;
                    iov.Base = ref _addr_iov.Base.val;
                    iov.SetLen(1L);

                }

                msg.Accrights = (int8.val)(@unsafe.Pointer(_addr_oob[0L]));
                msg.Accrightslen = int32(len(oob));

            }

            _addr_msg.Iov = _addr_iov;
            msg.Iov = ref _addr_msg.Iov.val;
            msg.Iovlen = 1L;
            n, err = sendmsg(fd, _addr_msg, flags);

            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if (len(oob) > 0L && len(p) == 0L)
            {
                n = 0L;
            }

            return (n, error.As(null!)!);

        }

        /*
         * Exposed directly
         */
        //sys    Access(path string, mode uint32) (err error)
        //sys    Adjtime(delta *Timeval, olddelta *Timeval) (err error)
        //sys    Chdir(path string) (err error)
        //sys    Chmod(path string, mode uint32) (err error)
        //sys    Chown(path string, uid int, gid int) (err error)
        //sys    Chroot(path string) (err error)
        //sys    Close(fd int) (err error)
        //sys    Dup(fd int) (nfd int, err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Getdents(fd int, buf []byte, basep *uintptr) (n int, err error)
        //sysnb    Getgid() (gid int)
        //sysnb    Getpid() (pid int)
        //sys    Geteuid() (euid int)
        //sys    Getegid() (egid int)
        //sys    Getppid() (ppid int)
        //sys    Getpriority(which int, who int) (n int, err error)
        //sysnb    Getrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Gettimeofday(tv *Timeval) (err error)
        //sysnb    Getuid() (uid int)
        //sys    Kill(pid int, signum Signal) (err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Listen(s int, backlog int) (err error) = libsocket.__xnet_listen
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Pathconf(path string, name int) (val int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Readlink(path string, buf []byte) (n int, err error)
        //sys    Rename(from string, to string) (err error)
        //sys    Rmdir(path string) (err error)
        //sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = lseek
        //sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error) = libsendfile.sendfile
        //sysnb    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Setuid(uid int) (err error)
        //sys    Shutdown(s int, how int) (err error) = libsocket.shutdown
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Symlink(path string, link string) (err error)
        //sys    Sync() (err error)
        //sys    Truncate(path string, length int64) (err error)
        //sys    Fsync(fd int) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sys    Umask(newmask int) (oldmask int)
        //sys    Unlink(path string) (err error)
        //sys    utimes(path string, times *[2]Timeval) (err error)
        //sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error) = libsocket.__xnet_bind
        //sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error) = libsocket.__xnet_connect
        //sys    mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys    munmap(addr uintptr, length uintptr) (err error)
        //sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error) = libsocket.__xnet_sendto
        //sys    socket(domain int, typ int, proto int) (fd int, err error) = libsocket.__xnet_socket
        //sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error) = libsocket.__xnet_socketpair
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error) = libsocket.__xnet_getsockopt
        //sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error) = libsocket.getpeername
        //sys    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error) = libsocket.getsockname
        //sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error) = libsocket.setsockopt
        //sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error) = libsocket.recvfrom
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error) = libsocket.__xnet_recvmsg
        //sys    getexecname() (path unsafe.Pointer, err error) = libc.getexecname
        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)

        public static (@string, error) Getexecname() => func((_, panic, __) =>
        {
            @string path = default;
            error err = default!;

            var (ptr, err) = getexecname();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(ptr)[..];
            foreach (var (i, b) in bytes)
            {
                if (b == 0L)
                {
                    return (string(bytes[..i]), error.As(null!)!);
                }

            }
            panic("unreachable");

        });

        private static (long, error) readlen(long fd, ptr<byte> _addr_buf, long nbuf)
        {
            long n = default;
            error err = default!;
            ref byte buf = ref _addr_buf.val;

            var (r0, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(_addr_libc_read)), 3L, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0L, 0L, 0L);
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        private static (long, error) writelen(long fd, ptr<byte> _addr_buf, long nbuf)
        {
            long n = default;
            error err = default!;
            ref byte buf = ref _addr_buf.val;

            var (r0, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(_addr_libc_write)), 3L, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0L, 0L, 0L);
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }
    }
}
