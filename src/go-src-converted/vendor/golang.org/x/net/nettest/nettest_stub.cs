// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !aix,!darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd,!solaris,!windows

// package nettest -- go2cs converted at 2020 October 09 06:07:44 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Go\src\vendor\golang.org\x\net\nettest\nettest_stub.go

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
            return false;
        }
    }
}}}}}
