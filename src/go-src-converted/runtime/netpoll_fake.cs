// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fake network poller for wasm/js.
// Should never be used, because wasm/js network connections do not honor "SetNonblock".

// +build js,wasm

// package runtime -- go2cs converted at 2020 October 09 04:47:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_fake.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void netpollinit()
        {
        }

        private static bool netpollIsPollDescriptor(System.UIntPtr fd)
        {
            return false;
        }

        private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;

            return 0L;
        }

        private static int netpollclose(System.UIntPtr fd)
        {
            return 0L;
        }

        private static void netpollarm(ptr<pollDesc> _addr_pd, long mode)
        {
            ref pollDesc pd = ref _addr_pd.val;

        }

        private static void netpollBreak()
        {
        }

        private static gList netpoll(long delay)
        {
            return new gList();
        }
    }
}
