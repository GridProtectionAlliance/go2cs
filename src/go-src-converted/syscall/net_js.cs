// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// js/wasm uses fake networking directly implemented in the net package.
// This file only exists to make the compiler happy.

// +build js,wasm

// package syscall -- go2cs converted at 2020 October 08 03:26:50 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\net_js.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly var AF_UNSPEC = (var)iota;
        public static readonly var AF_UNIX = (var)0;
        public static readonly var AF_INET = (var)1;
        public static readonly var AF_INET6 = (var)2;


        public static readonly long SOCK_STREAM = (long)1L + iota;
        public static readonly var SOCK_DGRAM = (var)0;
        public static readonly var SOCK_RAW = (var)1;
        public static readonly var SOCK_SEQPACKET = (var)2;


        public static readonly long IPPROTO_IP = (long)0L;
        public static readonly long IPPROTO_IPV4 = (long)4L;
        public static readonly ulong IPPROTO_IPV6 = (ulong)0x29UL;
        public static readonly long IPPROTO_TCP = (long)6L;
        public static readonly ulong IPPROTO_UDP = (ulong)0x11UL;


        private static readonly var _ = (var)iota;
        public static readonly var IPV6_V6ONLY = (var)0;
        public static readonly var SOMAXCONN = (var)1;
        public static readonly var SO_ERROR = (var)2;


        // Misc constants expected by package net but not supported.
        private static readonly var _ = (var)iota;
        public static readonly SYS_FCNTL F_DUPFD_CLOEXEC = (SYS_FCNTL)500L; // unsupported

        public partial interface Sockaddr
        {
        }

        public partial struct SockaddrInet4
        {
            public long Port;
            public array<byte> Addr;
        }

        public partial struct SockaddrInet6
        {
            public long Port;
            public uint ZoneId;
            public array<byte> Addr;
        }

        public partial struct SockaddrUnix
        {
            public @string Name;
        }

        public static (long, error) Socket(long proto, long sotype, long unused)
        {
            long fd = default;
            error err = default!;

            return (0L, error.As(ENOSYS)!);
        }

        public static error Bind(long fd, Sockaddr sa)
        {
            return error.As(ENOSYS)!;
        }

        public static error StopIO(long fd)
        {
            return error.As(ENOSYS)!;
        }

        public static error Listen(long fd, long backlog)
        {
            return error.As(ENOSYS)!;
        }

        public static (long, Sockaddr, error) Accept(long fd)
        {
            long newfd = default;
            Sockaddr sa = default;
            error err = default!;

            return (0L, null, error.As(ENOSYS)!);
        }

        public static error Connect(long fd, Sockaddr sa)
        {
            return error.As(ENOSYS)!;
        }

        public static (long, Sockaddr, error) Recvfrom(long fd, slice<byte> p, long flags)
        {
            long n = default;
            Sockaddr from = default;
            error err = default!;

            return (0L, null, error.As(ENOSYS)!);
        }

        public static error Sendto(long fd, slice<byte> p, long flags, Sockaddr to)
        {
            return error.As(ENOSYS)!;
        }

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            long n = default;
            long oobn = default;
            long recvflags = default;
            Sockaddr from = default;
            error err = default!;

            return (0L, 0L, 0L, null, error.As(ENOSYS)!);
        }

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            long n = default;
            error err = default!;

            return (0L, error.As(ENOSYS)!);
        }

        public static (long, error) GetsockoptInt(long fd, long level, long opt)
        {
            long value = default;
            error err = default!;

            return (0L, error.As(ENOSYS)!);
        }

        public static error SetsockoptInt(long fd, long level, long opt, long value)
        {
            return error.As(null!)!;
        }

        public static error SetReadDeadline(long fd, long t)
        {
            return error.As(ENOSYS)!;
        }

        public static error SetWriteDeadline(long fd, long t)
        {
            return error.As(ENOSYS)!;
        }

        public static error Shutdown(long fd, long how)
        {
            return error.As(ENOSYS)!;
        }

        public static error SetNonblock(long fd, bool nonblocking)
        {
            return error.As(null!)!;
        }
    }
}
