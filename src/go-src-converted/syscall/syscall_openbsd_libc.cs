// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && !mips64
// +build openbsd,!mips64

// package syscall -- go2cs converted at 2022 March 06 22:27:10 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_openbsd_libc.go
using abi = go.@internal.abi_package;

namespace go;

public static partial class syscall_package {

private static void init() {
    execveOpenBSD = execve;
}

//sys directSyscall(trap uintptr, a1 uintptr, a2 uintptr, a3 uintptr, a4 uintptr, a5 uintptr) (ret uintptr, err error) = SYS_syscall

private static (System.UIntPtr, System.UIntPtr, Errno) syscallInternal(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return syscall6X(abi.FuncPCABI0(libc_syscall_trampoline), trap, a1, a2, a3, 0, 0);
}

private static (System.UIntPtr, System.UIntPtr, Errno) syscall6Internal(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return syscall10X(abi.FuncPCABI0(libc_syscall_trampoline), trap, a1, a2, a3, a4, a5, a6, 0, 0, 0);
}

private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscallInternal(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return rawSyscall6X(abi.FuncPCABI0(libc_syscall_trampoline), trap, a1, a2, a3, 0, 0);
}

private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall6Internal(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return rawSyscall10X(abi.FuncPCABI0(libc_syscall_trampoline), trap, a1, a2, a3, a4, a5, a6, 0, 0, 0);
}

private static (System.UIntPtr, System.UIntPtr, Errno) syscall9Internal(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return rawSyscall10X(abi.FuncPCABI0(libc_syscall_trampoline), trap, a1, a2, a3, a4, a5, a6, a7, a8, a9);
}

// Implemented in the runtime package (runtime/sys_openbsd3.go)
private static (System.UIntPtr, System.UIntPtr, Errno) syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
private static (System.UIntPtr, System.UIntPtr, Errno) syscallX(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall10(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall10X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10);
private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall10X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10);

private static (System.UIntPtr, System.UIntPtr, Errno) syscall9(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return syscall10(fn, a1, a2, a3, a4, a5, a6, a7, a8, a9, 0);
}
private static (System.UIntPtr, System.UIntPtr, Errno) syscall9X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return syscall10X(fn, a1, a2, a3, a4, a5, a6, a7, a8, a9, 0);
}

//sys    readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_read
//sys    writelen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_write
//sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = SYS_lseek
//sys    getcwd(buf []byte) (n int, err error)
//sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error)
//sysnb fork() (pid int, err error)
//sysnb ioctl(fd int, req int, arg int) (err error)
//sysnb execve(path *byte, argv **byte, envp **byte) (err error)
//sysnb exit(res int) (err error)
//sys   ptrace(request int, pid int, addr uintptr, data uintptr) (err error)
//sysnb getentropy(p []byte) (err error)
//sys   fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
//sys    fcntlPtr(fd int, cmd int, arg unsafe.Pointer) (val int, err error) = SYS_fcntl
//sys   unlinkat(fd int, path string, flags int) (err error)
//sys   openat(fd int, path string, flags int, perm uint32) (fdret int, err error)

} // end syscall_package
