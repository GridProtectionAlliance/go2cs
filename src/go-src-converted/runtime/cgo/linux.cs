// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Linux system call wrappers that provide POSIX semantics through the
// corresponding cgo->libc (nptl) wrappers for various system calls.

//go:build linux
// +build linux

// package cgo -- go2cs converted at 2022 March 06 22:12:30 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\linux.go
using @unsafe = go.@unsafe_package;

namespace go.runtime;

public static partial class cgo_package {

    // Each of the following entries is needed to ensure that the
    // syscall.syscall_linux code can conditionally call these
    // function pointers:
    //
    //  1. find the C-defined function start
    //  2. force the local byte alias to be mapped to that location
    //  3. map the Go pointer to the function to the syscall package

    //go:cgo_import_static _cgo_libc_setegid
    //go:linkname _cgo_libc_setegid _cgo_libc_setegid
    //go:linkname cgo_libc_setegid syscall.cgo_libc_setegid
private static byte _cgo_libc_setegid = default;
private static var cgo_libc_setegid = @unsafe.Pointer(_addr__cgo_libc_setegid);

//go:cgo_import_static _cgo_libc_seteuid
//go:linkname _cgo_libc_seteuid _cgo_libc_seteuid
//go:linkname cgo_libc_seteuid syscall.cgo_libc_seteuid
private static byte _cgo_libc_seteuid = default;
private static var cgo_libc_seteuid = @unsafe.Pointer(_addr__cgo_libc_seteuid);

//go:cgo_import_static _cgo_libc_setregid
//go:linkname _cgo_libc_setregid _cgo_libc_setregid
//go:linkname cgo_libc_setregid syscall.cgo_libc_setregid
private static byte _cgo_libc_setregid = default;
private static var cgo_libc_setregid = @unsafe.Pointer(_addr__cgo_libc_setregid);

//go:cgo_import_static _cgo_libc_setresgid
//go:linkname _cgo_libc_setresgid _cgo_libc_setresgid
//go:linkname cgo_libc_setresgid syscall.cgo_libc_setresgid
private static byte _cgo_libc_setresgid = default;
private static var cgo_libc_setresgid = @unsafe.Pointer(_addr__cgo_libc_setresgid);

//go:cgo_import_static _cgo_libc_setresuid
//go:linkname _cgo_libc_setresuid _cgo_libc_setresuid
//go:linkname cgo_libc_setresuid syscall.cgo_libc_setresuid
private static byte _cgo_libc_setresuid = default;
private static var cgo_libc_setresuid = @unsafe.Pointer(_addr__cgo_libc_setresuid);

//go:cgo_import_static _cgo_libc_setreuid
//go:linkname _cgo_libc_setreuid _cgo_libc_setreuid
//go:linkname cgo_libc_setreuid syscall.cgo_libc_setreuid
private static byte _cgo_libc_setreuid = default;
private static var cgo_libc_setreuid = @unsafe.Pointer(_addr__cgo_libc_setreuid);

//go:cgo_import_static _cgo_libc_setgroups
//go:linkname _cgo_libc_setgroups _cgo_libc_setgroups
//go:linkname cgo_libc_setgroups syscall.cgo_libc_setgroups
private static byte _cgo_libc_setgroups = default;
private static var cgo_libc_setgroups = @unsafe.Pointer(_addr__cgo_libc_setgroups);

//go:cgo_import_static _cgo_libc_setgid
//go:linkname _cgo_libc_setgid _cgo_libc_setgid
//go:linkname cgo_libc_setgid syscall.cgo_libc_setgid
private static byte _cgo_libc_setgid = default;
private static var cgo_libc_setgid = @unsafe.Pointer(_addr__cgo_libc_setgid);

//go:cgo_import_static _cgo_libc_setuid
//go:linkname _cgo_libc_setuid _cgo_libc_setuid
//go:linkname cgo_libc_setuid syscall.cgo_libc_setuid
private static byte _cgo_libc_setuid = default;
private static var cgo_libc_setuid = @unsafe.Pointer(_addr__cgo_libc_setuid);

} // end cgo_package
