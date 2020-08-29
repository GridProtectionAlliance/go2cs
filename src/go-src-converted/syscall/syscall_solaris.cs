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

// package syscall -- go2cs converted at 2020 August 29 08:38:20 UTC
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

        private static long clen(slice<byte> n)
        {
            for (long i = 0L; i < len(n); i++)
            {>>MARKER:FUNCTION_sysvicall6_BLOCK_PREFIX<<
                if (n[i] == 0L)
                {>>MARKER:FUNCTION_rawSysvicall6_BLOCK_PREFIX<<
                    return i;
                }
            }

            return len(n);
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            var (reclen, ok) = direntReclen(buf);
            if (!ok)
            {
                return (0L, false);
            }
            return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);
        }

        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) pipe()
;

        public static error Pipe(slice<long> p)
        {
            if (len(p) != 2L)
            {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
                return error.As(EINVAL);
            }
            var (r0, w0, e1) = pipe();
            if (e1 != 0L)
            {
                err = Errno(e1);
            }
            p[0L] = int(r0);
            p[1L] = int(w0);
            return;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrInet4 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_INET;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrInet4, null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrInet6 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_INET6;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrInet6, null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrUnix sa)
        {
            var name = sa.Name;
            var n = len(name);
            if (n >= len(sa.raw.Path))
            {
                return (null, 0L, EINVAL);
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
            return (@unsafe.Pointer(ref sa.raw), sl, null);
        }

        public static (Sockaddr, error) Getsockname(long fd)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            err = getsockname(fd, ref rsa, ref len);

            if (err != null)
            {
                return;
            }
            return anyToSockaddr(ref rsa);
        }

        public static readonly var ImplementsGetwd = true;

        //sys    Getcwd(buf []byte) (n int, err error)



        //sys    Getcwd(buf []byte) (n int, err error)

        public static (@string, error) Getwd()
        {
            array<byte> buf = new array<byte>(PathMax); 
            // Getcwd will return an error if it failed for any reason.
            _, err = Getcwd(buf[0L..]);
            if (err != null)
            {
                return ("", err);
            }
            var n = clen(buf[..]);
            if (n < 1L)
            {
                return ("", EINVAL);
            }
            return (string(buf[..n]), null);
        }

        /*
         * Wrapped
         */

        //sysnb    getgroups(ngid int, gid *_Gid_t) (n int, err error)
        //sysnb    setgroups(ngid int, gid *_Gid_t) (err error)

        public static (slice<long>, error) Getgroups()
        {
            var (n, err) = getgroups(0L, null);
            if (err != null)
            {
                return (null, err);
            }
            if (n == 0L)
            {
                return (null, null);
            } 

            // Sanity check group count. Max is 16 on BSD.
            if (n < 0L || n > 1000L)
            {
                return (null, EINVAL);
            }
            var a = make_slice<_Gid_t>(n);
            n, err = getgroups(n, ref a[0L]);
            if (err != null)
            {
                return (null, err);
            }
            gids = make_slice<long>(n);
            foreach (var (i, v) in a[0L..n])
            {
                gids[i] = int(v);
            }
            return;
        }

        public static error Setgroups(slice<long> gids)
        {
            if (len(gids) == 0L)
            {
                return error.As(setgroups(0L, null));
            }
            var a = make_slice<_Gid_t>(len(gids));
            foreach (var (i, v) in gids)
            {
                a[i] = _Gid_t(v);
            }
            return error.As(setgroups(len(a), ref a[0L]));
        }

        public static (long, error) ReadDirent(long fd, slice<byte> buf)
        { 
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

        private static readonly ulong mask = 0x7FUL;
        private static readonly ulong core = 0x80UL;
        private static readonly long shift = 8L;

        private static readonly long exited = 0L;
        private static readonly ulong stopped = 0x7FUL;

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

        private static (System.UIntPtr, System.UIntPtr) wait4(System.UIntPtr pid, ref WaitStatus wstatus, System.UIntPtr options, ref Rusage rusage)
;

        public static (long, error) Wait4(long pid, ref WaitStatus wstatus, long options, ref Rusage rusage)
        {
            var (r0, e1) = wait4(uintptr(pid), wstatus, uintptr(options), rusage);
            if (e1 != 0L)
            {>>MARKER:FUNCTION_wait4_BLOCK_PREFIX<<
                err = Errno(e1);
            }
            return (int(r0), err);
        }

        private static (@string, System.UIntPtr) gethostname()
;

        public static (@string, error) Gethostname()
        {
            var (name, e1) = gethostname();
            if (e1 != 0L)
            {>>MARKER:FUNCTION_gethostname_BLOCK_PREFIX<<
                err = Errno(e1);
            }
            return (name, err);
        }

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL);
            }
            return error.As(utimensat(_AT_FDCWD, path, new ptr<ref array<Timespec>>(@unsafe.Pointer(ref ts[0L])), 0L));
        }

        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)

        // FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
        public static error FcntlFlock(System.UIntPtr fd, long cmd, ref Flock_t lk)
        {
            var (_, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(ref libc_fcntl)), 3L, uintptr(fd), uintptr(cmd), uintptr(@unsafe.Pointer(lk)), 0L, 0L, 0L);
            if (e1 != 0L)
            {
                return error.As(e1);
            }
            return error.As(null);
        }

        private static (Sockaddr, error) anyToSockaddr(ref RawSockaddrAny rsa)
        {

            if (rsa.Addr.Family == AF_UNIX) 
                var pp = (RawSockaddrUnix.Value)(@unsafe.Pointer(rsa));
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

                ref array<byte> bytes = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Path[0L]))[0L..n];
                sa.Name = string(bytes);
                return (sa, null);
            else if (rsa.Addr.Family == AF_INET) 
                pp = (RawSockaddrInet4.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet4>();
                ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
            else if (rsa.Addr.Family == AF_INET6) 
                pp = (RawSockaddrInet6.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet6>();
                p = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Port));
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
                return (sa, null);
                        return (null, EAFNOSUPPORT);
        }

        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error) = libsocket.accept

        public static (long, Sockaddr, error) Accept(long fd)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            nfd, err = accept(fd, ref rsa, ref len);
            if (err != null)
            {
                return;
            }
            sa, err = anyToSockaddr(ref rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }
            return;
        }

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            Msghdr msg = default;
            RawSockaddrAny rsa = default;
            msg.Name = (byte.Value)(@unsafe.Pointer(ref rsa));
            msg.Namelen = uint32(SizeofSockaddrAny);
            Iovec iov = default;
            if (len(p) > 0L)
            {
                iov.Base = (int8.Value)(@unsafe.Pointer(ref p[0L]));
                iov.SetLen(len(p));
            }
            sbyte dummy = default;
            if (len(oob) > 0L)
            { 
                // receive at least one normal byte
                if (len(p) == 0L)
                {
                    iov.Base = ref dummy;
                    iov.SetLen(1L);
                }
                msg.Accrights = (int8.Value)(@unsafe.Pointer(ref oob[0L]));
                msg.Accrightslen = int32(len(oob));
            }
            msg.Iov = ref iov;
            msg.Iovlen = 1L;
            n, err = recvmsg(fd, ref msg, flags);

            if (err != null)
            {
                return;
            }
            oobn = int(msg.Accrightslen); 
            // source address is only specified if the socket is unconnected
            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(ref rsa);
            }
            return;
        }

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            _, err = SendmsgN(fd, p, oob, to, flags);
            return;
        }

        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = libsocket.__xnet_sendmsg

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            unsafe.Pointer ptr = default;
            _Socklen salen = default;
            if (to != null)
            {
                ptr, salen, err = to.sockaddr();
                if (err != null)
                {
                    return (0L, err);
                }
            }
            Msghdr msg = default;
            msg.Name = (byte.Value)(@unsafe.Pointer(ptr));
            msg.Namelen = uint32(salen);
            Iovec iov = default;
            if (len(p) > 0L)
            {
                iov.Base = (int8.Value)(@unsafe.Pointer(ref p[0L]));
                iov.SetLen(len(p));
            }
            sbyte dummy = default;
            if (len(oob) > 0L)
            { 
                // send at least one normal byte
                if (len(p) == 0L)
                {
                    iov.Base = ref dummy;
                    iov.SetLen(1L);
                }
                msg.Accrights = (int8.Value)(@unsafe.Pointer(ref oob[0L]));
                msg.Accrightslen = int32(len(oob));
            }
            msg.Iov = ref iov;
            msg.Iovlen = 1L;
            n, err = sendmsg(fd, ref msg, flags);

            if (err != null)
            {
                return (0L, err);
            }
            if (len(oob) > 0L && len(p) == 0L)
            {
                n = 0L;
            }
            return (n, null);
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
            var (ptr, err) = getexecname();
            if (err != null)
            {
                return ("", err);
            }
            ref array<byte> bytes = new ptr<ref array<byte>>(ptr)[..];
            foreach (var (i, b) in bytes)
            {
                if (b == 0L)
                {
                    return (string(bytes[..i]), null);
                }
            }
            panic("unreachable");
        });

        private static (long, error) readlen(long fd, ref byte buf, long nbuf)
        {
            var (r0, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(ref libc_read)), 3L, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0L, 0L, 0L);
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }
            return;
        }

        private static (long, error) writelen(long fd, ref byte buf, long nbuf)
        {
            var (r0, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(ref libc_write)), 3L, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0L, 0L, 0L);
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }
            return;
        }

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL);
            }
            return error.As(utimes(path, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }
    }
}
