// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// package socktest -- go2cs converted at 2020 October 09 05:00:24 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Go\src\net\internal\socktest\switch_stub.go

using static go.builtin;

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        // Sockets maps a socket descriptor to the status of socket.
        public partial struct Sockets // : map<long, Status>
        {
        }

        private static @string familyString(long family)
        {
            return "<nil>";
        }

        private static @string typeString(long sotype)
        {
            return "<nil>";
        }

        private static @string protocolString(long proto)
        {
            return "<nil>";
        }
    }
}}}
