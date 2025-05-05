// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

// BUG(tmm1): On Windows, the Write method of syscall.RawConn
// does not integrate with the runtime's network poller. It cannot
// wait for the connection to become writeable, and does not respect
// deadlines. If the user-provided callback returns false, the Write
// method will fail immediately.
// BUG(mikio): On JS and Plan 9, the Control, Read and Write
// methods of syscall.RawConn are not implemented.
[GoType] partial struct rawConn {
    internal ж<netFD> fd;
}

[GoRecv] internal static bool ok(this ref rawConn c) {
    return c != nil && c.fd != nil;
}

[GoRecv] internal static error Control(this ref rawConn c, Action<uintptr> f) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.pfd.RawControl(f);
    runtime.KeepAlive(c.fd);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "raw-control"u8, Net: c.fd.net, Source: default!, ΔAddr: c.fd.laddr, Err: err); err = ref Ꮡerr.val;
    }
    return err;
}

[GoRecv] internal static error Read(this ref rawConn c, Func<uintptr, bool> f) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.pfd.RawRead(f);
    runtime.KeepAlive(c.fd);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "raw-read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return err;
}

[GoRecv] internal static error Write(this ref rawConn c, Func<uintptr, bool> f) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.pfd.RawWrite(f);
    runtime.KeepAlive(c.fd);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "raw-write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return err;
}

// PollFD returns the poll.FD of the underlying connection.
//
// Other packages in std that also import [internal/poll] (such as os)
// can use a type assertion to access this extension method so that
// they can pass the *poll.FD to functions like poll.Splice.
//
// PollFD is not intended for use outside the standard library.
[GoRecv] internal static ж<poll.FD> PollFD(this ref rawConn c) {
    if (!c.ok()) {
        return default!;
    }
    return Ꮡ(c.fd.pfd);
}

internal static ж<rawConn> newRawConn(ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.val;

    return Ꮡ(new rawConn(fd: fd));
}

// Network returns the network type of the underlying connection.
//
// Other packages in std that import internal/poll and are unable to
// import net (such as os) can use a type assertion to access this
// extension method so that they can distinguish different socket types.
//
// Network is not intended for use outside the standard library.
[GoRecv] internal static poll.String Network(this ref rawConn c) {
    return ((poll.String)c.fd.net);
}

[GoType] partial struct rawListener {
    internal partial ref rawConn rawConn { get; }
}

[GoRecv] internal static error Read(this ref rawListener l, Func<uintptr, bool> _) {
    return syscall.EINVAL;
}

[GoRecv] internal static error Write(this ref rawListener l, Func<uintptr, bool> _) {
    return syscall.EINVAL;
}

internal static ж<rawListener> newRawListener(ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.val;

    return Ꮡ(new rawListener(new rawConn(fd: fd)));
}

} // end net_package
