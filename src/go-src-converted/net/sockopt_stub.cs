// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package net -- go2cs converted at 2020 October 08 03:34:28 UTC
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
            return error.As(null!)!;
        }

        private static error setDefaultListenerSockopts(long s)
        {
            return error.As(null!)!;
        }

        private static error setDefaultMulticastSockopts(long s)
        {
            return error.As(null!)!;
        }

        private static error setReadBuffer(ptr<netFD> _addr_fd, long bytes)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.ENOPROTOOPT)!;
        }

        private static error setWriteBuffer(ptr<netFD> _addr_fd, long bytes)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.ENOPROTOOPT)!;
        }

        private static error setKeepAlive(ptr<netFD> _addr_fd, bool keepalive)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.ENOPROTOOPT)!;
        }

        private static error setLinger(ptr<netFD> _addr_fd, long sec)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.ENOPROTOOPT)!;
        }
    }
}
