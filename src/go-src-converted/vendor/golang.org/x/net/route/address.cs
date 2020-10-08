// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 October 08 05:01:35 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Go\src\vendor\golang.org\x\net\route\address.go
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
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
        private static long Family(this ptr<LinkAddr> _addr_a)
        {
            ref LinkAddr a = ref _addr_a.val;

            return sysAF_LINK;
        }

        private static (long, long) lenAndSpace(this ptr<LinkAddr> _addr_a)
        {
            long _p0 = default;
            long _p0 = default;
            ref LinkAddr a = ref _addr_a.val;

            long l = 8L + len(a.Name) + len(a.Addr);
            return (l, roundup(l));
        }

        private static (long, error) marshal(this ptr<LinkAddr> _addr_a, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref LinkAddr a = ref _addr_a.val;

            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, error.As(errShortBuffer)!);
            }

            var nlen = len(a.Name);
            var alen = len(a.Addr);
            if (nlen > 255L || alen > 255L)
            {
                return (0L, error.As(errInvalidAddr)!);
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
                copy(data[..nlen], a.Name);
                data = data[nlen..];
            }

            if (alen > 0L)
            {
                b[6L] = byte(alen);
                copy(data[..alen], a.Addr);
                data = data[alen..];
            }

            return (ll, error.As(null!)!);

        }

        private static (Addr, error) parseLinkAddr(slice<byte> b)
        {
            Addr _p0 = default;
            error _p0 = default!;

            if (len(b) < 8L)
            {
                return (null, error.As(errInvalidAddr)!);
            }

            var (_, a, err) = parseKernelLinkAddr(sysAF_LINK, b[4L..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            a._<ptr<LinkAddr>>().Index = int(nativeEndian.Uint16(b[2L..4L]));
            return (a, error.As(null!)!);

        }

        // parseKernelLinkAddr parses b as a link-layer address in
        // conventional BSD kernel form.
        private static (long, Addr, error) parseKernelLinkAddr(long _, slice<byte> b)
        {
            long _p0 = default;
            Addr _p0 = default;
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
                return (0L, null, error.As(errInvalidAddr)!);
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

            return (l, addr(new LinkAddr(Name:name,Addr:addr)), error.As(null!)!);

        }

        // An Inet4Addr represents an internet address for IPv4.
        public partial struct Inet4Addr
        {
            public array<byte> IP; // IP address
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ptr<Inet4Addr> _addr_a)
        {
            ref Inet4Addr a = ref _addr_a.val;

            return sysAF_INET;
        }

        private static (long, long) lenAndSpace(this ptr<Inet4Addr> _addr_a)
        {
            long _p0 = default;
            long _p0 = default;
            ref Inet4Addr a = ref _addr_a.val;

            return (sizeofSockaddrInet, roundup(sizeofSockaddrInet));
        }

        private static (long, error) marshal(this ptr<Inet4Addr> _addr_a, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Inet4Addr a = ref _addr_a.val;

            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, error.As(errShortBuffer)!);
            }

            b[0L] = byte(l);
            b[1L] = sysAF_INET;
            copy(b[4L..8L], a.IP[..]);
            return (ll, error.As(null!)!);

        }

        // An Inet6Addr represents an internet address for IPv6.
        public partial struct Inet6Addr
        {
            public array<byte> IP; // IP address
            public long ZoneID; // zone identifier
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ptr<Inet6Addr> _addr_a)
        {
            ref Inet6Addr a = ref _addr_a.val;

            return sysAF_INET6;
        }

        private static (long, long) lenAndSpace(this ptr<Inet6Addr> _addr_a)
        {
            long _p0 = default;
            long _p0 = default;
            ref Inet6Addr a = ref _addr_a.val;

            return (sizeofSockaddrInet6, roundup(sizeofSockaddrInet6));
        }

        private static (long, error) marshal(this ptr<Inet6Addr> _addr_a, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Inet6Addr a = ref _addr_a.val;

            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, error.As(errShortBuffer)!);
            }

            b[0L] = byte(l);
            b[1L] = sysAF_INET6;
            copy(b[8L..24L], a.IP[..]);
            if (a.ZoneID > 0L)
            {
                nativeEndian.PutUint32(b[24L..28L], uint32(a.ZoneID));
            }

            return (ll, error.As(null!)!);

        }

        // parseInetAddr parses b as an internet address for IPv4 or IPv6.
        private static (Addr, error) parseInetAddr(long af, slice<byte> b)
        {
            Addr _p0 = default;
            error _p0 = default!;


            if (af == sysAF_INET) 
                if (len(b) < sizeofSockaddrInet)
                {
                    return (null, error.As(errInvalidAddr)!);
                }

                ptr<Inet4Addr> a = addr(new Inet4Addr());
                copy(a.IP[..], b[4L..8L]);
                return (a, error.As(null!)!);
            else if (af == sysAF_INET6) 
                if (len(b) < sizeofSockaddrInet6)
                {
                    return (null, error.As(errInvalidAddr)!);
                }

                a = addr(new Inet6Addr(ZoneID:int(nativeEndian.Uint32(b[24:28]))));
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

                return (a, error.As(null!)!);
            else 
                return (null, error.As(errInvalidAddr)!);
            
        }

        // parseKernelInetAddr parses b as an internet address in conventional
        // BSD kernel form.
        private static (long, Addr, error) parseKernelInetAddr(long af, slice<byte> b)
        {
            long _p0 = default;
            Addr _p0 = default;
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
            var l = int(b[0L]);
            if (runtime.GOOS == "darwin")
            { 
                // On Darwin, an address in the kernel form is also
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
                return (0L, null, error.As(errInvalidAddr)!);
            } 
            // Don't reorder case expressions.
            // The case expressions for IPv6 must come first.
            const long off4 = (long)4L; // offset of in_addr
            const long off6 = (long)8L; // offset of in6_addr

            if (b[0L] == sizeofSockaddrInet6) 
                ptr<Inet6Addr> a = addr(new Inet6Addr());
                copy(a.IP[..], b[off6..off6 + 16L]);
                return (int(b[0L]), a, error.As(null!)!);
            else if (af == sysAF_INET6) 
                a = addr(new Inet6Addr());
                if (l - 1L < off6)
                {
                    copy(a.IP[..], b[1L..l]);
                }
                else
                {
                    copy(a.IP[..], b[l - off6..l]);
                }

                return (int(b[0L]), a, error.As(null!)!);
            else if (b[0L] == sizeofSockaddrInet) 
                a = addr(new Inet4Addr());
                copy(a.IP[..], b[off4..off4 + 4L]);
                return (int(b[0L]), a, error.As(null!)!);
            else // an old fashion, AF_UNSPEC or unknown means AF_INET
                a = addr(new Inet4Addr());
                if (l - 1L < off4)
                {
                    copy(a.IP[..], b[1L..l]);
                }
                else
                {
                    copy(a.IP[..], b[l - off4..l]);
                }

                return (int(b[0L]), a, error.As(null!)!);
            
        }

        // A DefaultAddr represents an address of various operating
        // system-specific features.
        public partial struct DefaultAddr
        {
            public long af;
            public slice<byte> Raw; // raw format of address
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ptr<DefaultAddr> _addr_a)
        {
            ref DefaultAddr a = ref _addr_a.val;

            return a.af;
        }

        private static (long, long) lenAndSpace(this ptr<DefaultAddr> _addr_a)
        {
            long _p0 = default;
            long _p0 = default;
            ref DefaultAddr a = ref _addr_a.val;

            var l = len(a.Raw);
            return (l, roundup(l));
        }

        private static (long, error) marshal(this ptr<DefaultAddr> _addr_a, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref DefaultAddr a = ref _addr_a.val;

            var (l, ll) = a.lenAndSpace();
            if (len(b) < ll)
            {
                return (0L, error.As(errShortBuffer)!);
            }

            if (l > 255L)
            {
                return (0L, error.As(errInvalidAddr)!);
            }

            b[1L] = byte(l);
            copy(b[..l], a.Raw);
            return (ll, error.As(null!)!);

        }

        private static (Addr, error) parseDefaultAddr(slice<byte> b)
        {
            Addr _p0 = default;
            error _p0 = default!;

            if (len(b) < 2L || len(b) < int(b[0L]))
            {
                return (null, error.As(errInvalidAddr)!);
            }

            ptr<DefaultAddr> a = addr(new DefaultAddr(af:int(b[1]),Raw:b[:b[0]]));
            return (a, error.As(null!)!);

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
                        case ptr<LinkAddr> a:
                            var (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                        case ptr<Inet4Addr> a:
                            (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                        case ptr<Inet6Addr> a:
                            (_, ll) = a.lenAndSpace();
                            l += ll;
                            break;
                        case ptr<DefaultAddr> a:
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
            ulong _p0 = default;
            error _p0 = default!;

            ulong attrs = default;
            {
                var a__prev1 = a;

                foreach (var (__i, __a) in as)
                {
                    i = __i;
                    a = __a;
                    switch (a.type())
                    {
                        case ptr<LinkAddr> a:
                            var (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, error.As(err)!);
                            }

                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                        case ptr<Inet4Addr> a:
                            (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, error.As(err)!);
                            }

                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                        case ptr<Inet6Addr> a:
                            (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, error.As(err)!);
                            }

                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                        case ptr<DefaultAddr> a:
                            (l, err) = a.marshal(b);
                            if (err != null)
                            {
                                return (0L, error.As(err)!);
                            }

                            b = b[l..];
                            attrs |= 1L << (int)(uint(i));
                            break;
                    }

                }

                a = a__prev1;
            }

            return (attrs, error.As(null!)!);

        }

        private static (slice<Addr>, error) parseAddrs(ulong attrs, Func<long, slice<byte>, (long, Addr, error)> fn, slice<byte> b)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;

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
                            return (null, error.As(err)!);
                        }

                        as[i] = a;
                        var l = roundup(int(b[0L]));
                        if (len(b) < l)
                        {
                            return (null, error.As(errMessageTooShort)!);
                        }

                        b = b[l..];
                    else if (b[1L] == sysAF_INET || b[1L] == sysAF_INET6) 
                        af = int(b[1L]);
                        (a, err) = parseInetAddr(af, b);
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        as[i] = a;
                        l = roundup(int(b[0L]));
                        if (len(b) < l)
                        {
                            return (null, error.As(errMessageTooShort)!);
                        }

                        b = b[l..];
                    else 
                        var (l, a, err) = fn(af, b);
                        if (err != null)
                        {
                            return (null, error.As(err)!);
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
                        return (null, error.As(err)!);
                    }

                    as[i] = a;
                    l = roundup(int(b[0L]));
                    if (len(b) < l)
                    {
                        return (null, error.As(errMessageTooShort)!);
                    }

                    b = b[l..];

                }

            }

            return (as[..], error.As(null!)!);

        }
    }
}}}}}
