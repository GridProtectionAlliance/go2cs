// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 August 29 10:12:31 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\interface.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        // An InterfaceMessage represents an interface message.
        public partial struct InterfaceMessage
        {
            public long Version; // message version
            public long Type; // message type
            public long Flags; // interface flags
            public long Index; // interface index
            public @string Name; // interface name
            public slice<Addr> Addrs; // addresses

            public long extOff; // offset of header extension
            public slice<byte> raw; // raw message
        }

        // An InterfaceAddrMessage represents an interface address message.
        public partial struct InterfaceAddrMessage
        {
            public long Version; // message version
            public long Type; // message type
            public long Flags; // interface flags
            public long Index; // interface index
            public slice<Addr> Addrs; // addresses

            public slice<byte> raw; // raw message
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ref InterfaceAddrMessage m)
        {
            return null;
        }

        // An InterfaceMulticastAddrMessage represents an interface multicast
        // address message.
        public partial struct InterfaceMulticastAddrMessage
        {
            public long Version; // message version
            public long Type; // messsage type
            public long Flags; // interface flags
            public long Index; // interface index
            public slice<Addr> Addrs; // addresses

            public slice<byte> raw; // raw message
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ref InterfaceMulticastAddrMessage m)
        {
            return null;
        }

        // An InterfaceAnnounceMessage represents an interface announcement
        // message.
        public partial struct InterfaceAnnounceMessage
        {
            public long Version; // message version
            public long Type; // message type
            public long Index; // interface index
            public @string Name; // interface name
            public long What; // what type of announcement

            public slice<byte> raw; // raw message
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ref InterfaceAnnounceMessage m)
        {
            return null;
        }
    }
}}}}}
