// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:37:28 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_darwin.go
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
                return ref new RouteMessage(Header:p.Header,Data:b[SizeofRtMsghdr:any.Msglen]);
            else if (any.Type == RTM_IFINFO) 
                p = (InterfaceMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceMessage(Header:p.Header,Data:b[SizeofIfMsghdr:any.Msglen]);
            else if (any.Type == RTM_NEWADDR || any.Type == RTM_DELADDR) 
                p = (InterfaceAddrMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceAddrMessage(Header:p.Header,Data:b[SizeofIfaMsghdr:any.Msglen]);
            else if (any.Type == RTM_NEWMADDR2 || any.Type == RTM_DELMADDR) 
                p = (InterfaceMulticastAddrMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceMulticastAddrMessage(Header:p.Header,Data:b[SizeofIfmaMsghdr2:any.Msglen]);
                        return null;
        }

        // InterfaceMulticastAddrMessage represents a routing message
        // containing network interface address entries.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial struct InterfaceMulticastAddrMessage
        {
            public IfmaMsghdr2 Header;
            public slice<byte> Data;
        }

        private static (slice<Sockaddr>, error) sockaddr(this ref InterfaceMulticastAddrMessage m)
        {
            array<Sockaddr> sas = new array<Sockaddr>(RTAX_MAX);
            var b = m.Data[..];
            for (var i = uint(0L); i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++)
            {
                if (m.Header.Addrs & (1L << (int)(i)) == 0L)
                {
                    continue;
                }
                var rsa = (RawSockaddr.Value)(@unsafe.Pointer(ref b[0L]));

                if (rsa.Family == AF_LINK) 
                    var (sa, err) = parseSockaddrLink(b);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    sas[i] = sa;
                    b = b[rsaAlignOf(int(rsa.Len))..];
                else if (rsa.Family == AF_INET || rsa.Family == AF_INET6) 
                    (sa, err) = parseSockaddrInet(b, rsa.Family);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    sas[i] = sa;
                    b = b[rsaAlignOf(int(rsa.Len))..];
                else 
                    var (sa, l, err) = parseLinkLayerAddr(b);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    sas[i] = sa;
                    b = b[l..];
                            }

            return (sas[..], null);
        }
    }
}
