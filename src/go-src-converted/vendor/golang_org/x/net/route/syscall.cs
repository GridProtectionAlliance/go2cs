// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 August 29 10:12:38 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\syscall.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static System.UIntPtr zero = default;

        private static error sysctl(slice<int> mib, ref byte old, ref System.UIntPtr oldlen, ref byte @new, System.UIntPtr newlen)
        {
            unsafe.Pointer p = default;
            if (len(mib) > 0L)
            {
                p = @unsafe.Pointer(ref mib[0L]);
            }
            else
            {
                p = @unsafe.Pointer(ref zero);
            }
            var (_, _, errno) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(p), uintptr(len(mib)), uintptr(@unsafe.Pointer(old)), uintptr(@unsafe.Pointer(oldlen)), uintptr(@unsafe.Pointer(new)), uintptr(newlen));
            if (errno != 0L)
            {
                return error.As(error(errno));
            }
            return error.As(null);
        }
    }
}}}}}
