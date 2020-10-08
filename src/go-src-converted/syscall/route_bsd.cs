// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package syscall -- go2cs converted at 2020 October 08 03:26:51 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\route_bsd.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static @string freebsdConfArch = default;        private static var minRoutingSockaddrLen = rsaAlignOf(0L);

        // Round the length of a raw sockaddr up to align it properly.
        private static long rsaAlignOf(long salen)
        {
            var salign = sizeofPtr;
            if (darwin64Bit)
            { 
                // Darwin kernels require 32-bit aligned access to
                // routing facilities.
                salign = 4L;

            }
            else if (netbsd32Bit)
            { 
                // NetBSD 6 and beyond kernels require 64-bit aligned
                // access to routing facilities.
                salign = 8L;

            }
            else if (runtime.GOOS == "freebsd")
            { 
                // In the case of kern.supported_archs="amd64 i386",
                // we need to know the underlying kernel's
                // architecture because the alignment for routing
                // facilities are set at the build time of the kernel.
                if (freebsdConfArch == "amd64")
                {
                    salign = 8L;
                }

            }

            if (salen == 0L)
            {
                return salign;
            }

            return (salen + salign - 1L) & ~(salign - 1L);

        }

        // parseSockaddrLink parses b as a datalink socket address.
        private static (ptr<SockaddrDatalink>, error) parseSockaddrLink(slice<byte> b)
        {
            ptr<SockaddrDatalink> _p0 = default!;
            error _p0 = default!;

            if (len(b) < 8L)
            {
                return (_addr_null!, error.As(EINVAL)!);
            }

            var (sa, _, err) = parseLinkLayerAddr(b[4L..]);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var rsa = (RawSockaddrDatalink.val)(@unsafe.Pointer(_addr_b[0L]));
            sa.Len = rsa.Len;
            sa.Family = rsa.Family;
            sa.Index = rsa.Index;
            return (_addr_sa!, error.As(null!)!);

        }

        // parseLinkLayerAddr parses b as a datalink socket address in
        // conventional BSD kernel form.
        private static (ptr<SockaddrDatalink>, long, error) parseLinkLayerAddr(slice<byte> b)
        {
            ptr<SockaddrDatalink> _p0 = default!;
            long _p0 = default;
            error _p0 = default!;
 
            // The encoding looks like the following:
            // +----------------------------+
            // | Type             (1 octet) |
            // +----------------------------+
            // | Name length      (1 octet) |
            // +----------------------------+
            // | Address length   (1 octet) |
            // +----------------------------+
            // | Selector length  (1 octet) |
            // +----------------------------+
            // | Data            (variable) |
            // +----------------------------+
            private partial struct linkLayerAddr
            {
                public byte Type;
                public byte Nlen;
                public byte Alen;
                public byte Slen;
            }
            var lla = (linkLayerAddr.val)(@unsafe.Pointer(_addr_b[0L]));
            long l = 4L + int(lla.Nlen) + int(lla.Alen) + int(lla.Slen);
            if (len(b) < l)
            {
                return (_addr_null!, 0L, error.As(EINVAL)!);
            }

            b = b[4L..];
            ptr<SockaddrDatalink> sa = addr(new SockaddrDatalink(Type:lla.Type,Nlen:lla.Nlen,Alen:lla.Alen,Slen:lla.Slen));
            for (long i = 0L; len(sa.Data) > i && i < l - 4L; i++)
            {
                sa.Data[i] = int8(b[i]);
            }

            return (_addr_sa!, rsaAlignOf(l), error.As(null!)!);

        }

        // parseSockaddrInet parses b as an internet socket address.
        private static (Sockaddr, error) parseSockaddrInet(slice<byte> b, byte family)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;


            if (family == AF_INET) 
                if (len(b) < SizeofSockaddrInet4)
                {
                    return (null, error.As(EINVAL)!);
                }

                var rsa = (RawSockaddrAny.val)(@unsafe.Pointer(_addr_b[0L]));
                return anyToSockaddr(rsa);
            else if (family == AF_INET6) 
                if (len(b) < SizeofSockaddrInet6)
                {
                    return (null, error.As(EINVAL)!);
                }

                rsa = (RawSockaddrAny.val)(@unsafe.Pointer(_addr_b[0L]));
                return anyToSockaddr(rsa);
            else 
                return (null, error.As(EINVAL)!);
            
        }

        private static readonly var offsetofInet4 = (var)int(@unsafe.Offsetof(new RawSockaddrInet4().Addr));
        private static readonly var offsetofInet6 = (var)int(@unsafe.Offsetof(new RawSockaddrInet6().Addr));


        // parseNetworkLayerAddr parses b as an internet socket address in
        // conventional BSD kernel form.
        private static (Sockaddr, error) parseNetworkLayerAddr(slice<byte> b, byte family)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;
 
            // The encoding looks similar to the NLRI encoding.
            // +----------------------------+
            // | Length           (1 octet) |
            // +----------------------------+
            // | Address prefix  (variable) |
            // +----------------------------+
            //
            // The differences between the kernel form and the NLRI
            // encoding are:
            //
            // - The length field of the kernel form indicates the prefix
            //   length in bytes, not in bits
            //
            // - In the kernel form, zero value of the length field
            //   doesn't mean 0.0.0.0/0 or ::/0
            //
            // - The kernel form appends leading bytes to the prefix field
            //   to make the <length, prefix> tuple to be conformed with
            //   the routing message boundary
            var l = int(rsaAlignOf(int(b[0L])));
            if (len(b) < l)
            {
                return (null, error.As(EINVAL)!);
            } 
            // Don't reorder case expressions.
            // The case expressions for IPv6 must come first.

            if (b[0L] == SizeofSockaddrInet6) 
                ptr<SockaddrInet6> sa = addr(new SockaddrInet6());
                copy(sa.Addr[..], b[offsetofInet6..]);
                return (sa, error.As(null!)!);
            else if (family == AF_INET6) 
                sa = addr(new SockaddrInet6());
                if (l - 1L < offsetofInet6)
                {
                    copy(sa.Addr[..], b[1L..l]);
                }
                else
                {
                    copy(sa.Addr[..], b[l - offsetofInet6..l]);
                }

                return (sa, error.As(null!)!);
            else if (b[0L] == SizeofSockaddrInet4) 
                sa = addr(new SockaddrInet4());
                copy(sa.Addr[..], b[offsetofInet4..]);
                return (sa, error.As(null!)!);
            else // an old fashion, AF_UNSPEC or unknown means AF_INET
                sa = addr(new SockaddrInet4());
                if (l - 1L < offsetofInet4)
                {
                    copy(sa.Addr[..], b[1L..l]);
                }
                else
                {
                    copy(sa.Addr[..], b[l - offsetofInet4..l]);
                }

                return (sa, error.As(null!)!);
            
        }

        // RouteRIB returns routing information base, as known as RIB,
        // which consists of network facility information, states and
        // parameters.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public static (slice<byte>, error) RouteRIB(long facility, long param)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            _C_int mib = new slice<_C_int>(new _C_int[] { CTL_NET, AF_ROUTE, 0, 0, _C_int(facility), _C_int(param) }); 
            // Find size.
            ref var n = ref heap(uintptr(0L), out ptr<var> _addr_n);
            {
                var err__prev1 = err;

                var err = sysctl(mib, null, _addr_n, null, 0L);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            if (n == 0L)
            {
                return (null, error.As(null!)!);
            }

            var tab = make_slice<byte>(n);
            {
                var err__prev1 = err;

                err = sysctl(mib, _addr_tab[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            return (tab[..n], error.As(null!)!);

        }

        // RoutingMessage represents a routing message.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial interface RoutingMessage
        {
            (slice<Sockaddr>, error) sockaddr();
        }

        private static readonly var anyMessageLen = (var)int(@unsafe.Sizeof(new anyMessage()));



        private partial struct anyMessage
        {
            public ushort Msglen;
            public byte Version;
            public byte Type;
        }

        // RouteMessage represents a routing message containing routing
        // entries.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial struct RouteMessage
        {
            public RtMsghdr Header;
            public slice<byte> Data;
        }

        private static (slice<Sockaddr>, error) sockaddr(this ptr<RouteMessage> _addr_m)
        {
            slice<Sockaddr> _p0 = default;
            error _p0 = default!;
            ref RouteMessage m = ref _addr_m.val;

            array<Sockaddr> sas = new array<Sockaddr>(RTAX_MAX);
            var b = m.Data[..];
            var family = uint8(AF_UNSPEC);
            for (var i = uint(0L); i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++)
            {
                if (m.Header.Addrs & (1L << (int)(i)) == 0L)
                {
                    continue;
                }

                var rsa = (RawSockaddr.val)(@unsafe.Pointer(_addr_b[0L]));

                if (rsa.Family == AF_LINK) 
                    var (sa, err) = parseSockaddrLink(b);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    sas[i] = sa;
                    b = b[rsaAlignOf(int(rsa.Len))..];
                else if (rsa.Family == AF_INET || rsa.Family == AF_INET6) 
                    (sa, err) = parseSockaddrInet(b, rsa.Family);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    sas[i] = sa;
                    b = b[rsaAlignOf(int(rsa.Len))..];
                    family = rsa.Family;
                else 
                    (sa, err) = parseNetworkLayerAddr(b, family);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    sas[i] = sa;
                    b = b[rsaAlignOf(int(b[0L]))..];
                
            }

            return (sas[..], error.As(null!)!);

        }

        // InterfaceMessage represents a routing message containing
        // network interface entries.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial struct InterfaceMessage
        {
            public IfMsghdr Header;
            public slice<byte> Data;
        }

        private static (slice<Sockaddr>, error) sockaddr(this ptr<InterfaceMessage> _addr_m)
        {
            slice<Sockaddr> _p0 = default;
            error _p0 = default!;
            ref InterfaceMessage m = ref _addr_m.val;

            array<Sockaddr> sas = new array<Sockaddr>(RTAX_MAX);
            if (m.Header.Addrs & RTA_IFP == 0L)
            {
                return (null, error.As(null!)!);
            }

            var (sa, err) = parseSockaddrLink(m.Data[..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            sas[RTAX_IFP] = sa;
            return (sas[..], error.As(null!)!);

        }

        // InterfaceAddrMessage represents a routing message containing
        // network interface address entries.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public partial struct InterfaceAddrMessage
        {
            public IfaMsghdr Header;
            public slice<byte> Data;
        }

        private static (slice<Sockaddr>, error) sockaddr(this ptr<InterfaceAddrMessage> _addr_m)
        {
            slice<Sockaddr> _p0 = default;
            error _p0 = default!;
            ref InterfaceAddrMessage m = ref _addr_m.val;

            array<Sockaddr> sas = new array<Sockaddr>(RTAX_MAX);
            var b = m.Data[..];
            var family = uint8(AF_UNSPEC);
            for (var i = uint(0L); i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++)
            {
                if (m.Header.Addrs & (1L << (int)(i)) == 0L)
                {
                    continue;
                }

                var rsa = (RawSockaddr.val)(@unsafe.Pointer(_addr_b[0L]));

                if (rsa.Family == AF_LINK) 
                    var (sa, err) = parseSockaddrLink(b);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    sas[i] = sa;
                    b = b[rsaAlignOf(int(rsa.Len))..];
                else if (rsa.Family == AF_INET || rsa.Family == AF_INET6) 
                    (sa, err) = parseSockaddrInet(b, rsa.Family);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    sas[i] = sa;
                    b = b[rsaAlignOf(int(rsa.Len))..];
                    family = rsa.Family;
                else 
                    (sa, err) = parseNetworkLayerAddr(b, family);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    sas[i] = sa;
                    b = b[rsaAlignOf(int(b[0L]))..];
                
            }

            return (sas[..], error.As(null!)!);

        }

        // ParseRoutingMessage parses b as routing messages and returns the
        // slice containing the RoutingMessage interfaces.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public static (slice<RoutingMessage>, error) ParseRoutingMessage(slice<byte> b)
        {
            slice<RoutingMessage> msgs = default;
            error err = default!;

            long nmsgs = 0L;
            long nskips = 0L;
            while (len(b) >= anyMessageLen)
            {
                nmsgs++;
                var any = (anyMessage.val)(@unsafe.Pointer(_addr_b[0L]));
                if (any.Version != RTM_VERSION)
                {
                    b = b[any.Msglen..];
                    continue;
                }

                {
                    var m = any.toRoutingMessage(b);

                    if (m == null)
                    {
                        nskips++;
                    }
                    else
                    {
                        msgs = append(msgs, m);
                    }

                }

                b = b[any.Msglen..];

            } 
            // We failed to parse any of the messages - version mismatch?
 
            // We failed to parse any of the messages - version mismatch?
            if (nmsgs != len(msgs) + nskips)
            {
                return (null, error.As(EINVAL)!);
            }

            return (msgs, error.As(null!)!);

        }

        // ParseRoutingSockaddr parses msg's payload as raw sockaddrs and
        // returns the slice containing the Sockaddr interfaces.
        //
        // Deprecated: Use golang.org/x/net/route instead.
        public static (slice<Sockaddr>, error) ParseRoutingSockaddr(RoutingMessage msg)
        {
            slice<Sockaddr> _p0 = default;
            error _p0 = default!;

            var (sas, err) = msg.sockaddr();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (sas, error.As(null!)!);

        }
    }
}
