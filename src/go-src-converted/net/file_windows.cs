// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:21 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\file_windows.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (Conn, error) fileConn(ref os.File f)
        { 
            // TODO: Implement this
            return (null, syscall.EWINDOWS);
        }

        private static (Listener, error) fileListener(ref os.File f)
        { 
            // TODO: Implement this
            return (null, syscall.EWINDOWS);
        }

        private static (PacketConn, error) filePacketConn(ref os.File f)
        { 
            // TODO: Implement this
            return (null, syscall.EWINDOWS);
        }
    }
}
