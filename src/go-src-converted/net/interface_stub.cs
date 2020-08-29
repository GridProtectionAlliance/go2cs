// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl

// package net -- go2cs converted at 2020 August 29 08:26:38 UTC
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
            return (null, null);
        }

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ref Interface ifi)
        {
            return (null, null);
        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ref Interface ifi)
        {
            return (null, null);
        }
    }
}
