// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// Package lif provides basic functions for the manipulation of
// logical network interfaces and interface addresses on Solaris.
//
// The package supports Solaris 11 or above.
// package lif -- go2cs converted at 2020 August 29 10:12:16 UTC
// import "vendor/golang_org/x/net/lif" ==> using lif = go.vendor.golang_org.x.net.lif_package
// Original source: C:\Go\src\vendor\golang_org\x\net\lif\lif.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        private partial struct endpoint
        {
            public long af;
            public System.UIntPtr s;
        }

        private static error close(this ref endpoint ep)
        {
            return error.As(syscall.Close(int(ep.s)));
        }

        private static (slice<endpoint>, error) newEndpoints(long af)
        {
            error lastErr = default;
            slice<endpoint> eps = default;
            long afs = new slice<long>(new long[] { sysAF_INET, sysAF_INET6 });
            if (af != sysAF_UNSPEC)
            {
                afs = new slice<long>(new long[] { af });
            }
            foreach (var (_, af) in afs)
            {
                var (s, err) = syscall.Socket(af, sysSOCK_DGRAM, 0L);
                if (err != null)
                {
                    lastErr = error.As(err);
                    continue;
                }
                eps = append(eps, new endpoint(af:af,s:uintptr(s)));
            }
            if (len(eps) == 0L)
            {
                return (null, lastErr);
            }
            return (eps, null);
        }
    }
}}}}}
