// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package net -- go2cs converted at 2020 August 29 08:28:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\writev_unix.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) writeBuffers(this ref conn c, ref Buffers v)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            var (n, err) = c.fd.writeBuffers(v);
            if (err != null)
            {
                return (n, ref new OpError(Op:"writev",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }
            return (n, null);
        }

        private static (long, error) writeBuffers(this ref netFD fd, ref Buffers v)
        {
            n, err = fd.pfd.Writev(new ptr<ref slice<slice<byte>>>(v));
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("writev", err));
        }
    }
}
