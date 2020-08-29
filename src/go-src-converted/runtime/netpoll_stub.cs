// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// package runtime -- go2cs converted at 2020 August 29 08:18:38 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_stub.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static uint netpollWaiters = default;

        // Polls for ready network connections.
        // Returns list of goroutines that become runnable.
        private static ref g netpoll(bool block)
        { 
            // Implementation for platforms that do not support
            // integrated network poller.
            return;
        }

        private static bool netpollinited()
        {
            return false;
        }
    }
}
