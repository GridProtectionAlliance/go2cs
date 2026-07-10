// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// Fsync wraps syscall.Fsync.
public static error Fsync(this ж<FD> Ꮡfd) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.Fsync(fd.Sysfd);
});

} // end poll_package
