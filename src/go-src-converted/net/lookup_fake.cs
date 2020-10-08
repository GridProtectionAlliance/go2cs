// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package net -- go2cs converted at 2020 October 08 03:33:52 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\lookup_fake.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) lookupProtocol(context.Context ctx, @string name)
        {
            long proto = default;
            error err = default!;

            return lookupProtocolMap(name);
        }

        private static (slice<@string>, error) lookupHost(this ptr<Resolver> _addr__p0, context.Context ctx, @string host)
        {
            slice<@string> addrs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }

        private static (slice<IPAddr>, error) lookupIP(this ptr<Resolver> _addr__p0, context.Context ctx, @string network, @string host)
        {
            slice<IPAddr> addrs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }

        private static (long, error) lookupPort(this ptr<Resolver> _addr__p0, context.Context ctx, @string network, @string service)
        {
            long port = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return goLookupPort(network, service);
        }

        private static (@string, error) lookupCNAME(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            @string cname = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return ("", error.As(syscall.ENOPROTOOPT)!);
        }

        private static (@string, slice<ptr<SRV>>, error) lookupSRV(this ptr<Resolver> _addr__p0, context.Context ctx, @string service, @string proto, @string name)
        {
            @string cname = default;
            slice<ptr<SRV>> srvs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return ("", null, error.As(syscall.ENOPROTOOPT)!);
        }

        private static (slice<ptr<MX>>, error) lookupMX(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            slice<ptr<MX>> mxs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }

        private static (slice<ptr<NS>>, error) lookupNS(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            slice<ptr<NS>> nss = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }

        private static (slice<@string>, error) lookupTXT(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            slice<@string> txts = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }

        private static (slice<@string>, error) lookupAddr(this ptr<Resolver> _addr__p0, context.Context ctx, @string addr)
        {
            slice<@string> ptrs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            return (null, error.As(syscall.ENOPROTOOPT)!);
        }

        // concurrentThreadsLimit returns the number of threads we permit to
        // run concurrently doing DNS lookups.
        private static long concurrentThreadsLimit()
        {
            return 500L;
        }
    }
}
