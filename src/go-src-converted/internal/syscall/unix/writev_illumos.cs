// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build illumos
// +build illumos

// package unix -- go2cs converted at 2022 March 06 22:12:56 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\writev_illumos.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    //go:cgo_import_dynamic libc_writev writev "libc.so"

    //go:linkname procwritev libc_writev
private static System.UIntPtr procwritev = default;

public static (System.UIntPtr, error) Writev(nint fd, slice<syscall.Iovec> iovs) {
    System.UIntPtr _p0 = default;
    error _p0 = default!;

    ptr<syscall.Iovec> p;
    if (len(iovs) > 0) {
        p = _addr_iovs[0];
    }
    var (n, _, errno) = syscall6(uintptr(@unsafe.Pointer(_addr_procwritev)), 3, uintptr(fd), uintptr(@unsafe.Pointer(p)), uintptr(len(iovs)), 0, 0, 0);
    if (errno != 0) {
        return (0, error.As(errno)!);
    }
    return (n, error.As(null!)!);

}

} // end unix_package
