// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package socktest -- go2cs converted at 2020 August 29 08:36:19 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Go\src\net\internal\socktest\switch_windows.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        // Sockets maps a socket descriptor to the status of socket.
        public partial struct Sockets // : map<syscall.Handle, Status>
        {
        }

        private static ref Status sockso(this ref Switch _sw, syscall.Handle s) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            sw.smu.RLock();
            defer(sw.smu.RUnlock());
            var (so, ok) = sw.sotab[s];
            if (!ok)
            {
                return null;
            }
            return ref so;
        });

        // addLocked returns a new Status without locking.
        // sw.smu must be held before call.
        private static ref Status addLocked(this ref Switch sw, syscall.Handle s, long family, long sotype, long proto)
        {
            sw.once.Do(sw.init);
            Status so = new Status(Cookie:cookie(family,sotype,proto));
            sw.sotab[s] = so;
            return ref so;
        }
    }
}}}
