// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl plan9

// package nettest -- go2cs converted at 2020 August 29 10:12:06 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\helper_stub.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
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
        private static long maxOpenFiles()
        {
            return defaultMaxOpenFiles;
        }

        private static (@string, bool) supportsRawIPSocket()
        {
            return (fmt.Sprintf("not supported on %s", runtime.GOOS), false);
        }

        private static bool supportsIPv6MulticastDeliveryOnLoopback()
        {
            return false;
        }

        private static bool causesIPv6Crash()
        {
            return false;
        }

        private static bool protocolNotSupported(error err)
        {
            return false;
        }
    }
}}}}}}
