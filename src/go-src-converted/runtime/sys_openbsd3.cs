// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && !mips64
// +build openbsd,!mips64

// package runtime -- go2cs converted at 2022 March 06 22:12:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_openbsd3.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // The X versions of syscall expect the libc call to return a 64-bit result.
    // Otherwise (the non-X version) expects a 32-bit result.
    // This distinction is required because an error is indicated by returning -1,
    // and we need to know whether to check 32 or 64 bits of the result.
    // (Some libc functions that return 32 bits put junk in the upper 32 bits of AX.)

    //go:linkname syscall_syscall syscall.syscall
    //go:nosplit
    //go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(funcPC(syscall)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall();

//go:linkname syscall_syscallX syscall.syscallX
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscallX(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(funcPC(syscallX)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscallX();

//go:linkname syscall_syscall6 syscall.syscall6
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(funcPC(syscall6)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall6();

//go:linkname syscall_syscall6X syscall.syscall6X
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(funcPC(syscall6X)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall6X();

//go:linkname syscall_syscall10 syscall.syscall10
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall10(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(funcPC(syscall10)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall10();

//go:linkname syscall_syscall10X syscall.syscall10X
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall10X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(funcPC(syscall10X)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall10X();

//go:linkname syscall_rawSyscall syscall.rawSyscall
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    libcCall(@unsafe.Pointer(funcPC(syscall)), @unsafe.Pointer(_addr_fn));
    return ;
}

//go:linkname syscall_rawSyscall6 syscall.rawSyscall6
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    libcCall(@unsafe.Pointer(funcPC(syscall6)), @unsafe.Pointer(_addr_fn));
    return ;
}

//go:linkname syscall_rawSyscall6X syscall.rawSyscall6X
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    libcCall(@unsafe.Pointer(funcPC(syscall6X)), @unsafe.Pointer(_addr_fn));
    return ;
}

//go:linkname syscall_rawSyscall10X syscall.rawSyscall10X
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall10X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    libcCall(@unsafe.Pointer(funcPC(syscall10X)), @unsafe.Pointer(_addr_fn));
    return ;
}

} // end runtime_package
