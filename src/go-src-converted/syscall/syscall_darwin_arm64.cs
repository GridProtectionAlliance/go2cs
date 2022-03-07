// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:26:53 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_darwin_arm64.go
using abi = go.@internal.abi_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:sec,Usec:int32(usec));
}

//sys    Fstat(fd int, stat *Stat_t) (err error)
//sys    Fstatfs(fd int, stat *Statfs_t) (err error)
//sysnb    Gettimeofday(tp *Timeval) (err error)
//sys    Lstat(path string, stat *Stat_t) (err error)
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statfs(path string, stat *Statfs_t) (err error)
//sys    fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
//sys    ptrace1(request int, pid int, addr uintptr, data uintptr) (err error) = SYS_ptrace

public static void SetKevent(ptr<Kevent_t> _addr_k, nint fd, nint mode, nint flags) {
    ref Kevent_t k = ref _addr_k.val;

    k.Ident = uint64(fd);
    k.Filter = int16(mode);
    k.Flags = uint16(flags);
}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint64(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = uint32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

private static (nint, error) sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    ref var length = ref heap(uint64(count), out ptr<var> _addr_length);

    var (_, _, e1) = syscall6(abi.FuncPCABI0(libc_sendfile_trampoline), uintptr(infd), uintptr(outfd), uintptr(offset), uintptr(@unsafe.Pointer(_addr_length)), 0, 0);

    written = int(length);

    if (e1 != 0) {
        err = e1;
    }
    return ;

}

private static void libc_sendfile_trampoline();

//go:cgo_import_dynamic libc_sendfile sendfile "/usr/lib/libSystem.B.dylib"

// Implemented in the runtime package (runtime/sys_darwin_64.go)
private static (System.UIntPtr, System.UIntPtr, Errno) syscallX(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);

public static (System.UIntPtr, System.UIntPtr, Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9); // sic

} // end syscall_package
