// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using syscall = syscall_package;

partial class poll_package {

// SetsockoptInt wraps the setsockopt network call with an int argument.
[GoRecv] public static error SetsockoptInt(this ref FD fd, nint level, nint name, nint arg) => func((defer, _) => {
    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.SetsockoptInt(fd.Sysfd, level, name, arg);
});

// SetsockoptInet4Addr wraps the setsockopt network call with an IPv4 address.
[GoRecv] public static error SetsockoptInet4Addr(this ref FD fd, nint level, nint name, array<byte> arg) => func((defer, _) => {
    arg = arg.Clone();

    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.SetsockoptInet4Addr(fd.Sysfd, level, name, arg);
});

// SetsockoptLinger wraps the setsockopt network call with a Linger argument.
[GoRecv] public static error SetsockoptLinger(this ref FD fd, nint level, nint name, ж<syscall.Linger> Ꮡl) => func((defer, _) => {
    ref var l = ref Ꮡl.val;

    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.SetsockoptLinger(fd.Sysfd, level, name, Ꮡl);
});

// GetsockoptInt wraps the getsockopt network call with an int argument.
[GoRecv] public static (nint, error) GetsockoptInt(this ref FD fd, nint level, nint name) => func((defer, _) => {
    {
        var err = fd.incref(); if (err != default!) {
            return (-1, err);
        }
    }
    defer(fd.decref);
    return syscall.GetsockoptInt(fd.Sysfd, level, name);
});

} // end poll_package
