// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && gccgo && arm
// +build linux,gccgo,arm

// package unix -- go2cs converted at 2022 March 06 23:27:07 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_gccgo_arm.go
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

} // end unix_package
