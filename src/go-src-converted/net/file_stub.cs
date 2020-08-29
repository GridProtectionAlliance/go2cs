// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl

// package net -- go2cs converted at 2020 August 29 08:26:17 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\file_stub.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (Conn, error) fileConn(ref os.File f)
        {
            return (null, syscall.ENOPROTOOPT);
        }
        private static (Listener, error) fileListener(ref os.File f)
        {
            return (null, syscall.ENOPROTOOPT);
        }
        private static (PacketConn, error) filePacketConn(ref os.File f)
        {
            return (null, syscall.ENOPROTOOPT);
        }
    }
}
