// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.12

// package route -- go2cs converted at 2020 October 08 03:33:23 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\syscall_go1_11_darwin.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static System.UIntPtr zero = default;

        private static error sysctl(slice<int> mib, ptr<byte> _addr_old, ptr<System.UIntPtr> _addr_oldlen, ptr<byte> _addr_@new, System.UIntPtr newlen)
        {
            ref byte old = ref _addr_old.val;
            ref System.UIntPtr oldlen = ref _addr_oldlen.val;
            ref byte @new = ref _addr_@new.val;

            unsafe.Pointer p = default;
            if (len(mib) > 0L)
            {
                p = @unsafe.Pointer(_addr_mib[0L]);
            }
            else
            {
                p = @unsafe.Pointer(_addr_zero);
            }

            var (_, _, errno) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(p), uintptr(len(mib)), uintptr(@unsafe.Pointer(old)), uintptr(@unsafe.Pointer(oldlen)), uintptr(@unsafe.Pointer(new)), newlen);
            if (errno != 0L)
            {
                return error.As(error(errno))!;
            }

            return error.As(null!)!;

        }
    }
}}}}
