// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using syscall = syscall_package;

partial class poll_package {

// SetsockoptIPMreq wraps the setsockopt network call with an IPMreq argument.
[GoRecv] public static error SetsockoptIPMreq(this ref FD fd, nint level, nint name, ж<syscall.IPMreq> Ꮡmreq) => func((defer, _) => {
    ref var mreq = ref Ꮡmreq.val;

    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.SetsockoptIPMreq(fd.Sysfd, level, name, Ꮡmreq);
});

// SetsockoptIPv6Mreq wraps the setsockopt network call with an IPv6Mreq argument.
[GoRecv] public static error SetsockoptIPv6Mreq(this ref FD fd, nint level, nint name, ж<syscall.IPv6Mreq> Ꮡmreq) => func((defer, _) => {
    ref var mreq = ref Ꮡmreq.val;

    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.SetsockoptIPv6Mreq(fd.Sysfd, level, name, Ꮡmreq);
});

} // end poll_package
