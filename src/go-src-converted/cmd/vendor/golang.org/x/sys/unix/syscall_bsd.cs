// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// BSD system call wrappers shared by *BSD based systems
// including OS X (Darwin) and FreeBSD.  Like the other
// syscall_*.go files it is compiled as Go code but also
// used as input to mksyscall which parses the //sys
// lines and generates system call stubs.

// package unix -- go2cs converted at 2020 October 08 04:46:43 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_bsd.go
using runtime = go.runtime_package;
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
            }            return ;

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
        private static readonly long killed = (long)9L;
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

        public static syscall.Signal Signal(this WaitStatus w)
        {
            var sig = syscall.Signal(w & mask);
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
            return w & mask == stopped && syscall.Signal(w >> (int)(shift)) != SIGSTOP;
        }

        public static bool Killed(this WaitStatus w)
        {
            return w & mask == killed && syscall.Signal(w >> (int)(shift)) != SIGKILL;
        }

        public static bool Continued(this WaitStatus w)
        {
            return w & mask == stopped && syscall.Signal(w >> (int)(shift)) == SIGSTOP;
        }

        public static syscall.Signal StopSignal(this WaitStatus w)
        {
            if (!w.Stopped())
            {
                return -1L;
            }

            return syscall.Signal(w >> (int)(shift)) & 0xFFUL;

        }

        public static long TrapCause(this WaitStatus w)
        {
            return -1L;
        }

        //sys    wait4(pid int, wstatus *_C_int, options int, rusage *Rusage) (wpid int, err error)

        public static (long, error) Wait4(long pid, ptr<WaitStatus> _addr_wstatus, long options, ptr<Rusage> _addr_rusage)
        {
            long wpid = default;
            error err = default!;
            ref WaitStatus wstatus = ref _addr_wstatus.val;
            ref Rusage rusage = ref _addr_rusage.val;

            ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
            wpid, err = wait4(pid, _addr_status, options, rusage);
            if (wstatus != null)
            {
                wstatus = WaitStatus(status);
            }

            return ;

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

            sa.raw.Len = SizeofSockaddrInet4;
            sa.raw.Family = AF_INET;
            ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);

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

            sa.raw.Len = SizeofSockaddrInet6;
            sa.raw.Family = AF_INET6;
            ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrUnix> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrUnix sa = ref _addr_sa.val;

            var name = sa.Name;
            var n = len(name);
            if (n >= len(sa.raw.Path) || n == 0L)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Len = byte(3L + n); // 2 for Family, Len; 1 for NUL
            sa.raw.Family = AF_UNIX;
            for (long i = 0L; i < n; i++)
            {
                sa.raw.Path[i] = int8(name[i]);
            }

            return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrDatalink> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrDatalink sa = ref _addr_sa.val;

            if (sa.Index == 0L)
            {
                return (null, 0L, error.As(EINVAL)!);
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

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrDatalink, error.As(null!)!);

        }

        private static (Sockaddr, error) anyToSockaddr(long fd, ptr<RawSockaddrAny> _addr_rsa)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;


            if (rsa.Addr.Family == AF_LINK) 
                var pp = (RawSockaddrDatalink.val)(@unsafe.Pointer(rsa));
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
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_UNIX) 
                pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
                if (pp.Len < 2L || pp.Len > SizeofSockaddrUnix)
                {
                    return (null, error.As(EINVAL)!);
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

            if (runtime.GOOS == "darwin" && len == 0L)
            { 
                // Accepted socket has no address.
                // This is likely due to a bug in xnu kernels,
                // where instead of ECONNABORTED error socket
                // is accepted, but has no address.
                Close(nfd);
                return (0L, null, error.As(ECONNABORTED)!);

            }

            sa, err = anyToSockaddr(fd, _addr_rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }

            return ;

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
            // TODO(jsing): DragonFly has a "bug" (see issue 3349), which should be
            // reported upstream.
            if (runtime.GOOS == "dragonfly" && rsa.Addr.Family == AF_UNSPEC && rsa.Addr.Len == 0L)
            {
                rsa.Addr.Family = AF_UNIX;
                rsa.Addr.Len = SizeofSockaddrUnix;
            }

            return anyToSockaddr(fd, _addr_rsa);

        }

        //sysnb socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)

        // GetsockoptString returns the string value of the socket option opt for the
        // socket associated with fd at the given socket level.
        public static (@string, error) GetsockoptString(long fd, long level, long opt)
        {
            @string _p0 = default;
            error _p0 = default!;

            var buf = make_slice<byte>(256L);
            ref var vallen = ref heap(_Socklen(len(buf)), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_buf[0L]), _addr_vallen);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (string(buf[..vallen - 1L]), error.As(null!)!);

        }

        //sys   recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
        //sys   sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error)

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
                // receive at least one normal byte
                if (len(p) == 0L)
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
                from, err = anyToSockaddr(fd, _addr_rsa);
            }

            return ;

        }

        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error)

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
                // send at least one normal byte
                if (len(p) == 0L)
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

        //sys    kevent(kq int, change unsafe.Pointer, nchange int, event unsafe.Pointer, nevent int, timeout *Timespec) (n int, err error)

        public static (long, error) Kevent(long kq, slice<Kevent_t> changes, slice<Kevent_t> events, ptr<Timespec> _addr_timeout)
        {
            long n = default;
            error err = default!;
            ref Timespec timeout = ref _addr_timeout.val;

            unsafe.Pointer change = default;            unsafe.Pointer @event = default;

            if (len(changes) > 0L)
            {
                change = @unsafe.Pointer(_addr_changes[0L]);
            }

            if (len(events) > 0L)
            {
                event = @unsafe.Pointer(_addr_events[0L]);
            }

            return kevent(kq, change, len(changes), event, len(events), timeout);

        }

        // sysctlmib translates name to mib number and appends any additional args.
        private static (slice<_C_int>, error) sysctlmib(@string name, params long[] args)
        {
            slice<_C_int> _p0 = default;
            error _p0 = default!;
            args = args.Clone();
 
            // Translate name to mib number.
            var (mib, err) = nametomib(name);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            foreach (var (_, a) in args)
            {
                mib = append(mib, _C_int(a));
            }
            return (mib, error.As(null!)!);

        }

        public static (@string, error) Sysctl(@string name)
        {
            @string _p0 = default;
            error _p0 = default!;

            return SysctlArgs(name);
        }

        public static (@string, error) SysctlArgs(@string name, params long[] args)
        {
            @string _p0 = default;
            error _p0 = default!;
            args = args.Clone();

            var (buf, err) = SysctlRaw(name, args);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var n = len(buf); 

            // Throw away terminating NUL.
            if (n > 0L && buf[n - 1L] == '\x00')
            {
                n--;
            }

            return (string(buf[0L..n]), error.As(null!)!);

        }

        public static (uint, error) SysctlUint32(@string name)
        {
            uint _p0 = default;
            error _p0 = default!;

            return SysctlUint32Args(name);
        }

        public static (uint, error) SysctlUint32Args(@string name, params long[] args)
        {
            uint _p0 = default;
            error _p0 = default!;
            args = args.Clone();

            var (mib, err) = sysctlmib(name, args);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            ref var n = ref heap(uintptr(4L), out ptr<var> _addr_n);
            var buf = make_slice<byte>(4L);
            {
                var err = sysctl(mib, _addr_buf[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            if (n != 4L)
            {
                return (0L, error.As(EIO)!);
            }

            return (new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_buf[0L])), error.As(null!)!);

        }

        public static (ulong, error) SysctlUint64(@string name, params long[] args)
        {
            ulong _p0 = default;
            error _p0 = default!;
            args = args.Clone();

            var (mib, err) = sysctlmib(name, args);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            ref var n = ref heap(uintptr(8L), out ptr<var> _addr_n);
            var buf = make_slice<byte>(8L);
            {
                var err = sysctl(mib, _addr_buf[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            if (n != 8L)
            {
                return (0L, error.As(EIO)!);
            }

            return (new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_buf[0L])), error.As(null!)!);

        }

        public static (slice<byte>, error) SysctlRaw(@string name, params long[] args)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            args = args.Clone();

            var (mib, err) = sysctlmib(name, args);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // Find size.
            ref var n = ref heap(uintptr(0L), out ptr<var> _addr_n);
            {
                var err__prev1 = err;

                var err = sysctl(mib, null, _addr_n, null, 0L);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            if (n == 0L)
            {
                return (null, error.As(null!)!);
            } 

            // Read into buffer of that size.
            var buf = make_slice<byte>(n);
            {
                var err__prev1 = err;

                err = sysctl(mib, _addr_buf[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return (null, error.As(err)!);
                } 

                // The actual call may return less than the original reported required
                // size so ensure we deal with that.

                err = err__prev1;

            } 

            // The actual call may return less than the original reported required
            // size so ensure we deal with that.
            return (buf[..n], error.As(null!)!);

        }

        public static (ptr<Clockinfo>, error) SysctlClockinfo(@string name)
        {
            ptr<Clockinfo> _p0 = default!;
            error _p0 = default!;

            var (mib, err) = sysctlmib(name);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ref var n = ref heap(uintptr(SizeofClockinfo), out ptr<var> _addr_n);
            ref Clockinfo ci = ref heap(out ptr<Clockinfo> _addr_ci);
            {
                var err = sysctl(mib, (byte.val)(@unsafe.Pointer(_addr_ci)), _addr_n, null, 0L);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            if (n != SizeofClockinfo)
            {
                return (_addr_null!, error.As(EIO)!);
            }

            return (_addr__addr_ci!, error.As(null!)!);

        }

        //sys    utimes(path string, timeval *[2]Timeval) (err error)

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (tv == null)
            {
                return error.As(utimes(path, null))!;
            }

            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (ts == null)
            {
                var err = utimensat(AT_FDCWD, path, null, 0L);
                if (err != ENOSYS)
                {
                    return error.As(err)!;
                }

                return error.As(utimes(path, null))!;

            }

            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            } 
            // Darwin setattrlist can set nanosecond timestamps
            err = setattrlistTimes(path, ts, 0L);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            }

            err = utimensat(AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            } 
            // Not as efficient as it could be because Timespec and
            // Timeval have different types in the different OSes
            array<Timeval> tv = new array<Timeval>(new Timeval[] { NsecToTimeval(TimespecToNsec(ts[0])), NsecToTimeval(TimespecToNsec(ts[1])) });
            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

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

            var err = setattrlistTimes(path, ts, flags);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            }

            return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), flags))!;

        }

        //sys    futimes(fd int, timeval *[2]Timeval) (err error)

        public static error Futimes(long fd, slice<Timeval> tv)
        {
            if (tv == null)
            {
                return error.As(futimes(fd, null))!;
            }

            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(futimes(fd, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        //sys   poll(fds *PollFd, nfds int, timeout int) (n int, err error)

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

        // TODO: wrap
        //    Acct(name nil-string) (err error)
        //    Gethostuuid(uuid *byte, timeout *Timespec) (err error)
        //    Ptrace(req int, pid int, addr uintptr, data int) (ret uintptr, err error)

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

        //sys    Madvise(b []byte, behav int) (err error)
        //sys    Mlock(b []byte) (err error)
        //sys    Mlockall(flags int) (err error)
        //sys    Mprotect(b []byte, prot int) (err error)
        //sys    Msync(b []byte, flags int) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Munlockall() (err error)
    }
}}}}}}
