// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 August 29 08:26:20 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\file_unix.go
using poll = go.@internal.poll_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) dupSocket(ref os.File f)
        {
            var (s, err) = dupCloseOnExec(int(f.Fd()));
            if (err != null)
            {
                return (-1L, err);
            }
            {
                var err = syscall.SetNonblock(s, true);

                if (err != null)
                {
                    poll.CloseFunc(s);
                    return (-1L, os.NewSyscallError("setnonblock", err));
                }
            }
            return (s, null);
        }

        private static (ref netFD, error) newFileFD(ref os.File f)
        {
            var (s, err) = dupSocket(f);
            if (err != null)
            {
                return (null, err);
            }
            var family = syscall.AF_UNSPEC;
            var (sotype, err) = syscall.GetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_TYPE);
            if (err != null)
            {
                poll.CloseFunc(s);
                return (null, os.NewSyscallError("getsockopt", err));
            }
            var (lsa, _) = syscall.Getsockname(s);
            var (rsa, _) = syscall.Getpeername(s);
            switch (lsa.type())
            {
                case ref syscall.SockaddrInet4 _:
                    family = syscall.AF_INET;
                    break;
                case ref syscall.SockaddrInet6 _:
                    family = syscall.AF_INET6;
                    break;
                case ref syscall.SockaddrUnix _:
                    family = syscall.AF_UNIX;
                    break;
                default:
                {
                    poll.CloseFunc(s);
                    return (null, syscall.EPROTONOSUPPORT);
                    break;
                }
            }
            var (fd, err) = newFD(s, family, sotype, "");
            if (err != null)
            {
                poll.CloseFunc(s);
                return (null, err);
            }
            var laddr = fd.addrFunc()(lsa);
            var raddr = fd.addrFunc()(rsa);
            fd.net = laddr.Network();
            {
                var err = fd.init();

                if (err != null)
                {
                    fd.Close();
                    return (null, err);
                }

            }
            fd.setAddr(laddr, raddr);
            return (fd, null);
        }

        private static (Conn, error) fileConn(ref os.File f)
        {
            var (fd, err) = newFileFD(f);
            if (err != null)
            {
                return (null, err);
            }
            switch (fd.laddr.type())
            {
                case ref TCPAddr _:
                    return (newTCPConn(fd), null);
                    break;
                case ref UDPAddr _:
                    return (newUDPConn(fd), null);
                    break;
                case ref IPAddr _:
                    return (newIPConn(fd), null);
                    break;
                case ref UnixAddr _:
                    return (newUnixConn(fd), null);
                    break;
            }
            fd.Close();
            return (null, syscall.EINVAL);
        }

        private static (Listener, error) fileListener(ref os.File f)
        {
            var (fd, err) = newFileFD(f);
            if (err != null)
            {
                return (null, err);
            }
            switch (fd.laddr.type())
            {
                case ref TCPAddr laddr:
                    return (ref new TCPListener(fd), null);
                    break;
                case ref UnixAddr laddr:
                    return (ref new UnixListener(fd:fd,path:laddr.Name,unlink:false), null);
                    break;
            }
            fd.Close();
            return (null, syscall.EINVAL);
        }

        private static (PacketConn, error) filePacketConn(ref os.File f)
        {
            var (fd, err) = newFileFD(f);
            if (err != null)
            {
                return (null, err);
            }
            switch (fd.laddr.type())
            {
                case ref UDPAddr _:
                    return (newUDPConn(fd), null);
                    break;
                case ref IPAddr _:
                    return (newIPConn(fd), null);
                    break;
                case ref UnixAddr _:
                    return (newUnixConn(fd), null);
                    break;
            }
            fd.Close();
            return (null, syscall.EINVAL);
        }
    }
}
