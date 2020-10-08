// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !cgo netgo

// package net -- go2cs converted at 2020 October 08 03:31:12 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_stub.go
using context = go.context_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static void init()
        {
            netGo = true;
        }

        private partial struct addrinfoErrno // : long
        {
        }

        private static @string Error(this addrinfoErrno eai)
        {
            return "<nil>";
        }
        private static bool Temporary(this addrinfoErrno eai)
        {
            return false;
        }
        private static bool Timeout(this addrinfoErrno eai)
        {
            return false;
        }

        private static (slice<@string>, error, bool) cgoLookupHost(context.Context ctx, @string name)
        {
            slice<@string> addrs = default;
            error err = default!;
            bool completed = default;

            return (null, error.As(null!)!, false);
        }

        private static (long, error, bool) cgoLookupPort(context.Context ctx, @string network, @string service)
        {
            long port = default;
            error err = default!;
            bool completed = default;

            return (0L, error.As(null!)!, false);
        }

        private static (slice<IPAddr>, error, bool) cgoLookupIP(context.Context ctx, @string network, @string name)
        {
            slice<IPAddr> addrs = default;
            error err = default!;
            bool completed = default;

            return (null, error.As(null!)!, false);
        }

        private static (@string, error, bool) cgoLookupCNAME(context.Context ctx, @string name)
        {
            @string cname = default;
            error err = default!;
            bool completed = default;

            return ("", error.As(null!)!, false);
        }

        private static (slice<@string>, error, bool) cgoLookupPTR(context.Context ctx, @string addr)
        {
            slice<@string> ptrs = default;
            error err = default!;
            bool completed = default;

            return (null, error.As(null!)!, false);
        }
    }
}
