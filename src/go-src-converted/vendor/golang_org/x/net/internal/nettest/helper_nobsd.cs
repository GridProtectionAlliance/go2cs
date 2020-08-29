// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux solaris

// package nettest -- go2cs converted at 2020 August 29 10:12:05 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\helper_nobsd.go

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
        private static bool supportsIPv6MulticastDeliveryOnLoopback()
        {
            return true;
        }

        private static bool causesIPv6Crash()
        {
            return false;
        }
    }
}}}}}}
