// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package net -- go2cs converted at 2020 October 09 04:52:37 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\writev_unix.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) writeBuffers(this ptr<conn> _addr_c, ptr<Buffers> _addr_v)
        {
            long _p0 = default;
            error _p0 = default!;
            ref conn c = ref _addr_c.val;
            ref Buffers v = ref _addr_v.val;

            if (!c.ok())
            {
                return (0L, error.As(syscall.EINVAL)!);
            }
            var (n, err) = c.fd.writeBuffers(v);
            if (err != null)
            {
                return (n, error.As(addr(new OpError(Op:"writev",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!);
            }
            return (n, error.As(null!)!);

        }

        private static (long, error) writeBuffers(this ptr<netFD> _addr_fd, ptr<Buffers> _addr_v)
        {
            long n = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;
            ref Buffers v = ref _addr_v.val;

            n, err = fd.pfd.Writev(new ptr<ptr<slice<slice<byte>>>>(v));
            runtime.KeepAlive(fd);
            return (n, error.As(wrapSyscallError("writev", err))!);
        }
    }
}
