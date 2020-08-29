// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl

// package net -- go2cs converted at 2020 August 29 08:27:24 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockoptip_stub.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setIPv4MulticastInterface(ref netFD fd, ref Interface ifi)
        { 
            // See golang.org/issue/7399.
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setIPv4MulticastLoopback(ref netFD fd, bool v)
        { 
            // See golang.org/issue/7399.
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error joinIPv4Group(ref netFD fd, ref Interface ifi, IP ip)
        { 
            // See golang.org/issue/7399.
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setIPv6MulticastInterface(ref netFD fd, ref Interface ifi)
        { 
            // See golang.org/issue/7399.
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setIPv6MulticastLoopback(ref netFD fd, bool v)
        { 
            // See golang.org/issue/7399.
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error joinIPv6Group(ref netFD fd, ref Interface ifi, IP ip)
        { 
            // See golang.org/issue/7399.
            return error.As(syscall.ENOPROTOOPT);
        }
    }
}
