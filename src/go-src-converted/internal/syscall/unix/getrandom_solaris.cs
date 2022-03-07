// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\getrandom_solaris.go
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    //go:cgo_import_dynamic libc_getrandom getrandom "libc.so"

    //go:linkname procGetrandom libc_getrandom
private static System.UIntPtr procGetrandom = default;

private static int getrandomUnsupported = default; // atomic

// GetRandomFlag is a flag supported by the getrandom system call.
public partial struct GetRandomFlag { // : System.UIntPtr
}

 
// GRND_NONBLOCK means return EAGAIN rather than blocking.
public static readonly GetRandomFlag GRND_NONBLOCK = 0x0001; 

// GRND_RANDOM means use the /dev/random pool instead of /dev/urandom.
public static readonly GetRandomFlag GRND_RANDOM = 0x0002;


// GetRandom calls the getrandom system call.
public static (nint, error) GetRandom(slice<byte> p, GetRandomFlag flags) {
    nint n = default;
    error err = default!;

    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    if (atomic.LoadInt32(_addr_getrandomUnsupported) != 0) {
        return (0, error.As(syscall.ENOSYS)!);
    }
    var (r1, _, errno) = syscall6(uintptr(@unsafe.Pointer(_addr_procGetrandom)), 3, uintptr(@unsafe.Pointer(_addr_p[0])), uintptr(len(p)), uintptr(flags), 0, 0, 0);
    if (errno != 0) {
        if (errno == syscall.ENOSYS) {
            atomic.StoreInt32(_addr_getrandomUnsupported, 1);
        }
        return (0, error.As(errno)!);

    }
    return (int(r1), error.As(null!)!);

}

} // end unix_package
