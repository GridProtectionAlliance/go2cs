// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin && !ios
// +build darwin,!ios

// package unix -- go2cs converted at 2022 March 06 22:12:54 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\getentropy_darwin.go
using abi = go.@internal.abi_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    //go:cgo_import_dynamic libc_getentropy getentropy "/usr/lib/libSystem.B.dylib"
private static void libc_getentropy_trampoline();

// GetEntropy calls the macOS getentropy system call.
public static error GetEntropy(slice<byte> p) {
    var (_, _, errno) = syscall_syscall(abi.FuncPCABI0(libc_getentropy_trampoline), uintptr(@unsafe.Pointer(_addr_p[0])), uintptr(len(p)), 0);
    if (errno != 0) {>>MARKER:FUNCTION_libc_getentropy_trampoline_BLOCK_PREFIX<<
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

//go:linkname syscall_syscall syscall.syscall
private static (System.UIntPtr, System.UIntPtr, syscall.Errno) syscall_syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);

} // end unix_package
