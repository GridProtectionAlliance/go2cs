// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package net -- go2cs converted at 2020 October 09 04:51:29 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\file_stub.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (Conn, error) fileConn(ptr<os.File> _addr_f)
        {
            Conn _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }
        private static (Listener, error) fileListener(ptr<os.File> _addr_f)
        {
            Listener _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }
        private static (PacketConn, error) filePacketConn(ptr<os.File> _addr_f)
        {
            PacketConn _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }
    }
}
