// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nettest -- go2cs converted at 2020 August 29 10:12:11 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\rlimit.go

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
        private static readonly long defaultMaxOpenFiles = 256L;

        // MaxOpenFiles returns the maximum number of open files for the
        // caller's process.


        // MaxOpenFiles returns the maximum number of open files for the
        // caller's process.
        public static long MaxOpenFiles()
        {
            return maxOpenFiles();
        }
    }
}}}}}}
