// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package nettest -- go2cs converted at 2020 August 29 10:12:04 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\helper_bsd.go
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net {
namespace @internal
{
    public static partial class nettest_package
    {
        private static long darwinVersion = default;

        private static void init()
        {
            if (runtime.GOOS == "darwin")
            { 
                // See http://support.apple.com/kb/HT1633.
                var (s, err) = syscall.Sysctl("kern.osrelease");
                if (err != null)
                {
                    return;
                }
                var ss = strings.Split(s, ".");
                if (len(ss) == 0L)
                {
                    return;
                }
                darwinVersion, _ = strconv.Atoi(ss[0L]);
            }
        }

        private static bool supportsIPv6MulticastDeliveryOnLoopback()
        {
            switch (runtime.GOOS)
            {
                case "freebsd": 
                    // See http://www.freebsd.org/cgi/query-pr.cgi?pr=180065.
                    // Even after the fix, it looks like the latest
                    // kernels don't deliver link-local scoped multicast
                    // packets correctly.
                    return false;
                    break;
                case "darwin": 
                    return !causesIPv6Crash();
                    break;
                default: 
                    return true;
                    break;
            }
        }

        private static bool causesIPv6Crash()
        { 
            // We see some kernel crash when running IPv6 with IP-level
            // options on Darwin kernel version 12 or below.
            // See golang.org/issues/17015.
            return darwinVersion < 13L;
        }
    }
}}}}}}
