// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:28:00 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsock_posix.go
using context = go.context_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static Addr sockaddrToTCP(syscall.Sockaddr sa)
        {
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    return ref new TCPAddr(IP:sa.Addr[0:],Port:sa.Port);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    return ref new TCPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return null;
        }

        private static long family(this ref TCPAddr a)
        {
            if (a == null || len(a.IP) <= IPv4len)
            {
                return syscall.AF_INET;
            }
            if (a.IP.To4() != null)
            {
                return syscall.AF_INET;
            }
            return syscall.AF_INET6;
        }

        private static (syscall.Sockaddr, error) sockaddr(this ref TCPAddr a, long family)
        {
            if (a == null)
            {
                return (null, null);
            }
            return ipToSockaddr(family, a.IP, a.Port, a.Zone);
        }

        private static sockaddr toLocal(this ref TCPAddr a, @string net)
        {
            return ref new TCPAddr(loopbackIP(net),a.Port,a.Zone);
        }

        private static (long, error) readFrom(this ref TCPConn c, io.Reader r)
        {
            {
                var (n, err, handled) = sendFile(c.fd, r);

                if (handled)
                {
                    return (n, err);
                }

            }
            return genericReadFrom(c, r);
        }

        private static (ref TCPConn, error) dialTCP(context.Context ctx, @string net, ref TCPAddr laddr, ref TCPAddr raddr)
        {
            if (testHookDialTCP != null)
            {
                return testHookDialTCP(ctx, net, laddr, raddr);
            }
            return doDialTCP(ctx, net, laddr, raddr);
        }

        private static (ref TCPConn, error) doDialTCP(context.Context ctx, @string net, ref TCPAddr laddr, ref TCPAddr raddr)
        {
            var (fd, err) = internetSocket(ctx, net, laddr, raddr, syscall.SOCK_STREAM, 0L, "dial"); 

            // TCP has a rarely used mechanism called a 'simultaneous connection' in
            // which Dial("tcp", addr1, addr2) run on the machine at addr1 can
            // connect to a simultaneous Dial("tcp", addr2, addr1) run on the machine
            // at addr2, without either machine executing Listen. If laddr == nil,
            // it means we want the kernel to pick an appropriate originating local
            // address. Some Linux kernels cycle blindly through a fixed range of
            // local ports, regardless of destination port. If a kernel happens to
            // pick local port 50001 as the source for a Dial("tcp", "", "localhost:50001"),
            // then the Dial will succeed, having simultaneously connected to itself.
            // This can only happen when we are letting the kernel pick a port (laddr == nil)
            // and when there is no listener for the destination address.
            // It's hard to argue this is anything other than a kernel bug. If we
            // see this happen, rather than expose the buggy effect to users, we
            // close the fd and try again. If it happens twice more, we relent and
            // use the result. See also:
            //    https://golang.org/issue/2690
            //    http://stackoverflow.com/questions/4949858/
            //
            // The opposite can also happen: if we ask the kernel to pick an appropriate
            // originating local address, sometimes it picks one that is already in use.
            // So if the error is EADDRNOTAVAIL, we have to try again too, just for
            // a different reason.
            //
            // The kernel socket code is no doubt enjoying watching us squirm.
            for (long i = 0L; i < 2L && (laddr == null || laddr.Port == 0L) && (selfConnect(fd, err) || spuriousENOTAVAIL(err)); i++)
            {
                if (err == null)
                {
                    fd.Close();
                }
                fd, err = internetSocket(ctx, net, laddr, raddr, syscall.SOCK_STREAM, 0L, "dial");
            }


            if (err != null)
            {
                return (null, err);
            }
            return (newTCPConn(fd), null);
        }

        private static bool selfConnect(ref netFD fd, error err)
        { 
            // If the connect failed, we clearly didn't connect to ourselves.
            if (err != null)
            {
                return false;
            } 

            // The socket constructor can return an fd with raddr nil under certain
            // unknown conditions. The errors in the calls there to Getpeername
            // are discarded, but we can't catch the problem there because those
            // calls are sometimes legally erroneous with a "socket not connected".
            // Since this code (selfConnect) is already trying to work around
            // a problem, we make sure if this happens we recognize trouble and
            // ask the DialTCP routine to try again.
            // TODO: try to understand what's really going on.
            if (fd.laddr == null || fd.raddr == null)
            {
                return true;
            }
            ref TCPAddr l = fd.laddr._<ref TCPAddr>();
            ref TCPAddr r = fd.raddr._<ref TCPAddr>();
            return l.Port == r.Port && l.IP.Equal(r.IP);
        }

        private static bool spuriousENOTAVAIL(error err)
        {
            {
                ref OpError (op, ok) = err._<ref OpError>();

                if (ok)
                {
                    err = op.Err;
                }

            }
            {
                ref os.SyscallError (sys, ok) = err._<ref os.SyscallError>();

                if (ok)
                {
                    err = sys.Err;
                }

            }
            return err == syscall.EADDRNOTAVAIL;
        }

        private static bool ok(this ref TCPListener ln)
        {
            return ln != null && ln.fd != null;
        }

        private static (ref TCPConn, error) accept(this ref TCPListener ln)
        {
            var (fd, err) = ln.fd.accept();
            if (err != null)
            {
                return (null, err);
            }
            return (newTCPConn(fd), null);
        }

        private static error close(this ref TCPListener ln)
        {
            return error.As(ln.fd.Close());
        }

        private static (ref os.File, error) file(this ref TCPListener ln)
        {
            var (f, err) = ln.fd.dup();
            if (err != null)
            {
                return (null, err);
            }
            return (f, null);
        }

        private static (ref TCPListener, error) listenTCP(context.Context ctx, @string network, ref TCPAddr laddr)
        {
            var (fd, err) = internetSocket(ctx, network, laddr, null, syscall.SOCK_STREAM, 0L, "listen");
            if (err != null)
            {
                return (null, err);
            }
            return (ref new TCPListener(fd), null);
        }
    }
}
