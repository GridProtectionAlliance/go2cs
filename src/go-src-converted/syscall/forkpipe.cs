// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || solaris
// +build aix darwin solaris

// package syscall -- go2cs converted at 2022 March 13 05:40:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\forkpipe.go
namespace go;

public static partial class syscall_package {

// Try to open a pipe with O_CLOEXEC set on both file descriptors.
private static error forkExecPipe(slice<nint> p) {
    var err = Pipe(p);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = fcntl(p[0], F_SETFD, FD_CLOEXEC);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = fcntl(p[1], F_SETFD, FD_CLOEXEC);
    return error.As(err)!;
}

} // end syscall_package
