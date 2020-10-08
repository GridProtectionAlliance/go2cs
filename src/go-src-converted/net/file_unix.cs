// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 October 08 03:33:04 UTC
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
        private static (long, error) dupSocket(ptr<os.File> _addr_f)
        {
            long _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (s, call, err) = poll.DupCloseOnExec(int(f.Fd()));
            if (err != null)
            {
                if (call != "")
                {
                    err = os.NewSyscallError(call, err);
                }
                return (-1L, error.As(err)!);

            }
            {
                var err = syscall.SetNonblock(s, true);

                if (err != null)
                {
                    poll.CloseFunc(s);
                    return (-1L, error.As(os.NewSyscallError("setnonblock", err))!);
                }
            }

            return (s, error.As(null!)!);

        }

        private static (ptr<netFD>, error) newFileFD(ptr<os.File> _addr_f)
        {
            ptr<netFD> _p0 = default!;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (s, err) = dupSocket(_addr_f);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var family = syscall.AF_UNSPEC;
            var (sotype, err) = syscall.GetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_TYPE);
            if (err != null)
            {
                poll.CloseFunc(s);
                return (_addr_null!, error.As(os.NewSyscallError("getsockopt", err))!);
            }

            var (lsa, _) = syscall.Getsockname(s);
            var (rsa, _) = syscall.Getpeername(s);
            switch (lsa.type())
            {
                case ptr<syscall.SockaddrInet4> _:
                    family = syscall.AF_INET;
                    break;
                case ptr<syscall.SockaddrInet6> _:
                    family = syscall.AF_INET6;
                    break;
                case ptr<syscall.SockaddrUnix> _:
                    family = syscall.AF_UNIX;
                    break;
                default:
                {
                    poll.CloseFunc(s);
                    return (_addr_null!, error.As(syscall.EPROTONOSUPPORT)!);
                    break;
                }
            }
            var (fd, err) = newFD(s, family, sotype, "");
            if (err != null)
            {
                poll.CloseFunc(s);
                return (_addr_null!, error.As(err)!);
            }

            var laddr = fd.addrFunc()(lsa);
            var raddr = fd.addrFunc()(rsa);
            fd.net = laddr.Network();
            {
                var err = fd.init();

                if (err != null)
                {
                    fd.Close();
                    return (_addr_null!, error.As(err)!);
                }

            }

            fd.setAddr(laddr, raddr);
            return (_addr_fd!, error.As(null!)!);

        }

        private static (Conn, error) fileConn(ptr<os.File> _addr_f)
        {
            Conn _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (fd, err) = newFileFD(_addr_f);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            switch (fd.laddr.type())
            {
                case ptr<TCPAddr> _:
                    return (newTCPConn(fd), error.As(null!)!);
                    break;
                case ptr<UDPAddr> _:
                    return (newUDPConn(fd), error.As(null!)!);
                    break;
                case ptr<IPAddr> _:
                    return (newIPConn(fd), error.As(null!)!);
                    break;
                case ptr<UnixAddr> _:
                    return (newUnixConn(fd), error.As(null!)!);
                    break;
            }
            fd.Close();
            return (null, error.As(syscall.EINVAL)!);

        }

        private static (Listener, error) fileListener(ptr<os.File> _addr_f)
        {
            Listener _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (fd, err) = newFileFD(_addr_f);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            switch (fd.laddr.type())
            {
                case ptr<TCPAddr> laddr:
                    return (addr(new TCPListener(fd:fd)), error.As(null!)!);
                    break;
                case ptr<UnixAddr> laddr:
                    return (addr(new UnixListener(fd:fd,path:laddr.Name,unlink:false)), error.As(null!)!);
                    break;
            }
            fd.Close();
            return (null, error.As(syscall.EINVAL)!);

        }

        private static (PacketConn, error) filePacketConn(ptr<os.File> _addr_f)
        {
            PacketConn _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (fd, err) = newFileFD(_addr_f);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            switch (fd.laddr.type())
            {
                case ptr<UDPAddr> _:
                    return (newUDPConn(fd), error.As(null!)!);
                    break;
                case ptr<IPAddr> _:
                    return (newIPConn(fd), error.As(null!)!);
                    break;
                case ptr<UnixAddr> _:
                    return (newUnixConn(fd), error.As(null!)!);
                    break;
            }
            fd.Close();
            return (null, error.As(syscall.EINVAL)!);

        }
    }
}
