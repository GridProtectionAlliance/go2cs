// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || netbsd || openbsd
// +build dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2022 March 13 06:46:32 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\route\syscall.go
namespace go.vendor.golang.org.x.net;

using syscall = syscall_package;
using @unsafe = @unsafe_package;

public static partial class route_package {

private static System.UIntPtr zero = default;

private static error sysctl(slice<int> mib, ptr<byte> _addr_old, ptr<System.UIntPtr> _addr_oldlen, ptr<byte> _addr_@new, System.UIntPtr newlen) {
    ref byte old = ref _addr_old.val;
    ref System.UIntPtr oldlen = ref _addr_oldlen.val;
    ref byte @new = ref _addr_@new.val;

    unsafe.Pointer p = default;
    if (len(mib) > 0) {
        p = @unsafe.Pointer(_addr_mib[0]);
    }
    else
 {
        p = @unsafe.Pointer(_addr_zero);
    }
    var (_, _, errno) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(p), uintptr(len(mib)), uintptr(@unsafe.Pointer(old)), uintptr(@unsafe.Pointer(oldlen)), uintptr(@unsafe.Pointer(new)), newlen);
    if (errno != 0) {
        return error.As(error(errno))!;
    }
    return error.As(null!)!;
}

} // end route_package
