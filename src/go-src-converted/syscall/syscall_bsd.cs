// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// BSD system call wrappers shared by *BSD based systems
// including OS X (Darwin) and FreeBSD.  Like the other
// syscall_*.go files it is compiled as Go code but also
// used as input to mksyscall which parses the //sys
// lines and generates system call stubs.

// package syscall -- go2cs converted at 2020 August 29 08:37:44 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_bsd.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class syscall_package
    {
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
            }            return;
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
            // 64 bits should be enough. (32 bits isn't even on 386). Since the
            // actual system call is getdirentries64, 64 is a good guess.
            // TODO(rsc): Can we use a single global basep for all calls?
            var @base = (uintptr.Value)(@unsafe.Pointer(@new<uint64>()));
            return Getdirentries(fd, buf, base);
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

        //sys    wait4(pid int, wstatus *_C_int, options int, rusage *Rusage) (wpid int, err error)

        public static (long, error) Wait4(long pid, ref WaitStatus wstatus, long options, ref Rusage rusage)
        {
            _C_int status = default;
            wpid, err = wait4(pid, ref status, options, rusage);
            if (wstatus != null)
            {
                wstatus.Value = WaitStatus(status);
            }
            return;
        }

        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)
        //sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sysnb    socket(domain int, typ int, proto int) (fd int, err error)
        //sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
        //sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
        //sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sysnb    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sys    Shutdown(s int, how int) (err error)

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrInet4 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Len = SizeofSockaddrInet4;
            sa.raw.Family = AF_INET;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), _Socklen(sa.raw.Len), null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrInet6 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Len = SizeofSockaddrInet6;
            sa.raw.Family = AF_INET6;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), _Socklen(sa.raw.Len), null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrUnix sa)
        {
            var name = sa.Name;
            var n = len(name);
            if (n >= len(sa.raw.Path) || n == 0L)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Len = byte(3L + n); // 2 for Family, Len; 1 for NUL
            sa.raw.Family = AF_UNIX;
            for (long i = 0L; i < n; i++)
            {
                sa.raw.Path[i] = int8(name[i]);
            }

            return (@unsafe.Pointer(ref sa.raw), _Socklen(sa.raw.Len), null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrDatalink sa)
        {
            if (sa.Index == 0L)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Len = sa.Len;
            sa.raw.Family = AF_LINK;
            sa.raw.Index = sa.Index;
            sa.raw.Type = sa.Type;
            sa.raw.Nlen = sa.Nlen;
            sa.raw.Alen = sa.Alen;
            sa.raw.Slen = sa.Slen;
            for (long i = 0L; i < len(sa.raw.Data); i++)
            {
                sa.raw.Data[i] = sa.Data[i];
            }

            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrDatalink, null);
        }

        private static (Sockaddr, error) anyToSockaddr(ref RawSockaddrAny rsa)
        {

            if (rsa.Addr.Family == AF_LINK) 
                var pp = (RawSockaddrDatalink.Value)(@unsafe.Pointer(rsa));
                ptr<SockaddrDatalink> sa = @new<SockaddrDatalink>();
                sa.Len = pp.Len;
                sa.Family = pp.Family;
                sa.Index = pp.Index;
                sa.Type = pp.Type;
                sa.Nlen = pp.Nlen;
                sa.Alen = pp.Alen;
                sa.Slen = pp.Slen;
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(sa.Data); i++)
                    {
                        sa.Data[i] = pp.Data[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
            else if (rsa.Addr.Family == AF_UNIX) 
                pp = (RawSockaddrUnix.Value)(@unsafe.Pointer(rsa));
                if (pp.Len < 2L || pp.Len > SizeofSockaddrUnix)
                {
                    return (null, EINVAL);
                }
                sa = @new<SockaddrUnix>(); 

                // Some BSDs include the trailing NUL in the length, whereas
                // others do not. Work around this by subtracting the leading
                // family and len. The path is then scanned to see if a NUL
                // terminator still exists within the length.
                var n = int(pp.Len) - 2L; // subtract leading Family, Len
                {
                    long i__prev1 = i;

                    for (i = 0L; i < n; i++)
                    {
                        if (pp.Path[i] == 0L)
                        { 
                            // found early NUL; assume Len included the NUL
                            // or was overestimating.
                            n = i;
                            break;
                        }
                    }


                    i = i__prev1;
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

                    for (i = 0L; i < len(sa.Addr); i++)
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

        public static (long, Sockaddr, error) Accept(long fd)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            nfd, err = accept(fd, ref rsa, ref len);
            if (err != null)
            {
                return;
            }
            if (runtime.GOOS == "darwin" && len == 0L)
            { 
                // Accepted socket has no address.
                // This is likely due to a bug in xnu kernels,
                // where instead of ECONNABORTED error socket
                // is accepted, but has no address.
                Close(nfd);
                return (0L, null, ECONNABORTED);
            }
            sa, err = anyToSockaddr(ref rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }
            return;
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
            // TODO(jsing): DragonFly has a "bug" (see issue 3349), which should be
            // reported upstream.
            if (runtime.GOOS == "dragonfly" && rsa.Addr.Family == AF_UNSPEC && rsa.Addr.Len == 0L)
            {
                rsa.Addr.Family = AF_UNIX;
                rsa.Addr.Len = SizeofSockaddrUnix;
            }
            return anyToSockaddr(ref rsa);
        }

        //sysnb socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)

        public static (byte, error) GetsockoptByte(long fd, long level, long opt)
        {
            byte n = default;
            var vallen = _Socklen(1L);
            err = getsockopt(fd, level, opt, @unsafe.Pointer(ref n), ref vallen);
            return (n, err);
        }

        public static (array<byte>, error) GetsockoptInet4Addr(long fd, long level, long opt)
        {
            var vallen = _Socklen(4L);
            err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value[0L]), ref vallen);
            return (value, err);
        }

        public static (ref IPMreq, error) GetsockoptIPMreq(long fd, long level, long opt)
        {
            IPMreq value = default;
            var vallen = _Socklen(SizeofIPMreq);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref IPv6Mreq, error) GetsockoptIPv6Mreq(long fd, long level, long opt)
        {
            IPv6Mreq value = default;
            var vallen = _Socklen(SizeofIPv6Mreq);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref IPv6MTUInfo, error) GetsockoptIPv6MTUInfo(long fd, long level, long opt)
        {
            IPv6MTUInfo value = default;
            var vallen = _Socklen(SizeofIPv6MTUInfo);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref ICMPv6Filter, error) GetsockoptICMPv6Filter(long fd, long level, long opt)
        {
            ICMPv6Filter value = default;
            var vallen = _Socklen(SizeofICMPv6Filter);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        //sys   recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
        //sys   sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error)

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            Msghdr msg = default;
            RawSockaddrAny rsa = default;
            msg.Name = (byte.Value)(@unsafe.Pointer(ref rsa));
            msg.Namelen = uint32(SizeofSockaddrAny);
            Iovec iov = default;
            if (len(p) > 0L)
            {
                iov.Base = (byte.Value)(@unsafe.Pointer(ref p[0L]));
                iov.SetLen(len(p));
            }
            byte dummy = default;
            if (len(oob) > 0L)
            { 
                // receive at least one normal byte
                if (len(p) == 0L)
                {
                    iov.Base = ref dummy;
                    iov.SetLen(1L);
                }
                msg.Control = (byte.Value)(@unsafe.Pointer(ref oob[0L]));
                msg.SetControllen(len(oob));
            }
            msg.Iov = ref iov;
            msg.Iovlen = 1L;
            n, err = recvmsg(fd, ref msg, flags);

            if (err != null)
            {
                return;
            }
            oobn = int(msg.Controllen);
            recvflags = int(msg.Flags); 
            // source address is only specified if the socket is unconnected
            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(ref rsa);
            }
            return;
        }

        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error)

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            _, err = SendmsgN(fd, p, oob, to, flags);
            return;
        }

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
                iov.Base = (byte.Value)(@unsafe.Pointer(ref p[0L]));
                iov.SetLen(len(p));
            }
            byte dummy = default;
            if (len(oob) > 0L)
            { 
                // send at least one normal byte
                if (len(p) == 0L)
                {
                    iov.Base = ref dummy;
                    iov.SetLen(1L);
                }
                msg.Control = (byte.Value)(@unsafe.Pointer(ref oob[0L]));
                msg.SetControllen(len(oob));
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

        //sys    kevent(kq int, change unsafe.Pointer, nchange int, event unsafe.Pointer, nevent int, timeout *Timespec) (n int, err error)

        public static (long, error) Kevent(long kq, slice<Kevent_t> changes, slice<Kevent_t> events, ref Timespec timeout)
        {
            unsafe.Pointer change = default;            unsafe.Pointer @event = default;

            if (len(changes) > 0L)
            {
                change = @unsafe.Pointer(ref changes[0L]);
            }
            if (len(events) > 0L)
            {
                event = @unsafe.Pointer(ref events[0L]);
            }
            return kevent(kq, change, len(changes), event, len(events), timeout);
        }

        //sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error) = SYS___SYSCTL

        public static (@string, error) Sysctl(@string name)
        { 
            // Translate name to mib number.
            var (mib, err) = nametomib(name);
            if (err != null)
            {
                return ("", err);
            } 

            // Find size.
            var n = uintptr(0L);
            err = sysctl(mib, null, ref n, null, 0L);

            if (err != null)
            {
                return ("", err);
            }
            if (n == 0L)
            {
                return ("", null);
            } 

            // Read into buffer of that size.
            var buf = make_slice<byte>(n);
            err = sysctl(mib, ref buf[0L], ref n, null, 0L);

            if (err != null)
            {
                return ("", err);
            } 

            // Throw away terminating NUL.
            if (n > 0L && buf[n - 1L] == '\x00')
            {
                n--;
            }
            return (string(buf[0L..n]), null);
        }

        public static (uint, error) SysctlUint32(@string name)
        { 
            // Translate name to mib number.
            var (mib, err) = nametomib(name);
            if (err != null)
            {
                return (0L, err);
            } 

            // Read into buffer of that size.
            var n = uintptr(4L);
            var buf = make_slice<byte>(4L);
            err = sysctl(mib, ref buf[0L], ref n, null, 0L);

            if (err != null)
            {
                return (0L, err);
            }
            if (n != 4L)
            {
                return (0L, EIO);
            }
            return (@unsafe.Pointer(ref buf[0L]).Value, null);
        }

        //sys    utimes(path string, timeval *[2]Timeval) (err error)

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL);
            }
            return error.As(utimes(path, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL);
            } 
            // Darwin setattrlist can set nanosecond timestamps
            var err = setattrlistTimes(path, ts);
            if (err != ENOSYS)
            {
                return error.As(err);
            }
            err = utimensat(_AT_FDCWD, path, new ptr<ref array<Timespec>>(@unsafe.Pointer(ref ts[0L])), 0L);
            if (err != ENOSYS)
            {
                return error.As(err);
            } 
            // Not as efficient as it could be because Timespec and
            // Timeval have different types in the different OSes
            array<Timeval> tv = new array<Timeval>(new Timeval[] { NsecToTimeval(TimespecToNsec(ts[0])), NsecToTimeval(TimespecToNsec(ts[1])) });
            return error.As(utimes(path, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }

        //sys    futimes(fd int, timeval *[2]Timeval) (err error)

        public static error Futimes(long fd, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL);
            }
            return error.As(futimes(fd, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }

        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)

        // TODO: wrap
        //    Acct(name nil-string) (err error)
        //    Gethostuuid(uuid *byte, timeout *Timespec) (err error)
        //    Madvise(addr *byte, len int, behav int) (err error)
        //    Mprotect(addr *byte, len int, prot int) (err error)
        //    Msync(addr *byte, len int, flags int) (err error)
        //    Ptrace(req int, pid int, addr uintptr, data int) (ret uintptr, err error)

        private static mmapper mapper = ref new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,);

        public static (slice<byte>, error) Mmap(long fd, long offset, long length, long prot, long flags)
        {
            return mapper.Mmap(fd, offset, length, prot, flags);
        }

        public static error Munmap(slice<byte> b)
        {
            return error.As(mapper.Munmap(b));
        }
    }
}
