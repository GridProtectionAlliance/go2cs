// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:34:34 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_plan9.go

using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static long maxListenerBacklog()
        { 
            // /sys/include/ape/sys/socket.h:/SOMAXCONN
            return 5L;

        }
    }
}
