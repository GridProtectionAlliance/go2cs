// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || netbsd || openbsd
// +build dragonfly freebsd netbsd openbsd

// package syscall -- go2cs converted at 2022 March 13 05:40:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\forkpipe2.go
namespace go;

public static partial class syscall_package {

private static error forkExecPipe(slice<nint> p) {
    return error.As(Pipe2(p, O_CLOEXEC))!;
}

} // end syscall_package
