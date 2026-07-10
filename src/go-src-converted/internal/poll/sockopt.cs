// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// SetsockoptInt wraps the setsockopt network call with an int argument.
public static error SetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name, nint arg) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.SetsockoptInt(fd.Sysfd, level, name, arg);
});

// SetsockoptInet4Addr wraps the setsockopt network call with an IPv4 address.
public static error SetsockoptInet4Addr(this ж<FD> Ꮡfd, nint level, nint name, array<byte> arg) => func((defer, recover) => {
    arg = arg.Clone();

    ref var fd = ref Ꮡfd.Value;
    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.SetsockoptInet4Addr(fd.Sysfd, level, name, arg);
});

// SetsockoptLinger wraps the setsockopt network call with a Linger argument.
public static error SetsockoptLinger(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.Linger> Ꮡl) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;
    ref var l = ref Ꮡl.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.SetsockoptLinger(fd.Sysfd, level, name, Ꮡl);
});

// GetsockoptInt wraps the getsockopt network call with an int argument.
public static (nint, error) GetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return (-1, err);
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.GetsockoptInt(fd.Sysfd, level, name);
});

} // end poll_package
