// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fake network poller for NaCl.
// Should never be used, because NaCl network connections do not honor "SetNonblock".

// package runtime -- go2cs converted at 2020 August 29 08:18:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_nacl.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void netpollinit()
        {
        }

        private static System.UIntPtr netpolldescriptor()
        {
            return ~uintptr(0L);
        }

        private static int netpollopen(System.UIntPtr fd, ref pollDesc pd)
        {
            return 0L;
        }

        private static int netpollclose(System.UIntPtr fd)
        {
            return 0L;
        }

        private static void netpollarm(ref pollDesc pd, long mode)
        {
        }

        private static ref g netpoll(bool block)
        {
            return null;
        }
    }
}
