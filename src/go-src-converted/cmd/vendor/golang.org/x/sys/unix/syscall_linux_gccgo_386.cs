// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && gccgo && 386
// +build linux,gccgo,386

// package unix -- go2cs converted at 2022 March 06 23:27:06 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_gccgo_386.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static (long, syscall.Errno) seek(nint fd, long offset, nint whence) {
    long _p0 = default;
    syscall.Errno _p0 = default;

    ref long newoffset = ref heap(out ptr<long> _addr_newoffset);
    var offsetLow = uint32(offset & 0xffffffff);
    var offsetHigh = uint32((offset >> 32) & 0xffffffff);
    var (_, _, err) = Syscall6(SYS__LLSEEK, uintptr(fd), uintptr(offsetHigh), uintptr(offsetLow), uintptr(@unsafe.Pointer(_addr_newoffset)), uintptr(whence), 0);
    return (newoffset, err);
}

private static (nint, syscall.Errno) socketcall(nint call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5) {
    nint _p0 = default;
    syscall.Errno _p0 = default;

    var (fd, _, err) = Syscall(SYS_SOCKETCALL, uintptr(call), uintptr(@unsafe.Pointer(_addr_a0)), 0);
    return (int(fd), err);
}

private static (nint, syscall.Errno) rawsocketcall(nint call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5) {
    nint _p0 = default;
    syscall.Errno _p0 = default;

    var (fd, _, err) = RawSyscall(SYS_SOCKETCALL, uintptr(call), uintptr(@unsafe.Pointer(_addr_a0)), 0);
    return (int(fd), err);
}

} // end unix_package
