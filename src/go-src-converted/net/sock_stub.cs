// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix js,wasm solaris

// package net -- go2cs converted at 2020 October 09 04:52:23 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_stub.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static long maxListenerBacklog()
        { 
            // TODO: Implement this
            // NOTE: Never return a number bigger than 1<<16 - 1. See issue 5030.
            return syscall.SOMAXCONN;

        }
    }
}
