// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package nettest -- go2cs converted at 2020 October 08 05:01:32 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Go\src\vendor\golang.org\x\net\nettest\nettest_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class nettest_package
    {
        private static bool supportsRawSocket()
        {
            foreach (var (_, af) in new slice<long>(new long[] { syscall.AF_INET, syscall.AF_INET6 }))
            {
                var (s, err) = syscall.Socket(af, syscall.SOCK_RAW, 0L);
                if (err != null)
                {
                    continue;
                }
                syscall.Close(s);
                return true;

            }            return false;

        }
    }
}}}}}
