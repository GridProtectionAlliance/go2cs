// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package net -- go2cs converted at 2020 October 09 04:51:49 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_stub.go

using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex)
        {
            slice<Interface> _p0 = default;
            error _p0 = default!;

            return (null, error.As(null!)!);
        }

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            return (null, error.As(null!)!);
        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            return (null, error.As(null!)!);
        }
    }
}
