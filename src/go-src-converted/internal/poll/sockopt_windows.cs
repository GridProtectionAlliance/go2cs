// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using syscall = syscall_package;

partial class poll_package {

// WSAIoctl wraps the WSAIoctl network call.
[GoRecv] public static error WSAIoctl(this ref FD fd, uint32 iocc, ж<byte> Ꮡinbuf, uint32 cbif, ж<byte> Ꮡoutbuf, uint32 cbob, ж<uint32> Ꮡcbbr, ж<syscall.Overlapped> Ꮡoverlapped, uintptr completionRoutine) => func((defer, _) => {
    ref var inbuf = ref Ꮡinbuf.val;
    ref var outbuf = ref Ꮡoutbuf.val;
    ref var cbbr = ref Ꮡcbbr.val;
    ref var overlapped = ref Ꮡoverlapped.val;

    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.WSAIoctl(fd.Sysfd, iocc, Ꮡinbuf, cbif, Ꮡoutbuf, cbob, Ꮡcbbr, Ꮡoverlapped, completionRoutine);
});

} // end poll_package
