// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:28:12 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\unixsock_posix.go
using context = go.context_package;
using errors = go.errors_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        private static (ref netFD, error) unixSocket(context.Context ctx, @string net, sockaddr laddr, sockaddr raddr, @string mode)
        {
            long sotype = default;
            switch (net)
            {
                case "unix": 
                    sotype = syscall.SOCK_STREAM;
                    break;
                case "unixgram": 
                    sotype = syscall.SOCK_DGRAM;
                    break;
                case "unixpacket": 
                    sotype = syscall.SOCK_SEQPACKET;
                    break;
                default: 
                    return (null, UnknownNetworkError(net));
                    break;
            }

            switch (mode)
            {
                case "dial": 
                    if (laddr != null && laddr.isWildcard())
                    {
                        laddr = null;
                    }
                    if (raddr != null && raddr.isWildcard())
                    {
                        raddr = null;
                    }
                    if (raddr == null && (sotype != syscall.SOCK_DGRAM || laddr == null))
                    {
                        return (null, errMissingAddress);
                    }
                    break;
                case "listen": 
                    break;
                default: 
                    return (null, errors.New("unknown mode: " + mode));
                    break;
            }

            var (fd, err) = socket(ctx, net, syscall.AF_UNIX, sotype, 0L, false, laddr, raddr);
            if (err != null)
            {
                return (null, err);
            }
            return (fd, null);
        }

        private static Addr sockaddrToUnix(syscall.Sockaddr sa)
        {
            {
                ref syscall.SockaddrUnix (s, ok) = sa._<ref syscall.SockaddrUnix>();

                if (ok)
                {
                    return ref new UnixAddr(Name:s.Name,Net:"unix");
                }

            }
            return null;
        }

        private static Addr sockaddrToUnixgram(syscall.Sockaddr sa)
        {
            {
                ref syscall.SockaddrUnix (s, ok) = sa._<ref syscall.SockaddrUnix>();

                if (ok)
                {
                    return ref new UnixAddr(Name:s.Name,Net:"unixgram");
                }

            }
            return null;
        }

        private static Addr sockaddrToUnixpacket(syscall.Sockaddr sa)
        {
            {
                ref syscall.SockaddrUnix (s, ok) = sa._<ref syscall.SockaddrUnix>();

                if (ok)
                {
                    return ref new UnixAddr(Name:s.Name,Net:"unixpacket");
                }

            }
            return null;
        }

        private static @string sotypeToNet(long sotype) => func((_, panic, __) =>
        {

            if (sotype == syscall.SOCK_STREAM) 
                return "unix";
            else if (sotype == syscall.SOCK_DGRAM) 
                return "unixgram";
            else if (sotype == syscall.SOCK_SEQPACKET) 
                return "unixpacket";
            else 
                panic("sotypeToNet unknown socket type");
                    });

        private static long family(this ref UnixAddr a)
        {
            return syscall.AF_UNIX;
        }

        private static (syscall.Sockaddr, error) sockaddr(this ref UnixAddr a, long family)
        {
            if (a == null)
            {
                return (null, null);
            }
            return (ref new syscall.SockaddrUnix(Name:a.Name), null);
        }

        private static sockaddr toLocal(this ref UnixAddr a, @string net)
        {
            return a;
        }

        private static (long, ref UnixAddr, error) readFrom(this ref UnixConn c, slice<byte> b)
        {
            ref UnixAddr addr = default;
            var (n, sa, err) = c.fd.readFrom(b);
            switch (sa.type())
            {
                case ref syscall.SockaddrUnix sa:
                    if (sa.Name != "")
                    {
                        addr = ref new UnixAddr(Name:sa.Name,Net:sotypeToNet(c.fd.sotype));
                    }
                    break;
            }
            return (n, addr, err);
        }

        private static (long, long, long, ref UnixAddr, error) readMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob)
        {
            syscall.Sockaddr sa = default;
            n, oobn, flags, sa, err = c.fd.readMsg(b, oob);
            switch (sa.type())
            {
                case ref syscall.SockaddrUnix sa:
                    if (sa.Name != "")
                    {
                        addr = ref new UnixAddr(Name:sa.Name,Net:sotypeToNet(c.fd.sotype));
                    }
                    break;
            }
            return;
        }

        private static (long, error) writeTo(this ref UnixConn c, slice<byte> b, ref UnixAddr addr)
        {
            if (c.fd.isConnected)
            {
                return (0L, ErrWriteToConnected);
            }
            if (addr == null)
            {
                return (0L, errMissingAddress);
            }
            if (addr.Net != sotypeToNet(c.fd.sotype))
            {
                return (0L, syscall.EAFNOSUPPORT);
            }
            syscall.SockaddrUnix sa = ref new syscall.SockaddrUnix(Name:addr.Name);
            return c.fd.writeTo(b, sa);
        }

        private static (long, long, error) writeMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob, ref UnixAddr addr)
        {
            if (c.fd.sotype == syscall.SOCK_DGRAM && c.fd.isConnected)
            {
                return (0L, 0L, ErrWriteToConnected);
            }
            syscall.Sockaddr sa = default;
            if (addr != null)
            {
                if (addr.Net != sotypeToNet(c.fd.sotype))
                {
                    return (0L, 0L, syscall.EAFNOSUPPORT);
                }
                sa = ref new syscall.SockaddrUnix(Name:addr.Name);
            }
            return c.fd.writeMsg(b, oob, sa);
        }

        private static (ref UnixConn, error) dialUnix(context.Context ctx, @string net, ref UnixAddr laddr, ref UnixAddr raddr)
        {
            var (fd, err) = unixSocket(ctx, net, laddr, raddr, "dial");
            if (err != null)
            {
                return (null, err);
            }
            return (newUnixConn(fd), null);
        }

        private static (ref UnixConn, error) accept(this ref UnixListener ln)
        {
            var (fd, err) = ln.fd.accept();
            if (err != null)
            {
                return (null, err);
            }
            return (newUnixConn(fd), null);
        }

        private static error close(this ref UnixListener ln)
        { 
            // The operating system doesn't clean up
            // the file that announcing created, so
            // we have to clean it up ourselves.
            // There's a race here--we can't know for
            // sure whether someone else has come along
            // and replaced our socket name already--
            // but this sequence (remove then close)
            // is at least compatible with the auto-remove
            // sequence in ListenUnix. It's only non-Go
            // programs that can mess us up.
            // Even if there are racy calls to Close, we want to unlink only for the first one.
            ln.unlinkOnce.Do(() =>
            {
                if (ln.path[0L] != '@' && ln.unlink)
                {
                    syscall.Unlink(ln.path);
                }
            });
            return error.As(ln.fd.Close());
        }

        private static (ref os.File, error) file(this ref UnixListener ln)
        {
            var (f, err) = ln.fd.dup();
            if (err != null)
            {
                return (null, err);
            }
            return (f, null);
        }

        // SetUnlinkOnClose sets whether the underlying socket file should be removed
        // from the file system when the listener is closed.
        //
        // The default behavior is to unlink the socket file only when package net created it.
        // That is, when the listener and the underlying socket file were created by a call to
        // Listen or ListenUnix, then by default closing the listener will remove the socket file.
        // but if the listener was created by a call to FileListener to use an already existing
        // socket file, then by default closing the listener will not remove the socket file.
        private static void SetUnlinkOnClose(this ref UnixListener l, bool unlink)
        {
            l.unlink = unlink;
        }

        private static (ref UnixListener, error) listenUnix(context.Context ctx, @string network, ref UnixAddr laddr)
        {
            var (fd, err) = unixSocket(ctx, network, laddr, null, "listen");
            if (err != null)
            {
                return (null, err);
            }
            return (ref new UnixListener(fd:fd,path:fd.laddr.String(),unlink:true), null);
        }

        private static (ref UnixConn, error) listenUnixgram(context.Context ctx, @string network, ref UnixAddr laddr)
        {
            var (fd, err) = unixSocket(ctx, network, laddr, null, "listen");
            if (err != null)
            {
                return (null, err);
            }
            return (newUnixConn(fd), null);
        }
    }
}
