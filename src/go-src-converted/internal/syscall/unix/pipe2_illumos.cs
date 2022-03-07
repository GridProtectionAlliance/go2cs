// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build illumos
// +build illumos

// package unix -- go2cs converted at 2022 March 06 22:12:56 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\pipe2_illumos.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    //go:cgo_import_dynamic libc_pipe2 pipe2 "libc.so"

    //go:linkname procpipe2 libc_pipe2
private static System.UIntPtr procpipe2 = default;

private partial struct _C_int { // : int
}

public static error Pipe2(slice<nint> p, nint flags) {
    if (len(p) != 2) {
        return error.As(syscall.EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    var (_, _, errno) = syscall6(uintptr(@unsafe.Pointer(_addr_procpipe2)), 2, uintptr(@unsafe.Pointer(_addr_pp)), uintptr(flags), 0, 0, 0, 0);
    if (errno != 0) {
        return error.As(errno)!;
    }
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return error.As(null!)!;

}

} // end unix_package
