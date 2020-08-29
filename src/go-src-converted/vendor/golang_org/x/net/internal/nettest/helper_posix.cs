// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package nettest -- go2cs converted at 2020 August 29 10:12:06 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\helper_posix.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net {
namespace @internal
{
    public static partial class nettest_package
    {
        private static bool protocolNotSupported(error err)
        {
            switch (err.type())
            {
                case syscall.Errno err:

                    if (err == syscall.EPROTONOSUPPORT || err == syscall.ENOPROTOOPT) 
                        return true;
                                        break;
                case ref os.SyscallError err:
                    switch (err.Err.type())
                    {
                        case syscall.Errno err:

                            if (err == syscall.EPROTONOSUPPORT || err == syscall.ENOPROTOOPT) 
                                return true;
                                                        break;
                    }
                    break;
            }
            return false;
        }
    }
}}}}}}
