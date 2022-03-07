// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:54 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\getentropy_openbsd.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    // getentropy(2)'s syscall number, from /usr/src/sys/kern/syscalls.master
private static readonly System.UIntPtr entropyTrap = 7;

// GetEntropy calls the OpenBSD getentropy system call.


// GetEntropy calls the OpenBSD getentropy system call.
public static error GetEntropy(slice<byte> p) {
    var (_, _, errno) = syscall.Syscall(entropyTrap, uintptr(@unsafe.Pointer(_addr_p[0])), uintptr(len(p)), 0);
    if (errno != 0) {
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

} // end unix_package
