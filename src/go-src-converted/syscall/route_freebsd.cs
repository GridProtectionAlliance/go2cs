// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:37:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_freebsd.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // See http://www.freebsd.org/doc/en/books/porters-handbook/freebsd-versions.html.
        private static uint freebsdVersion = default;

        private static void init()
        {
            freebsdVersion, _ = SysctlUint32("kern.osreldate");
            var (conf, _) = Sysctl("kern.conftxt");
            for (long i = 0L;
            long j = 0L; j < len(conf); j++)
            {
                if (conf[j] != '\n')
                {
                    continue;
                }
                var s = conf[i..j];
                i = j + 1L;
                if (len(s) > len("machine") && s[..len("machine")] == "machine")
                {
                    s = s[len("machine")..];
                    for (long k = 0L; k < len(s); k++)
                    {
                        if (s[k] == ' ' || s[k] == '\t')
                        {
                            s = s[1L..];
                        }
                        break;
                    }

                    freebsdConfArch = s;
                    break;
                }
            }

        }

        private static RoutingMessage toRoutingMessage(this ref anyMessage any, slice<byte> b)
        {

            if (any.Type == RTM_ADD || any.Type == RTM_DELETE || any.Type == RTM_CHANGE || any.Type == RTM_GET || any.Type == RTM_LOSING || any.Type == RTM_REDIRECT || any.Type == RTM_MISS || any.Type == RTM_LOCK || any.Type == RTM_RESOLVE) 
                return any.parseRouteMessage(b);
            else if (any.Type == RTM_IFINFO) 
                return any.parseInterfaceMessage(b);
            else if (any.Type == RTM_IFANNOUNCE) 
                var p = (InterfaceAnnounceMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceAnnounceMessage(Header:p.Header);
            else if (any.Type == RTM_NEWADDR || any.Type == RTM_DELADDR) 
                p = (InterfaceAddrMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceAddrMessage(Header:p.Header,Data:b[SizeofIfaMsghdr:any.Msglen]);
            else if (any.Type == RTM_NEWMADDR || any.Type == RTM_DELMADDR) 
                p = (InterfaceMulticastAddrMessage.Value)(@unsafe.Pointer(any));
                return ref new InterfaceMulticastAddrMessage(Header:p.Header,Data:b[SizeofIfmaMsghdr:any.Msglen]);
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

        // InterfaceMulticastAddrMessage represents a routing message
        // containing network interface address entries.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial struct InterfaceMulticastAddrMessage
        {
            public IfmaMsghdr Header;
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
