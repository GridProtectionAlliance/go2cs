// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 August 29 10:12:29 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\address.go
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        // An Addr represents an address associated with packet routing.
        public partial interface Addr
        {
            long Family();
        }

        // A LinkAddr represents a link-layer address.
        public partial struct LinkAddr
        {
            public long Index; // interface index when attached
            public @string Name; // interface name when attached
            public slice<byte> Addr; // link-layer address when attached
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ref LinkAddr a)
        {
            return sysAF_LINK;
        }

        private static (long, long) lenAndSpace(this ref LinkAddr a)
        {
            long l = 8L + len(a.Name) + len(a.Addr);
            return (l, roundup(l));
        }

        private static (long, error) marshal(this ref LinkAddr a, slice<byte> b)
        {
            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, errShortBuffer);
            }
            var nlen = len(a.Name);
            var alen = len(a.Addr);
            if (nlen > 255L || alen > 255L)
            {
                return (0L, errInvalidAddr);
            }
            b[0L] = byte(l);
            b[1L] = sysAF_LINK;
            if (a.Index > 0L)
            {
                nativeEndian.PutUint16(b[2L..4L], uint16(a.Index));
            }
            var data = b[8L..];
            if (nlen > 0L)
            {
                b[5L] = byte(nlen);
                copy(data[..nlen], a.Addr);
                data = data[nlen..];
            }
            if (alen > 0L)
            {
                b[6L] = byte(alen);
                copy(data[..alen], a.Name);
                data = data[alen..];
            }
            return (ll, null);
        }

        private static (Addr, error) parseLinkAddr(slice<byte> b)
        {
            if (len(b) < 8L)
            {
                return (null, errInvalidAddr);
            }
            var (_, a, err) = parseKernelLinkAddr(sysAF_LINK, b[4L..]);
            if (err != null)
            {
                return (null, err);
            }
            a._<ref LinkAddr>().Index = int(nativeEndian.Uint16(b[2L..4L]));
            return (a, null);
        }

        // parseKernelLinkAddr parses b as a link-layer address in
        // conventional BSD kernel form.
        private static (long, Addr, error) parseKernelLinkAddr(long _, slice<byte> b)
        { 
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
            //
            // On some platforms, all-bit-one of length field means "don't
            // care".
            var nlen = int(b[1L]);
            var alen = int(b[2L]);
            var slen = int(b[3L]);
            if (nlen == 0xffUL)
            {
                nlen = 0L;
            }
            if (alen == 0xffUL)
            {
                alen = 0L;
            }
            if (slen == 0xffUL)
            {
                slen = 0L;
            }
            long l = 4L + nlen + alen + slen;
            if (len(b) < l)
            {
                return (0L, null, errInvalidAddr);
            }
            var data = b[4L..];
            @string name = default;
            slice<byte> addr = default;
            if (nlen > 0L)
            {
                name = string(data[..nlen]);
                data = data[nlen..];
            }
            if (alen > 0L)
            {
                addr = data[..alen];
                data = data[alen..];
            }
            return (l, ref new LinkAddr(Name:name,Addr:addr), null);
        }

        // An Inet4Addr represents an internet address for IPv4.
        public partial struct Inet4Addr
        {
            public array<byte> IP; // IP address
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ref Inet4Addr a)
        {
            return sysAF_INET;
        }

        private static (long, long) lenAndSpace(this ref Inet4Addr a)
        {
            return (sizeofSockaddrInet, roundup(sizeofSockaddrInet));
        }

        private static (long, error) marshal(this ref Inet4Addr a, slice<byte> b)
        {
            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, errShortBuffer);
            }
            b[0L] = byte(l);
            b[1L] = sysAF_INET;
            copy(b[4L..8L], a.IP[..]);
            return (ll, null);
        }

        // An Inet6Addr represents an internet address for IPv6.
        public partial struct Inet6Addr
        {
            public array<byte> IP; // IP address
            public long ZoneID; // zone identifier
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ref Inet6Addr a)
        {
            return sysAF_INET6;
        }

        private static (long, long) lenAndSpace(this ref Inet6Addr a)
        {
            return (sizeofSockaddrInet6, roundup(sizeofSockaddrInet6));
        }

        private static (long, error) marshal(this ref Inet6Addr a, slice<byte> b)
        {
            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, errShortBuffer);
            }
            b[0L] = byte(l);
            b[1L] = sysAF_INET6;
            copy(b[8L..24L], a.IP[..]);
            if (a.ZoneID > 0L)
            {
                nativeEndian.PutUint32(b[24L..28L], uint32(a.ZoneID));
            }
            return (ll, null);
        }

        // parseInetAddr parses b as an internet address for IPv4 or IPv6.
        private static (Addr, error) parseInetAddr(long af, slice<byte> b)
        {

            if (af == sysAF_INET) 
                if (len(b) < sizeofSockaddrInet)
                {
                    return (null, errInvalidAddr);
                }
                Inet4Addr a = ref new Inet4Addr();
                copy(a.IP[..], b[4L..8L]);
                return (a, null);
            else if (af == sysAF_INET6) 
                if (len(b) < sizeofSockaddrInet6)
                {
                    return (null, errInvalidAddr);
                }
                a = ref new Inet6Addr(ZoneID:int(nativeEndian.Uint32(b[24:28])));
                copy(a.IP[..], b[8L..24L]);
                if (a.IP[0L] == 0xfeUL && a.IP[1L] & 0xc0UL == 0x80UL || a.IP[0L] == 0xffUL && (a.IP[1L] & 0x0fUL == 0x01UL || a.IP[1L] & 0x0fUL == 0x02UL))
                { 
                    // KAME based IPv6 protocol stack usually
                    // embeds the interface index in the
                    // interface-local or link-local address as
                    // the kernel-internal form.
                    var id = int(bigEndian.Uint16(a.IP[2L..4L]));
                    if (id != 0L)
                    {
                        a.ZoneID = id;
                        a.IP[2L] = 0L;
                        a.IP[3L] = 0L;
                    }
                }
                return (a, null);
            else 
                return (null, errInvalidAddr);
                    }

        // parseKernelInetAddr parses b as an internet address in conventional
        // BSD kernel form.
        private static (long, Addr, error) parseKernelInetAddr(long af, slice<byte> b)
        { 
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
            var l = int(b[0L]);
            if (runtime.GOOS == "darwin")
            { 
                // On Darwn, an address in the kernel form is also
                // used as a message filler.
                if (l == 0L || len(b) > roundup(l))
                {
                    l = roundup(l);
                }
            }
            else
            {
                l = roundup(l);
            }
            if (len(b) < l)
            {
                return (0L, null, errInvalidAddr);
            } 
            // Don't reorder case expressions.
            // The case expressions for IPv6 must come first.
            const long off4 = 4L; // offset of in_addr
            const long off6 = 8L; // offset of in6_addr

            if (b[0L] == sizeofSockaddrInet6) 
                Inet6Addr a = ref new Inet6Addr();
                copy(a.IP[..], b[off6..off6 + 16L]);
                return (int(b[0L]), a, null);
            else if (af == sysAF_INET6) 
                a = ref new Inet6Addr();
                if (l - 1L < off6)
                {
                    copy(a.IP[..], b[1L..l]);
                }
                else
                {
                    copy(a.IP[..], b[l - off6..l]);
                }
                return (int(b[0L]), a, null);
            else if (b[0L] == sizeofSockaddrInet) 
                a = ref new Inet4Addr();
                copy(a.IP[..], b[off4..off4 + 4L]);
                return (int(b[0L]), a, null);
            else // an old fashion, AF_UNSPEC or unknown means AF_INET
                a = ref new Inet4Addr();
                if (l - 1L < off4)
                {
                    copy(a.IP[..], b[1L..l]);
                }
                else
                {
                    copy(a.IP[..], b[l - off4..l]);
                }
                return (int(b[0L]), a, null);
                    }

        // A DefaultAddr represents an address of various operating
        // system-specific features.
        public partial struct DefaultAddr
        {
            public long af;
            public slice<byte> Raw; // raw format of address
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ref DefaultAddr a)
        {
            return a.af;
        }

        private static (long, long) lenAndSpace(this ref DefaultAddr a)
        {
            var l = len(a.Raw);
            return (l, roundup(l));
        }

        private static (long, error) marshal(this ref DefaultAddr a, slice<byte> b)
        {
            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, errShortBuffer);
            }
            if (l > 255L)
            {
                return (0L, errInvalidAddr);
            }
            b[1L] = byte(l);
            copy(b[..l], a.Raw);
            return (ll, null);
        }

        private static (Addr, error) parseDefaultAddr(slice<byte> b)
        {
            if (len(b) < 2L || len(b) < int(b[0L]))
            {
                return (null, errInvalidAddr);
            }
            DefaultAddr a = ref new DefaultAddr(af:int(b[1]),Raw:b[:b[0]]);
            return (a, null);
        }

        private static long addrsSpace(slice<Addr> @as)
        {
            long l = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in as)
                {
                    a = __a;
                    switch (a.type())
                    {
                        case ref LinkAddr a:
                            var (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                        case ref Inet4Addr a:
                            (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                        case ref Inet6Addr a:
                            (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                        case ref DefaultAddr a:
                            (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                    }
                }

                a = a__prev1;
            }

            return l;
        }

        // marshalAddrs marshals as and returns a bitmap indicating which
        // address is stored in b.
        private static (ulong, error) marshalAddrs(slice<byte> b, slice<Addr> @as)
        {
            ulong attrs = default;
            {
                var a__prev1 = a;

                foreach (var (__i, __a) in as)
                {
                    i = __i;
                    a = __a;
                    switch (a.type())
                    {
                        case ref LinkAddr a:
                            var (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, err);
                            }
                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                        case ref Inet4Addr a:
                            (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, err);
                            }
                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                        case ref Inet6Addr a:
                            (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, err);
                            }
                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                        case ref DefaultAddr a:
                            (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, err);
                            }
                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                    }
                }

                a = a__prev1;
            }

            return (attrs, null);
        }

        private static (slice<Addr>, error) parseAddrs(ulong attrs, Func<long, slice<byte>, (long, Addr, error)> fn, slice<byte> b)
        {
            array<Addr> @as = new array<Addr>(sysRTAX_MAX);
            var af = int(sysAF_UNSPEC);
            for (var i = uint(0L); i < sysRTAX_MAX && len(b) >= roundup(0L); i++)
            {
                if (attrs & (1L << (int)(i)) == 0L)
                {
                    continue;
                }
                if (i <= sysRTAX_BRD)
                {

                    if (b[1L] == sysAF_LINK) 
                        var (a, err) = parseLinkAddr(b);
                        if (err != null)
                        {
                            return (null, err);
                        }
                        as[i] = a;
                        var l = roundup(int(b[0L]));
                        if (len(b) < l)
                        {
                            return (null, errMessageTooShort);
                        }
                        b = b[l..];
                    else if (b[1L] == sysAF_INET || b[1L] == sysAF_INET6) 
                        af = int(b[1L]);
                        (a, err) = parseInetAddr(af, b);
                        if (err != null)
                        {
                            return (null, err);
                        }
                        as[i] = a;
                        l = roundup(int(b[0L]));
                        if (len(b) < l)
                        {
                            return (null, errMessageTooShort);
                        }
                        b = b[l..];
                    else 
                        var (l, a, err) = fn(af, b);
                        if (err != null)
                        {
                            return (null, err);
                        }
                        as[i] = a;
                        var ll = roundup(l);
                        if (len(b) < ll)
                        {
                            b = b[l..];
                        }
                        else
                        {
                            b = b[ll..];
                        }
                                    }
                else
                {
                    (a, err) = parseDefaultAddr(b);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    as[i] = a;
                    l = roundup(int(b[0L]));
                    if (len(b) < l)
                    {
                        return (null, errMessageTooShort);
                    }
                    b = b[l..];
                }
            }

            return (as[..], null);
        }
    }
}}}}}
