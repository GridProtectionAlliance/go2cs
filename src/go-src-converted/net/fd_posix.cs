// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 October 09 04:51:26 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\fd_posix.go
using poll = go.@internal.poll_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // Network file descriptor.
        private partial struct netFD
        {
            public poll.FD pfd; // immutable until Close
            public long family;
            public long sotype;
            public bool isConnected; // handshake completed or use of association with peer
            public @string net;
            public Addr laddr;
            public Addr raddr;
        }

        private static void setAddr(this ptr<netFD> _addr_fd, Addr laddr, Addr raddr)
        {
            ref netFD fd = ref _addr_fd.val;

            fd.laddr = laddr;
            fd.raddr = raddr;
            runtime.SetFinalizer(fd, ptr<netFD>);
        }

        private static error Close(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            runtime.SetFinalizer(fd, null);
            return error.As(fd.pfd.Close())!;
        }

        private static error shutdown(this ptr<netFD> _addr_fd, long how)
        {
            ref netFD fd = ref _addr_fd.val;

            var err = fd.pfd.Shutdown(how);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("shutdown", err))!;
        }

        private static error closeRead(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.shutdown(syscall.SHUT_RD))!;
        }

        private static error closeWrite(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.shutdown(syscall.SHUT_WR))!;
        }

        private static (long, error) Read(this ptr<netFD> _addr_fd, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            n, err = fd.pfd.Read(p);
            runtime.KeepAlive(fd);
            return (n, error.As(wrapSyscallError(readSyscallName, err))!);
        }

        private static (long, syscall.Sockaddr, error) readFrom(this ptr<netFD> _addr_fd, slice<byte> p)
        {
            long n = default;
            syscall.Sockaddr sa = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            n, sa, err = fd.pfd.ReadFrom(p);
            runtime.KeepAlive(fd);
            return (n, sa, error.As(wrapSyscallError(readFromSyscallName, err))!);
        }

        private static (long, long, long, syscall.Sockaddr, error) readMsg(this ptr<netFD> _addr_fd, slice<byte> p, slice<byte> oob)
        {
            long n = default;
            long oobn = default;
            long flags = default;
            syscall.Sockaddr sa = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            n, oobn, flags, sa, err = fd.pfd.ReadMsg(p, oob);
            runtime.KeepAlive(fd);
            return (n, oobn, flags, sa, error.As(wrapSyscallError(readMsgSyscallName, err))!);
        }

        private static (long, error) Write(this ptr<netFD> _addr_fd, slice<byte> p)
        {
            long nn = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            nn, err = fd.pfd.Write(p);
            runtime.KeepAlive(fd);
            return (nn, error.As(wrapSyscallError(writeSyscallName, err))!);
        }

        private static (long, error) writeTo(this ptr<netFD> _addr_fd, slice<byte> p, syscall.Sockaddr sa)
        {
            long n = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            n, err = fd.pfd.WriteTo(p, sa);
            runtime.KeepAlive(fd);
            return (n, error.As(wrapSyscallError(writeToSyscallName, err))!);
        }

        private static (long, long, error) writeMsg(this ptr<netFD> _addr_fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa)
        {
            long n = default;
            long oobn = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            n, oobn, err = fd.pfd.WriteMsg(p, oob, sa);
            runtime.KeepAlive(fd);
            return (n, oobn, error.As(wrapSyscallError(writeMsgSyscallName, err))!);
        }

        private static error SetDeadline(this ptr<netFD> _addr_fd, time.Time t)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.pfd.SetDeadline(t))!;
        }

        private static error SetReadDeadline(this ptr<netFD> _addr_fd, time.Time t)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.pfd.SetReadDeadline(t))!;
        }

        private static error SetWriteDeadline(this ptr<netFD> _addr_fd, time.Time t)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.pfd.SetWriteDeadline(t))!;
        }
    }
}
