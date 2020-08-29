// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly netbsd openbsd

// package net -- go2cs converted at 2020 August 29 08:26:30 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_bsdvar.go
using syscall = go.syscall_package;

using route = go.golang_org.x.net.route_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (slice<route.Message>, error) interfaceMessages(long ifindex)
        {
            var (rib, err) = route.FetchRIB(syscall.AF_UNSPEC, syscall.NET_RT_IFLIST, ifindex);
            if (err != null)
            {
                return (null, err);
            }
            return route.ParseRIB(syscall.NET_RT_IFLIST, rib);
        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ref Interface ifi)
        { 
            // TODO(mikio): Implement this like other platforms.
            return (null, null);
        }
    }
}
