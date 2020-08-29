// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.7

// package nettest -- go2cs converted at 2020 August 29 10:12:24 UTC
// import "vendor/golang_org/x/net/nettest" ==> using nettest = go.vendor.golang_org.x.net.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\nettest\conntest_go16.go
using testing = go.testing_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class nettest_package
    {
        private static void testConn(ref testing.T t, MakePipe mp)
        { 
            // Avoid using subtests on Go 1.6 and below.
            timeoutWrapper(t, mp, testBasicIO);
            timeoutWrapper(t, mp, testPingPong);
            timeoutWrapper(t, mp, testRacyRead);
            timeoutWrapper(t, mp, testRacyWrite);
            timeoutWrapper(t, mp, testReadTimeout);
            timeoutWrapper(t, mp, testWriteTimeout);
            timeoutWrapper(t, mp, testPastTimeout);
            timeoutWrapper(t, mp, testPresentTimeout);
            timeoutWrapper(t, mp, testFutureTimeout);
            timeoutWrapper(t, mp, testCloseTimeout);
            timeoutWrapper(t, mp, testConcurrentMethods);
        }
    }
}}}}}
