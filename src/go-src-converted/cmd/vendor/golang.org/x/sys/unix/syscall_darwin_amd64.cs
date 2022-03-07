// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 && darwin
// +build amd64,darwin

// package unix -- go2cs converted at 2022 March 06 23:26:48 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin_amd64.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:sec,Usec:int32(usec));
}

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

private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Iovlen = int32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9);

//sys    Fstat(fd int, stat *Stat_t) (err error) = SYS_FSTAT64
//sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error) = SYS_FSTATAT64
//sys    Fstatfs(fd int, stat *Statfs_t) (err error) = SYS_FSTATFS64
//sys    getfsstat(buf unsafe.Pointer, size uintptr, flags int) (n int, err error) = SYS_GETFSSTAT64
//sys    Lstat(path string, stat *Stat_t) (err error) = SYS_LSTAT64
//sys    ptrace1(request int, pid int, addr uintptr, data uintptr) (err error) = SYS_ptrace
//sys    Stat(path string, stat *Stat_t) (err error) = SYS_STAT64
//sys    Statfs(path string, stat *Statfs_t) (err error) = SYS_STATFS64

} // end unix_package
