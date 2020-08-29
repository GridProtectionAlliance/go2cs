// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.7

// package nettest -- go2cs converted at 2020 August 29 10:12:24 UTC
// import "vendor/golang_org/x/net/nettest" ==> using nettest = go.vendor.golang_org.x.net.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\nettest\conntest_go17.go
using testing = go.testing_package;
using static go.builtin;
using System;

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
            // Use subtests on Go 1.7 and above since it is better organized.
            t.Run("BasicIO", t =>
            {
                timeoutWrapper(t, mp, testBasicIO);

            });
            t.Run("PingPong", t =>
            {
                timeoutWrapper(t, mp, testPingPong);

            });
            t.Run("RacyRead", t =>
            {
                timeoutWrapper(t, mp, testRacyRead);

            });
            t.Run("RacyWrite", t =>
            {
                timeoutWrapper(t, mp, testRacyWrite);

            });
            t.Run("ReadTimeout", t =>
            {
                timeoutWrapper(t, mp, testReadTimeout);

            });
            t.Run("WriteTimeout", t =>
            {
                timeoutWrapper(t, mp, testWriteTimeout);

            });
            t.Run("PastTimeout", t =>
            {
                timeoutWrapper(t, mp, testPastTimeout);

            });
            t.Run("PresentTimeout", t =>
            {
                timeoutWrapper(t, mp, testPresentTimeout);

            });
            t.Run("FutureTimeout", t =>
            {
                timeoutWrapper(t, mp, testFutureTimeout);

            });
            t.Run("CloseTimeout", t =>
            {
                timeoutWrapper(t, mp, testCloseTimeout);

            });
            t.Run("ConcurrentMethods", t =>
            {
                timeoutWrapper(t, mp, testConcurrentMethods);

            });
        }
    }
}}}}}
