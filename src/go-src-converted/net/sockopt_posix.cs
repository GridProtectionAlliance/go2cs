// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 October 09 04:52:19 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockopt_posix.go
using bytealg = go.@internal.bytealg_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // Boolean to int.
        private static long boolint(bool b)
        {
            if (b)
            {
                return 1L;
            }
            return 0L;

        }

        private static (ptr<Interface>, error) ipv4AddrToInterface(IP ip)
        {
            ptr<Interface> _p0 = default!;
            error _p0 = default!;

            var (ift, err) = Interfaces();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            foreach (var (_, ifi) in ift)
            {
                var (ifat, err) = ifi.Addrs();
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                foreach (var (_, ifa) in ifat)
                {
                    switch (ifa.type())
                    {
                        case ptr<IPAddr> v:
                            if (ip.Equal(v.IP))
                            {
                                return (_addr__addr_ifi!, error.As(null!)!);
                            }

                            break;
                        case ptr<IPNet> v:
                            if (ip.Equal(v.IP))
                            {
                                return (_addr__addr_ifi!, error.As(null!)!);
                            }

                            break;
                    }

                }

            }
            if (ip.Equal(IPv4zero))
            {
                return (_addr_null!, error.As(null!)!);
            }

            return (_addr_null!, error.As(errNoSuchInterface)!);

        }

        private static (IP, error) interfaceToIPv4Addr(ptr<Interface> _addr_ifi)
        {
            IP _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            if (ifi == null)
            {
                return (IPv4zero, error.As(null!)!);
            }

            var (ifat, err) = ifi.Addrs();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            foreach (var (_, ifa) in ifat)
            {
                switch (ifa.type())
                {
                    case ptr<IPAddr> v:
                        if (v.IP.To4() != null)
                        {
                            return (v.IP, error.As(null!)!);
                        }

                        break;
                    case ptr<IPNet> v:
                        if (v.IP.To4() != null)
                        {
                            return (v.IP, error.As(null!)!);
                        }

                        break;
                }

            }
            return (null, error.As(errNoSuchInterface)!);

        }

        private static error setIPv4MreqToInterface(ptr<syscall.IPMreq> _addr_mreq, ptr<Interface> _addr_ifi)
        {
            ref syscall.IPMreq mreq = ref _addr_mreq.val;
            ref Interface ifi = ref _addr_ifi.val;

            if (ifi == null)
            {
                return error.As(null!)!;
            }

            var (ifat, err) = ifi.Addrs();
            if (err != null)
            {
                return error.As(err)!;
            }

            foreach (var (_, ifa) in ifat)
            {
                switch (ifa.type())
                {
                    case ptr<IPAddr> v:
                        {
                            var a__prev1 = a;

                            var a = v.IP.To4();

                            if (a != null)
                            {
                                copy(mreq.Interface[..], a);
                                goto done;
                            }

                            a = a__prev1;

                        }

                        break;
                    case ptr<IPNet> v:
                        {
                            var a__prev1 = a;

                            a = v.IP.To4();

                            if (a != null)
                            {
                                copy(mreq.Interface[..], a);
                                goto done;
                            }

                            a = a__prev1;

                        }

                        break;
                }

            }
done:
            if (bytealg.Equal(mreq.Multiaddr[..], IPv4zero.To4()))
            {
                return error.As(errNoSuchMulticastInterface)!;
            }

            return error.As(null!)!;

        }

        private static error setReadBuffer(ptr<netFD> _addr_fd, long bytes)
        {
            ref netFD fd = ref _addr_fd.val;

            var err = fd.pfd.SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_RCVBUF, bytes);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;
        }

        private static error setWriteBuffer(ptr<netFD> _addr_fd, long bytes)
        {
            ref netFD fd = ref _addr_fd.val;

            var err = fd.pfd.SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_SNDBUF, bytes);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;
        }

        private static error setKeepAlive(ptr<netFD> _addr_fd, bool keepalive)
        {
            ref netFD fd = ref _addr_fd.val;

            var err = fd.pfd.SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_KEEPALIVE, boolint(keepalive));
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;
        }

        private static error setLinger(ptr<netFD> _addr_fd, long sec)
        {
            ref netFD fd = ref _addr_fd.val;

            ref syscall.Linger l = ref heap(out ptr<syscall.Linger> _addr_l);
            if (sec >= 0L)
            {
                l.Onoff = 1L;
                l.Linger = int32(sec);
            }
            else
            {
                l.Onoff = 0L;
                l.Linger = 0L;
            }

            var err = fd.pfd.SetsockoptLinger(syscall.SOL_SOCKET, syscall.SO_LINGER, _addr_l);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;

        }
    }
}
