// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:37:35 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_openbsd.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static RoutingMessage toRoutingMessage(this ref anyMessage any, slice<byte> b)
        {

            if (any.Type == RTM_ADD || any.Type == RTM_DELETE || any.Type == RTM_CHANGE || any.Type == RTM_GET || any.Type == RTM_LOSING || any.Type == RTM_REDIRECT || any.Type == RTM_MISS || any.Type == RTM_LOCK || any.Type == RTM_RESOLVE) 
                var p = (RouteMessage.Value)(@unsafe.Pointer(any)); 
                // We don't support sockaddr_rtlabel for now.
                p.Header.Addrs &= RTA_DST | RTA_GATEWAY | RTA_NETMASK | RTA_GENMASK | RTA_IFA | RTA_IFP | RTA_BRD | RTA_AUTHOR | RTA_SRC | RTA_SRCMASK;
                return ref new RouteMessage(Header:p.Header,Data:b[p.Header.Hdrlen:any.Msglen]);
            else if (any.Type == RTM_IFINFO) 
                p = (InterfaceMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceMessage(Header:p.Header,Data:b[p.Header.Hdrlen:any.Msglen]);
            else if (any.Type == RTM_IFANNOUNCE) 
                p = (InterfaceAnnounceMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceAnnounceMessage(Header:p.Header);
            else if (any.Type == RTM_NEWADDR || any.Type == RTM_DELADDR) 
                p = (InterfaceAddrMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceAddrMessage(Header:p.Header,Data:b[p.Header.Hdrlen:any.Msglen]);
                        return null;
        }

        // InterfaceAnnounceMessage represents a routing message containing
        // network interface arrival and departure information.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial struct InterfaceAnnounceMessage
        {
            public IfAnnounceMsghdr Header;
        }

        private static (slice<Sockaddr>, error) sockaddr(this ref InterfaceAnnounceMessage m)
        {
            return (null, null);
        }
    }
}
