// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl

// package net -- go2cs converted at 2020 August 29 08:27:34 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockopt_stub.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setDefaultSockopts(long s, long family, long sotype, bool ipv6only)
        {
            return error.As(null);
        }

        private static error setDefaultListenerSockopts(long s)
        {
            return error.As(null);
        }

        private static error setDefaultMulticastSockopts(long s)
        {
            return error.As(null);
        }

        private static error setReadBuffer(ref netFD fd, long bytes)
        {
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setWriteBuffer(ref netFD fd, long bytes)
        {
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setKeepAlive(ref netFD fd, bool keepalive)
        {
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setLinger(ref netFD fd, long sec)
        {
            return error.As(syscall.ENOPROTOOPT);
        }
    }
}
