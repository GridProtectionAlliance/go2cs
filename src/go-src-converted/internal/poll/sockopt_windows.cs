// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// WSAIoctl wraps the WSAIoctl network call.
public static error WSAIoctl(this ж<FD> Ꮡfd, uint32 iocc, ж<byte> Ꮡinbuf, uint32 cbif, ж<byte> Ꮡoutbuf, uint32 cbob, ж<uint32> Ꮡcbbr, ж<Δsyscall.Overlapped> Ꮡoverlapped, uintptr completionRoutine) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;
    ref var inbuf = ref Ꮡinbuf.Value;
    ref var outbuf = ref Ꮡoutbuf.Value;
    ref var cbbr = ref Ꮡcbbr.Value;
    ref var overlapped = ref Ꮡoverlapped.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.WSAIoctl(fd.Sysfd, iocc, Ꮡinbuf, cbif, Ꮡoutbuf, cbob, Ꮡcbbr, Ꮡoverlapped, completionRoutine);
});

} // end poll_package
