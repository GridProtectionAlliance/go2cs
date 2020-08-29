// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:27:31 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockopt_posix.go
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

        private static (ref Interface, error) ipv4AddrToInterface(IP ip)
        {
            var (ift, err) = Interfaces();
            if (err != null)
            {
                return (null, err);
            }
            foreach (var (_, ifi) in ift)
            {
                var (ifat, err) = ifi.Addrs();
                if (err != null)
                {
                    return (null, err);
                }
                foreach (var (_, ifa) in ifat)
                {
                    switch (ifa.type())
                    {
                        case ref IPAddr v:
                            if (ip.Equal(v.IP))
                            {
                                return (ref ifi, null);
                            }
                            break;
                        case ref IPNet v:
                            if (ip.Equal(v.IP))
                            {
                                return (ref ifi, null);
                            }
                            break;
                    }
                }
            }
            if (ip.Equal(IPv4zero))
            {
                return (null, null);
            }
            return (null, errNoSuchInterface);
        }

        private static (IP, error) interfaceToIPv4Addr(ref Interface ifi)
        {
            if (ifi == null)
            {
                return (IPv4zero, null);
            }
            var (ifat, err) = ifi.Addrs();
            if (err != null)
            {
                return (null, err);
            }
            foreach (var (_, ifa) in ifat)
            {
                switch (ifa.type())
                {
                    case ref IPAddr v:
                        if (v.IP.To4() != null)
                        {
                            return (v.IP, null);
                        }
                        break;
                    case ref IPNet v:
                        if (v.IP.To4() != null)
                        {
                            return (v.IP, null);
                        }
                        break;
                }
            }
            return (null, errNoSuchInterface);
        }

        private static error setIPv4MreqToInterface(ref syscall.IPMreq mreq, ref Interface ifi)
        {
            if (ifi == null)
            {
                return error.As(null);
            }
            var (ifat, err) = ifi.Addrs();
            if (err != null)
            {
                return error.As(err);
            }
            foreach (var (_, ifa) in ifat)
            {
                switch (ifa.type())
                {
                    case ref IPAddr v:
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
                    case ref IPNet v:
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
            if (bytesEqual(mreq.Multiaddr[..], IPv4zero.To4()))
            {
                return error.As(errNoSuchMulticastInterface);
            }
            return error.As(null);
        }

        private static error setReadBuffer(ref netFD fd, long bytes)
        {
            var err = fd.pfd.SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_RCVBUF, bytes);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error setWriteBuffer(ref netFD fd, long bytes)
        {
            var err = fd.pfd.SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_SNDBUF, bytes);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error setKeepAlive(ref netFD fd, bool keepalive)
        {
            var err = fd.pfd.SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_KEEPALIVE, boolint(keepalive));
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error setLinger(ref netFD fd, long sec)
        {
            syscall.Linger l = default;
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
            var err = fd.pfd.SetsockoptLinger(syscall.SOL_SOCKET, syscall.SO_LINGER, ref l);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }
    }
}
