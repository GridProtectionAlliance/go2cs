// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\ioctl_aix.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    //go:cgo_import_dynamic libc_ioctl ioctl "libc.a/shr_64.o"
    //go:linkname libc_ioctl libc_ioctl
private static System.UIntPtr libc_ioctl = default;

// Implemented in syscall/syscall_aix.go.
private static (System.UIntPtr, System.UIntPtr, syscall.Errno) syscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);

public static error Ioctl(nint fd, nint cmd, System.UIntPtr args) {
    error err = default!;

    var (_, _, e1) = syscall6(uintptr(@unsafe.Pointer(_addr_libc_ioctl)), 3, uintptr(fd), uintptr(cmd), uintptr(args), 0, 0, 0);
    if (e1 != 0) {>>MARKER:FUNCTION_syscall6_BLOCK_PREFIX<<
        err = e1;
    }
    return ;

}

} // end unix_package
