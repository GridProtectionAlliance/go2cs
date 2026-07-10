// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// SetsockoptIPMreq wraps the setsockopt network call with an IPMreq argument.
public static error SetsockoptIPMreq(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPMreq> Ꮡmreq) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;
    ref var mreq = ref Ꮡmreq.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.SetsockoptIPMreq(fd.Sysfd, level, name, Ꮡmreq);
});

// SetsockoptIPv6Mreq wraps the setsockopt network call with an IPv6Mreq argument.
public static error SetsockoptIPv6Mreq(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPv6Mreq> Ꮡmreq) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;
    ref var mreq = ref Ꮡmreq.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.SetsockoptIPv6Mreq(fd.Sysfd, level, name, Ꮡmreq);
});

} // end poll_package
