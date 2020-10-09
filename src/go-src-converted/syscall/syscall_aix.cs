// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Aix system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package syscall -- go2cs converted at 2020 October 09 05:01:34 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_aix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Implemented in runtime/syscall_aix.go.
        private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        // Constant expected by package but not supported
        private static readonly var _ = iota;
        public static readonly var TIOCSCTTY = 0;
        public static readonly var SYS_EXECVE = 1;
        public static readonly var SYS_FCNTL = 2;


        public static readonly long F_DUPFD_CLOEXEC = (long)0L; 
        // AF_LOCAL doesn't exist on AIX
        public static readonly var AF_LOCAL = AF_UNIX;


        private static (long, long) Unix(this ptr<StTimespec_t> _addr_ts)
        {
            long sec = default;
            long nsec = default;
            ref StTimespec_t ts = ref _addr_ts.val;

            return (int64(ts.Sec), int64(ts.Nsec));
        }

        private static long Nano(this ptr<StTimespec_t> _addr_ts)
        {
            ref StTimespec_t ts = ref _addr_ts.val;

            return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
        }

        /*
         * Wrapped
         */

        // fcntl must never be called with cmd=F_DUP2FD because it doesn't work on AIX
        // There is no way to create a custom fcntl and to keep //sys fcntl easily,
        // because we need fcntl name for its libc symbol. This is linked with the script.
        // But, as fcntl is currently not exported and isn't called with F_DUP2FD,
        // it doesn't matter.
        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)
        //sys    Dup2(old int, new int) (err error)

        //sysnb pipe(p *[2]_C_int) (err error)
        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {>>MARKER:FUNCTION_syscall6_BLOCK_PREFIX<<
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            err = pipe(_addr_pp);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

        }

        //sys    readlink(path string, buf []byte, bufSize uint64) (n int, err error)
        public static (long, error) Readlink(@string path, slice<byte> buf)
        {
            long n = default;
            error err = default!;

            var s = uint64(len(buf));
            return readlink(path, buf, s);
        }

        //sys    utimes(path string, times *[2]Timeval) (err error)
        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {>>MARKER:FUNCTION_rawSyscall6_BLOCK_PREFIX<<
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

            return error.As(utimensat(_AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L))!;

        }

        //sys    unlinkat(dirfd int, path string, flags int) (err error)
        public static error Unlinkat(long dirfd, @string path)
        {
            error err = default!;

            return error.As(unlinkat(dirfd, path, 0L))!;
        }

        //sys    getcwd(buf *byte, size uint64) (err error)

        public static readonly var ImplementsGetwd = true;



        public static (@string, error) Getwd()
        {
            @string ret = default;
            error err = default!;

            {
                var len = uint64(4096L);

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    var b = make_slice<byte>(len);
                    var err = getcwd(_addr_b[0L], len);
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

            err = getcwd(_addr_buf[0L], uint64(len(buf)));
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

        public static error Gettimeofday(ptr<Timeval> _addr_tv)
        {
            error err = default!;
            ref Timeval tv = ref _addr_tv.val;

            err = gettimeofday(tv, null);
            return ;
        }

        // TODO
        private static (long, error) sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            return (-1L, error.As(ENOSYS)!);
        }

        //sys    getdirent(fd int, buf []byte) (n int, err error)
        public static (long, error) ReadDirent(long fd, slice<byte> buf)
        {
            long n = default;
            error err = default!;

            return getdirent(fd, buf);
        }

        //sys  wait4(pid _Pid_t, status *_C_int, options int, rusage *Rusage) (wpid _Pid_t, err error)
        public static (long, error) Wait4(long pid, ptr<WaitStatus> _addr_wstatus, long options, ptr<Rusage> _addr_rusage)
        {
            long wpid = default;
            error err = default!;
            ref WaitStatus wstatus = ref _addr_wstatus.val;
            ref Rusage rusage = ref _addr_rusage.val;

            ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
            _Pid_t r = default;
            err = ERESTART; 
            // AIX wait4 may return with ERESTART errno, while the processus is still
            // active.
            while (err == ERESTART)
            {
                r, err = wait4(_Pid_t(pid), _addr_status, options, rusage);
            }

            wpid = int(r);
            if (wstatus != null)
            {
                wstatus = WaitStatus(status);
            }

            return ;

        }

        /*
         * Socket
         */
        //sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sys   Getkerninfo(op int32, where uintptr, size uintptr, arg int64) (i int32, err error)
        //sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
        //sys    Listen(s int, backlog int) (err error)
        //sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
        //sys    socket(domain int, typ int, proto int) (fd int, err error)
        //sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)
        //sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sys    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
        //sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    Shutdown(s int, how int) (err error)

        // In order to use msghdr structure with Control, Controllen in golang.org/x/net,
        // nrecvmsg and nsendmsg must be used.
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error) = nrecvmsg
        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = nsendmsg

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

        private static void setLen(this ptr<RawSockaddrUnix> _addr_sa, long n)
        {
            ref RawSockaddrUnix sa = ref _addr_sa.val;

            sa.Len = uint8(3L + n); // 2 for Family, Len; 1 for NUL.
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

            sa.raw.Family = AF_UNIX;
            sa.raw.setLen(n);
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

        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)
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
                iov.Base = (byte.val)(@unsafe.Pointer(_addr_p[0L]));
                iov.SetLen(len(p));
            }

            ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
            if (len(oob) > 0L)
            {
                long sockType = default;
                sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
                if (err != null)
                {
                    return ;
                } 
                // receive at least one normal byte
                if (sockType != SOCK_DGRAM && len(p) == 0L)
                {
                    _addr_iov.Base = _addr_dummy;
                    iov.Base = ref _addr_iov.Base.val;
                    iov.SetLen(1L);

                }

                msg.Control = (byte.val)(@unsafe.Pointer(_addr_oob[0L]));
                msg.SetControllen(len(oob));

            }

            _addr_msg.Iov = _addr_iov;
            msg.Iov = ref _addr_msg.Iov.val;
            msg.Iovlen = 1L;
            n, err = recvmsg(fd, _addr_msg, flags);

            if (err != null)
            {
                return ;
            }

            oobn = int(msg.Controllen);
            recvflags = int(msg.Flags); 
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
                iov.Base = (byte.val)(@unsafe.Pointer(_addr_p[0L]));
                iov.SetLen(len(p));
            }

            ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
            if (len(oob) > 0L)
            {
                long sockType = default;
                sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
                if (err != null)
                {
                    return (0L, error.As(err)!);
                } 
                // send at least one normal byte
                if (sockType != SOCK_DGRAM && len(p) == 0L)
                {
                    _addr_iov.Base = _addr_dummy;
                    iov.Base = ref _addr_iov.Base.val;
                    iov.SetLen(1L);

                }

                msg.Control = (byte.val)(@unsafe.Pointer(_addr_oob[0L]));
                msg.SetControllen(len(oob));

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

        private static (long, error) getLen(this ptr<RawSockaddrUnix> _addr_sa)
        {
            long _p0 = default;
            error _p0 = default!;
            ref RawSockaddrUnix sa = ref _addr_sa.val;
 
            // Some versions of AIX have a bug in getsockname (see IV78655).
            // We can't rely on sa.Len being set correctly.
            var n = SizeofSockaddrUnix - 3L; // subtract leading Family, Len, terminating NUL.
            for (long i = 0L; i < n; i++)
            {
                if (sa.Path[i] == 0L)
                {
                    n = i;
                    break;
                }

            }

            return (n, error.As(null!)!);

        }

        private static (Sockaddr, error) anyToSockaddr(ptr<RawSockaddrAny> _addr_rsa)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;


            if (rsa.Addr.Family == AF_UNIX) 
                var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
                ptr<SockaddrUnix> sa = @new<SockaddrUnix>();
                var (n, err) = pp.getLen();
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Path[0L]));
                sa.Name = string(bytes[0L..n]);
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

        public partial struct SockaddrDatalink
        {
            public byte Len;
            public byte Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<byte> Data;
            public RawSockaddrDatalink raw;
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

        /*
         * ptrace
         */

        //sys    Openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)
        //sys    ptrace64(request int, id int64, addr int64, data int, buff uintptr) (err error)

        private static Errno raw_ptrace(long request, long pid, ptr<byte> _addr_addr, ptr<byte> _addr_data)
        {
            ref byte addr = ref _addr_addr.val;
            ref byte data = ref _addr_data.val;

            if (request == PTRACE_TRACEME)
            { 
                // Convert to AIX ptrace call.
                var err = ptrace64(PT_TRACE_ME, 0L, 0L, 0L, 0L);
                if (err != null)
                {
                    return err._<Errno>();
                }

                return 0L;

            }

            return ENOSYS;

        }

        private static (long, error) ptracePeek(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;

            long n = 0L;
            while (len(out) > 0L)
            {
                var bsize = len(out);
                if (bsize > 1024L)
                {
                    bsize = 1024L;
                }

                err = ptrace64(PT_READ_BLOCK, int64(pid), int64(addr), bsize, uintptr(@unsafe.Pointer(_addr_out[0L])));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                addr += uintptr(bsize);
                n += bsize;
                out = out[n..];

            }

            return (n, error.As(null!)!);

        }

        public static (long, error) PtracePeekText(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;

            return ptracePeek(pid, addr, out);
        }

        public static (long, error) PtracePeekData(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;

            return ptracePeek(pid, addr, out);
        }

        private static (long, error) ptracePoke(long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;

            long n = 0L;
            while (len(data) > 0L)
            {
                var bsize = len(data);
                if (bsize > 1024L)
                {
                    bsize = 1024L;
                }

                err = ptrace64(PT_WRITE_BLOCK, int64(pid), int64(addr), bsize, uintptr(@unsafe.Pointer(_addr_data[0L])));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                addr += uintptr(bsize);
                n += bsize;
                data = data[n..];

            }

            return (n, error.As(null!)!);

        }

        public static (long, error) PtracePokeText(long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;

            return ptracePoke(pid, addr, data);
        }

        public static (long, error) PtracePokeData(long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;

            return ptracePoke(pid, addr, data);
        }

        public static error PtraceCont(long pid, long signal)
        {
            error err = default!;

            return error.As(ptrace64(PT_CONTINUE, int64(pid), 1L, signal, 0L))!;
        }

        public static error PtraceSingleStep(long pid)
        {
            error err = default!;

            return error.As(ptrace64(PT_STEP, int64(pid), 1L, 0L, 0L))!;
        }

        public static error PtraceAttach(long pid)
        {
            error err = default!;

            return error.As(ptrace64(PT_ATTACH, int64(pid), 0L, 0L, 0L))!;
        }

        public static error PtraceDetach(long pid)
        {
            error err = default!;

            return error.As(ptrace64(PT_DETACH, int64(pid), 0L, 0L, 0L))!;
        }

        /*
         * Direct access
         */

        //sys    Acct(path string) (err error)
        //sys    Chdir(path string) (err error)
        //sys    Chmod(path string, mode uint32) (err error)
        //sys    Chown(path string, uid int, gid int) (err error)
        //sys    Chroot(path string) (err error)
        //sys    Close(fd int) (err error)
        //sys    Dup(fd int) (nfd int, err error)
        //sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatfs(fd int, buf *Statfs_t) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sys    Fsync(fd int) (err error)
        //sysnb    Getgid() (gid int)
        //sysnb    Getpid() (pid int)
        //sys    Geteuid() (euid int)
        //sys    Getegid() (egid int)
        //sys    Getppid() (ppid int)
        //sys    Getpriority(which int, who int) (n int, err error)
        //sysnb    Getrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Getuid() (uid int)
        //sys    Kill(pid int, signum Signal) (err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
        //sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Reboot(how int) (err error)
        //sys    Rename(from string, to string) (err error)
        //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
        //sys    Rmdir(path string) (err error)
        //sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = lseek
        //sysnb    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Statfs(path string, buf *Statfs_t) (err error)
        //sys    Symlink(path string, link string) (err error)
        //sys    Truncate(path string, length int64) (err error)
        //sys    Umask(newmask int) (oldmask int)
        //sys    Unlink(path string) (err error)
        //sysnb    Uname(buf *Utsname) (err error)
        //sys    write(fd int, p []byte) (n int, err error)

        //sys    gettimeofday(tv *Timeval, tzp *Timezone) (err error)

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:int32(usec));
        }

        private static (long, error) readlen(long fd, ptr<byte> _addr_buf, long nbuf)
        {
            long n = default;
            error err = default!;
            ref byte buf = ref _addr_buf.val;

            var (r0, _, e1) = syscall6(uintptr(@unsafe.Pointer(_addr_libc_read)), 3L, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0L, 0L, 0L);
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        /*
         * Map
         */

        private static ptr<mmapper> mapper = addr(new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,));

        //sys    mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys    munmap(addr uintptr, length uintptr) (err error)

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
    }
}
