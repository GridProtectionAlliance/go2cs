// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package proxy -- go2cs converted at 2020 August 29 10:12:24 UTC
// import "vendor/golang_org/x/net/proxy" ==> using proxy = go.vendor.golang_org.x.net.proxy_package
// Original source: C:\Go\src\vendor\golang_org\x\net\proxy\direct.go
using net = go.net_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class proxy_package
    {
        private partial struct direct
        {
        }

        // Direct is a direct proxy: one that makes network connections directly.
        public static direct Direct = new direct();

        private static (net.Conn, error) Dial(this direct _p0, @string network, @string addr)
        {
            return net.Dial(network, addr);
        }
    }
}}}}}
