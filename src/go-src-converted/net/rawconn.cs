// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:14 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\rawconn.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On Windows, the Read and Write methods of
        // syscall.RawConn are not implemented.

        // BUG(mikio): On NaCl and Plan 9, the Control, Read and Write methods
        // of syscall.RawConn are not implemented.
        private partial struct rawConn
        {
            public ptr<netFD> fd;
        }

        private static bool ok(this ref rawConn c)
        {
            return c != null && c.fd != null;
        }

        private static error Control(this ref rawConn c, Action<System.UIntPtr> f)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            var err = c.fd.pfd.RawControl(f);
            runtime.KeepAlive(c.fd);
            if (err != null)
            {
                err = ref new OpError(Op:"raw-control",Net:c.fd.net,Source:nil,Addr:c.fd.laddr,Err:err);
            }
            return error.As(err);
        }

        private static error Read(this ref rawConn c, Func<System.UIntPtr, bool> f)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            var err = c.fd.pfd.RawRead(f);
            runtime.KeepAlive(c.fd);
            if (err != null)
            {
                err = ref new OpError(Op:"raw-read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return error.As(err);
        }

        private static error Write(this ref rawConn c, Func<System.UIntPtr, bool> f)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            var err = c.fd.pfd.RawWrite(f);
            runtime.KeepAlive(c.fd);
            if (err != null)
            {
                err = ref new OpError(Op:"raw-write",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return error.As(err);
        }

        private static (ref rawConn, error) newRawConn(ref netFD fd)
        {
            return (ref new rawConn(fd:fd), null);
        }

        private partial struct rawListener
        {
            public ref rawConn rawConn => ref rawConn_val;
        }

        private static error Read(this ref rawListener l, Func<System.UIntPtr, bool> _p0)
        {
            return error.As(syscall.EINVAL);
        }

        private static error Write(this ref rawListener l, Func<System.UIntPtr, bool> _p0)
        {
            return error.As(syscall.EINVAL);
        }

        private static (ref rawListener, error) newRawListener(ref netFD fd)
        {
            return (ref new rawListener(rawConn{fd:fd}), null);
        }
    }
}
