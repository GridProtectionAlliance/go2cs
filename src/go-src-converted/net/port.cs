// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:14 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\port.go

using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // parsePort parses service as a decimal integer and returns the
        // corresponding value as port. It is the caller's responsibility to
        // parse service as a non-decimal integer when needsLookup is true.
        //
        // Some system resolvers will return a valid port number when given a number
        // over 65536 (see https://golang.org/issues/11715). Alas, the parser
        // can't bail early on numbers > 65536. Therefore reasonably large/small
        // numbers are parsed in full and rejected if invalid.
        private static (long, bool) parsePort(@string service)
        {
            if (service == "")
            { 
                // Lock in the legacy behavior that an empty string
                // means port 0. See golang.org/issue/13610.
                return (0L, false);
            }
            const var max = uint32(1L << (int)(32L) - 1L);
            const var cutoff = uint32(1L << (int)(30L));
            var neg = false;
            if (service[0L] == '+')
            {
                service = service[1L..];
            }
            else if (service[0L] == '-')
            {
                neg = true;
                service = service[1L..];
            }
            uint n = default;
            foreach (var (_, d) in service)
            {
                if ('0' <= d && d <= '9')
                {
                    d -= '0';
                }
                else
                {
                    return (0L, true);
                }
                if (n >= cutoff)
                {
                    n = max;
                    break;
                }
                n *= 10L;
                var nn = n + uint32(d);
                if (nn < n || nn > max)
                {
                    n = max;
                    break;
                }
                n = nn;
            }            if (!neg && n >= cutoff)
            {
                port = int(cutoff - 1L);
            }
            else if (neg && n > cutoff)
            {
                port = int(cutoff);
            }
            else
            {
                port = int(n);
            }
            if (neg)
            {
                port = -port;
            }
            return (port, false);
        }
    }
}
