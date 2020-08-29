// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl

// package net -- go2cs converted at 2020 August 29 08:26:57 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\lookup_nacl.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) lookupProtocol(context.Context ctx, @string name)
        {
            return lookupProtocolMap(name);
        }

        private static (slice<@string>, error) lookupHost(this ref Resolver _p0, context.Context ctx, @string host)
        {
            return (null, syscall.ENOPROTOOPT);
        }

        private static (slice<IPAddr>, error) lookupIP(this ref Resolver _p0, context.Context ctx, @string host)
        {
            return (null, syscall.ENOPROTOOPT);
        }

        private static (long, error) lookupPort(this ref Resolver _p0, context.Context ctx, @string network, @string service)
        {
            return goLookupPort(network, service);
        }

        private static (@string, error) lookupCNAME(this ref Resolver _p0, context.Context ctx, @string name)
        {
            return ("", syscall.ENOPROTOOPT);
        }

        private static (@string, slice<ref SRV>, error) lookupSRV(this ref Resolver _p0, context.Context ctx, @string service, @string proto, @string name)
        {
            return ("", null, syscall.ENOPROTOOPT);
        }

        private static (slice<ref MX>, error) lookupMX(this ref Resolver _p0, context.Context ctx, @string name)
        {
            return (null, syscall.ENOPROTOOPT);
        }

        private static (slice<ref NS>, error) lookupNS(this ref Resolver _p0, context.Context ctx, @string name)
        {
            return (null, syscall.ENOPROTOOPT);
        }

        private static (slice<@string>, error) lookupTXT(this ref Resolver _p0, context.Context ctx, @string name)
        {
            return (null, syscall.ENOPROTOOPT);
        }

        private static (slice<@string>, error) lookupAddr(this ref Resolver _p0, context.Context ctx, @string addr)
        {
            return (null, syscall.ENOPROTOOPT);
        }
    }
}
