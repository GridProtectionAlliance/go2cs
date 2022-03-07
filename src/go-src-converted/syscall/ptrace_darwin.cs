// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !ios
// +build !ios

// package syscall -- go2cs converted at 2022 March 06 22:26:40 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\ptrace_darwin.go


namespace go;

public static partial class syscall_package {

    // Nosplit because it is called from forkAndExecInChild.
    //
    //go:nosplit
private static error ptrace(nint request, nint pid, System.UIntPtr addr, System.UIntPtr data) {
    return error.As(ptrace1(request, pid, addr, data))!;
}

} // end syscall_package
