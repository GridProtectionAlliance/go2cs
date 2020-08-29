// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:27:41 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_posix.go
using context = go.context_package;
using poll = go.@internal.poll_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        // A sockaddr represents a TCP, UDP, IP or Unix network endpoint
        // address that can be converted into a syscall.Sockaddr.
        private partial interface sockaddr : Addr
        {
            sockaddr family(); // isWildcard reports whether the address is a wildcard
// address.
            sockaddr isWildcard(); // sockaddr returns the address converted into a syscall
// sockaddr type that implements syscall.Sockaddr
// interface. It returns a nil interface when the address is
// nil.
            sockaddr sockaddr(long family); // toLocal maps the zero address to a local system address (127.0.0.1 or ::1)
            sockaddr toLocal(@string net);
        }

        // socket returns a network file descriptor that is ready for
        // asynchronous I/O using the network poller.
        private static (ref netFD, error) socket(context.Context ctx, @string net, long family, long sotype, long proto, bool ipv6only, sockaddr laddr, sockaddr raddr)
        {
            var (s, err) = sysSocket(family, sotype, proto);
            if (err != null)
            {
                return (null, err);
            }
            err = setDefaultSockopts(s, family, sotype, ipv6only);

            if (err != null)
            {
                poll.CloseFunc(s);
                return (null, err);
            }
            fd, err = newFD(s, family, sotype, net);

            if (err != null)
            {
                poll.CloseFunc(s);
                return (null, err);
            } 

            // This function makes a network file descriptor for the
            // following applications:
            //
            // - An endpoint holder that opens a passive stream
            //   connection, known as a stream listener
            //
            // - An endpoint holder that opens a destination-unspecific
            //   datagram connection, known as a datagram listener
            //
            // - An endpoint holder that opens an active stream or a
            //   destination-specific datagram connection, known as a
            //   dialer
            //
            // - An endpoint holder that opens the other connection, such
            //   as talking to the protocol stack inside the kernel
            //
            // For stream and datagram listeners, they will only require
            // named sockets, so we can assume that it's just a request
            // from stream or datagram listeners when laddr is not nil but
            // raddr is nil. Otherwise we assume it's just for dialers or
            // the other connection holders.
            if (laddr != null && raddr == null)
            {

                if (sotype == syscall.SOCK_STREAM || sotype == syscall.SOCK_SEQPACKET) 
                    {
                        var err__prev2 = err;

                        var err = fd.listenStream(laddr, listenerBacklog);

                        if (err != null)
                        {
                            fd.Close();
                            return (null, err);
                        }

                        err = err__prev2;

                    }
                    return (fd, null);
                else if (sotype == syscall.SOCK_DGRAM) 
                    {
                        var err__prev2 = err;

                        err = fd.listenDatagram(laddr);

                        if (err != null)
                        {
                            fd.Close();
                            return (null, err);
                        }

                        err = err__prev2;

                    }
                    return (fd, null);
                            }
            {
                var err__prev1 = err;

                err = fd.dial(ctx, laddr, raddr);

                if (err != null)
                {
                    fd.Close();
                    return (null, err);
                }

                err = err__prev1;

            }
            return (fd, null);
        }

        private static Func<syscall.Sockaddr, Addr> addrFunc(this ref netFD fd)
        {

            if (fd.family == syscall.AF_INET || fd.family == syscall.AF_INET6) 

                if (fd.sotype == syscall.SOCK_STREAM) 
                    return sockaddrToTCP;
                else if (fd.sotype == syscall.SOCK_DGRAM) 
                    return sockaddrToUDP;
                else if (fd.sotype == syscall.SOCK_RAW) 
                    return sockaddrToIP;
                            else if (fd.family == syscall.AF_UNIX) 

                if (fd.sotype == syscall.SOCK_STREAM) 
                    return sockaddrToUnix;
                else if (fd.sotype == syscall.SOCK_DGRAM) 
                    return sockaddrToUnixgram;
                else if (fd.sotype == syscall.SOCK_SEQPACKET) 
                    return sockaddrToUnixpacket;
                                        return _p0 => null;
        }

        private static error dial(this ref netFD fd, context.Context ctx, sockaddr laddr, sockaddr raddr)
        {
            error err = default;
            syscall.Sockaddr lsa = default;
            if (laddr != null)
            {
                lsa, err = laddr.sockaddr(fd.family);

                if (err != null)
                {
                    return error.As(err);
                }
                else if (lsa != null)
                {
                    {
                        error err__prev4 = err;

                        err = syscall.Bind(fd.pfd.Sysfd, lsa);

                        if (err != null)
                        {
                            return error.As(os.NewSyscallError("bind", err));
                        }

                        err = err__prev4;

                    }
                }
            }
            syscall.Sockaddr rsa = default; // remote address from the user
            syscall.Sockaddr crsa = default; // remote address we actually connected to
            if (raddr != null)
            {
                rsa, err = raddr.sockaddr(fd.family);

                if (err != null)
                {
                    return error.As(err);
                }
                crsa, err = fd.connect(ctx, lsa, rsa);

                if (err != null)
                {
                    return error.As(err);
                }
                fd.isConnected = true;
            }
            else
            {
                {
                    error err__prev2 = err;

                    err = fd.init();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            } 
            // Record the local and remote addresses from the actual socket.
            // Get the local address by calling Getsockname.
            // For the remote address, use
            // 1) the one returned by the connect method, if any; or
            // 2) the one from Getpeername, if it succeeds; or
            // 3) the one passed to us as the raddr parameter.
            lsa, _ = syscall.Getsockname(fd.pfd.Sysfd);
            if (crsa != null)
            {
                fd.setAddr(fd.addrFunc()(lsa), fd.addrFunc()(crsa));
            }            rsa, _ = syscall.Getpeername(fd.pfd.Sysfd);


            else if (rsa != null)
            {
                fd.setAddr(fd.addrFunc()(lsa), fd.addrFunc()(rsa));
            }
            else
            {
                fd.setAddr(fd.addrFunc()(lsa), raddr);
            }
            return error.As(null);
        }

        private static error listenStream(this ref netFD fd, sockaddr laddr, long backlog)
        {
            {
                var err__prev1 = err;

                var err = setDefaultListenerSockopts(fd.pfd.Sysfd);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var lsa__prev1 = lsa;
                var err__prev1 = err;

                var (lsa, err) = laddr.sockaddr(fd.family);

                if (err != null)
                {
                    return error.As(err);
                }
                else if (lsa != null)
                {
                    {
                        var err__prev3 = err;

                        err = syscall.Bind(fd.pfd.Sysfd, lsa);

                        if (err != null)
                        {
                            return error.As(os.NewSyscallError("bind", err));
                        }

                        err = err__prev3;

                    }
                }

                lsa = lsa__prev1;
                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = listenFunc(fd.pfd.Sysfd, backlog);

                if (err != null)
                {
                    return error.As(os.NewSyscallError("listen", err));
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = fd.init();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            var (lsa, _) = syscall.Getsockname(fd.pfd.Sysfd);
            fd.setAddr(fd.addrFunc()(lsa), null);
            return error.As(null);
        }

        private static error listenDatagram(this ref netFD fd, sockaddr laddr)
        {
            switch (laddr.type())
            {
                case ref UDPAddr addr:
                    if (addr.IP != null && addr.IP.IsMulticast())
                    {
                        {
                            var err__prev2 = err;

                            var err = setDefaultMulticastSockopts(fd.pfd.Sysfd);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                        var addr = addr.Value;

                        if (fd.family == syscall.AF_INET) 
                            addr.IP = IPv4zero;
                        else if (fd.family == syscall.AF_INET6) 
                            addr.IP = IPv6unspecified;
                                                laddr = ref addr;
                    }
                    break;
            }
            {
                var lsa__prev1 = lsa;
                var err__prev1 = err;

                var (lsa, err) = laddr.sockaddr(fd.family);

                if (err != null)
                {
                    return error.As(err);
                }
                else if (lsa != null)
                {
                    {
                        var err__prev3 = err;

                        err = syscall.Bind(fd.pfd.Sysfd, lsa);

                        if (err != null)
                        {
                            return error.As(os.NewSyscallError("bind", err));
                        }

                        err = err__prev3;

                    }
                }

                lsa = lsa__prev1;
                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = fd.init();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            var (lsa, _) = syscall.Getsockname(fd.pfd.Sysfd);
            fd.setAddr(fd.addrFunc()(lsa), null);
            return error.As(null);
        }
    }
}
