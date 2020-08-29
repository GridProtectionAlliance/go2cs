// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:28:10 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\unixsock_plan9.go
using context = go.context_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, ref UnixAddr, error) readFrom(this ref UnixConn c, slice<byte> b)
        {
            return (0L, null, syscall.EPLAN9);
        }

        private static (long, long, long, ref UnixAddr, error) readMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob)
        {
            return (0L, 0L, 0L, null, syscall.EPLAN9);
        }

        private static (long, error) writeTo(this ref UnixConn c, slice<byte> b, ref UnixAddr addr)
        {
            return (0L, syscall.EPLAN9);
        }

        private static (long, long, error) writeMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob, ref UnixAddr addr)
        {
            return (0L, 0L, syscall.EPLAN9);
        }

        private static (ref UnixConn, error) dialUnix(context.Context ctx, @string network, ref UnixAddr laddr, ref UnixAddr raddr)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref UnixConn, error) accept(this ref UnixListener ln)
        {
            return (null, syscall.EPLAN9);
        }

        private static error close(this ref UnixListener ln)
        {
            return error.As(syscall.EPLAN9);
        }

        private static (ref os.File, error) file(this ref UnixListener ln)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref UnixListener, error) listenUnix(context.Context ctx, @string network, ref UnixAddr laddr)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref UnixConn, error) listenUnixgram(context.Context ctx, @string network, ref UnixAddr laddr)
        {
            return (null, syscall.EPLAN9);
        }
    }
}
