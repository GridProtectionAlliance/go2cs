// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2022 March 13 06:46:29 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\route\address.go
namespace go.vendor.golang.org.x.net;

using runtime = runtime_package;
using System;

public static partial class route_package {

// An Addr represents an address associated with packet routing.
public partial interface Addr {
    nint Family();
}

// A LinkAddr represents a link-layer address.
public partial struct LinkAddr {
    public nint Index; // interface index when attached
    public @string Name; // interface name when attached
    public slice<byte> Addr; // link-layer address when attached
}

// Family implements the Family method of Addr interface.
private static nint Family(this ptr<LinkAddr> _addr_a) {
    ref LinkAddr a = ref _addr_a.val;

    return sysAF_LINK;
}

private static (nint, nint) lenAndSpace(this ptr<LinkAddr> _addr_a) {
    nint _p0 = default;
    nint _p0 = default;
    ref LinkAddr a = ref _addr_a.val;

    nint l = 8 + len(a.Name) + len(a.Addr);
    return (l, roundup(l));
}

private static (nint, error) marshal(this ptr<LinkAddr> _addr_a, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref LinkAddr a = ref _addr_a.val;

    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, error.As(errShortBuffer)!);
    }
    var nlen = len(a.Name);
    var alen = len(a.Addr);
    if (nlen > 255 || alen > 255) {
        return (0, error.As(errInvalidAddr)!);
    }
    b[0] = byte(l);
    b[1] = sysAF_LINK;
    if (a.Index > 0) {
        nativeEndian.PutUint16(b[(int)2..(int)4], uint16(a.Index));
    }
    var data = b[(int)8..];
    if (nlen > 0) {
        b[5] = byte(nlen);
        copy(data[..(int)nlen], a.Name);
        data = data[(int)nlen..];
    }
    if (alen > 0) {
        b[6] = byte(alen);
        copy(data[..(int)alen], a.Addr);
        data = data[(int)alen..];
    }
    return (ll, error.As(null!)!);
}

private static (Addr, error) parseLinkAddr(slice<byte> b) {
    Addr _p0 = default;
    error _p0 = default!;

    if (len(b) < 8) {
        return (null, error.As(errInvalidAddr)!);
    }
    var (_, a, err) = parseKernelLinkAddr(sysAF_LINK, b[(int)4..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    a._<ptr<LinkAddr>>().Index = int(nativeEndian.Uint16(b[(int)2..(int)4]));
    return (a, error.As(null!)!);
}

// parseKernelLinkAddr parses b as a link-layer address in
// conventional BSD kernel form.
private static (nint, Addr, error) parseKernelLinkAddr(nint _, slice<byte> b) {
    nint _p0 = default;
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
    var nlen = int(b[1]);
    var alen = int(b[2]);
    var slen = int(b[3]);
    if (nlen == 0xff) {
        nlen = 0;
    }
    if (alen == 0xff) {
        alen = 0;
    }
    if (slen == 0xff) {
        slen = 0;
    }
    nint l = 4 + nlen + alen + slen;
    if (len(b) < l) {
        return (0, null, error.As(errInvalidAddr)!);
    }
    var data = b[(int)4..];
    @string name = default;
    slice<byte> addr = default;
    if (nlen > 0) {
        name = string(data[..(int)nlen]);
        data = data[(int)nlen..];
    }
    if (alen > 0) {
        addr = data[..(int)alen];
        data = data[(int)alen..];
    }
    return (l, addr(new LinkAddr(Name:name,Addr:addr)), error.As(null!)!);
}

// An Inet4Addr represents an internet address for IPv4.
public partial struct Inet4Addr {
    public array<byte> IP; // IP address
}

// Family implements the Family method of Addr interface.
private static nint Family(this ptr<Inet4Addr> _addr_a) {
    ref Inet4Addr a = ref _addr_a.val;

    return sysAF_INET;
}

private static (nint, nint) lenAndSpace(this ptr<Inet4Addr> _addr_a) {
    nint _p0 = default;
    nint _p0 = default;
    ref Inet4Addr a = ref _addr_a.val;

    return (sizeofSockaddrInet, roundup(sizeofSockaddrInet));
}

private static (nint, error) marshal(this ptr<Inet4Addr> _addr_a, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Inet4Addr a = ref _addr_a.val;

    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, error.As(errShortBuffer)!);
    }
    b[0] = byte(l);
    b[1] = sysAF_INET;
    copy(b[(int)4..(int)8], a.IP[..]);
    return (ll, error.As(null!)!);
}

// An Inet6Addr represents an internet address for IPv6.
public partial struct Inet6Addr {
    public array<byte> IP; // IP address
    public nint ZoneID; // zone identifier
}

// Family implements the Family method of Addr interface.
private static nint Family(this ptr<Inet6Addr> _addr_a) {
    ref Inet6Addr a = ref _addr_a.val;

    return sysAF_INET6;
}

private static (nint, nint) lenAndSpace(this ptr<Inet6Addr> _addr_a) {
    nint _p0 = default;
    nint _p0 = default;
    ref Inet6Addr a = ref _addr_a.val;

    return (sizeofSockaddrInet6, roundup(sizeofSockaddrInet6));
}

private static (nint, error) marshal(this ptr<Inet6Addr> _addr_a, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Inet6Addr a = ref _addr_a.val;

    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, error.As(errShortBuffer)!);
    }
    b[0] = byte(l);
    b[1] = sysAF_INET6;
    copy(b[(int)8..(int)24], a.IP[..]);
    if (a.ZoneID > 0) {
        nativeEndian.PutUint32(b[(int)24..(int)28], uint32(a.ZoneID));
    }
    return (ll, error.As(null!)!);
}

// parseInetAddr parses b as an internet address for IPv4 or IPv6.
private static (Addr, error) parseInetAddr(nint af, slice<byte> b) {
    Addr _p0 = default;
    error _p0 = default!;


    if (af == sysAF_INET) 
        if (len(b) < sizeofSockaddrInet) {
            return (null, error.As(errInvalidAddr)!);
        }
        ptr<Inet4Addr> a = addr(new Inet4Addr());
        copy(a.IP[..], b[(int)4..(int)8]);
        return (a, error.As(null!)!);
    else if (af == sysAF_INET6) 
        if (len(b) < sizeofSockaddrInet6) {
            return (null, error.As(errInvalidAddr)!);
        }
        a = addr(new Inet6Addr(ZoneID:int(nativeEndian.Uint32(b[24:28]))));
        copy(a.IP[..], b[(int)8..(int)24]);
        if (a.IP[0] == 0xfe && a.IP[1] & 0xc0 == 0x80 || a.IP[0] == 0xff && (a.IP[1] & 0x0f == 0x01 || a.IP[1] & 0x0f == 0x02)) { 
            // KAME based IPv6 protocol stack usually
            // embeds the interface index in the
            // interface-local or link-local address as
            // the kernel-internal form.
            var id = int(bigEndian.Uint16(a.IP[(int)2..(int)4]));
            if (id != 0) {
                a.ZoneID = id;
                (a.IP[2], a.IP[3]) = (0, 0);
            }
        }
        return (a, error.As(null!)!);
    else 
        return (null, error.As(errInvalidAddr)!);
    }

// parseKernelInetAddr parses b as an internet address in conventional
// BSD kernel form.
private static (nint, Addr, error) parseKernelInetAddr(nint af, slice<byte> b) {
    nint _p0 = default;
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
    var l = int(b[0]);
    if (runtime.GOOS == "darwin" || runtime.GOOS == "ios") { 
        // On Darwin, an address in the kernel form is also
        // used as a message filler.
        if (l == 0 || len(b) > roundup(l)) {
            l = roundup(l);
        }
    }
    else
 {
        l = roundup(l);
    }
    if (len(b) < l) {
        return (0, null, error.As(errInvalidAddr)!);
    }
    const nint off4 = 4; // offset of in_addr
    const nint off6 = 8; // offset of in6_addr

    if (b[0] == sizeofSockaddrInet6) 
        ptr<Inet6Addr> a = addr(new Inet6Addr());
        copy(a.IP[..], b[(int)off6..(int)off6 + 16]);
        return (int(b[0]), a, error.As(null!)!);
    else if (af == sysAF_INET6) 
        a = addr(new Inet6Addr());
        if (l - 1 < off6) {
            copy(a.IP[..], b[(int)1..(int)l]);
        }
        else
 {
            copy(a.IP[..], b[(int)l - off6..(int)l]);
        }
        return (int(b[0]), a, error.As(null!)!);
    else if (b[0] == sizeofSockaddrInet) 
        a = addr(new Inet4Addr());
        copy(a.IP[..], b[(int)off4..(int)off4 + 4]);
        return (int(b[0]), a, error.As(null!)!);
    else // an old fashion, AF_UNSPEC or unknown means AF_INET
        a = addr(new Inet4Addr());
        if (l - 1 < off4) {
            copy(a.IP[..], b[(int)1..(int)l]);
        }
        else
 {
            copy(a.IP[..], b[(int)l - off4..(int)l]);
        }
        return (int(b[0]), a, error.As(null!)!);
    }

// A DefaultAddr represents an address of various operating
// system-specific features.
public partial struct DefaultAddr {
    public nint af;
    public slice<byte> Raw; // raw format of address
}

// Family implements the Family method of Addr interface.
private static nint Family(this ptr<DefaultAddr> _addr_a) {
    ref DefaultAddr a = ref _addr_a.val;

    return a.af;
}

private static (nint, nint) lenAndSpace(this ptr<DefaultAddr> _addr_a) {
    nint _p0 = default;
    nint _p0 = default;
    ref DefaultAddr a = ref _addr_a.val;

    var l = len(a.Raw);
    return (l, roundup(l));
}

private static (nint, error) marshal(this ptr<DefaultAddr> _addr_a, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref DefaultAddr a = ref _addr_a.val;

    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, error.As(errShortBuffer)!);
    }
    if (l > 255) {
        return (0, error.As(errInvalidAddr)!);
    }
    b[1] = byte(l);
    copy(b[..(int)l], a.Raw);
    return (ll, error.As(null!)!);
}

private static (Addr, error) parseDefaultAddr(slice<byte> b) {
    Addr _p0 = default;
    error _p0 = default!;

    if (len(b) < 2 || len(b) < int(b[0])) {
        return (null, error.As(errInvalidAddr)!);
    }
    ptr<DefaultAddr> a = addr(new DefaultAddr(af:int(b[1]),Raw:b[:b[0]]));
    return (a, error.As(null!)!);
}

private static nint addrsSpace(slice<Addr> @as) {
    nint l = default;
    {
        var a__prev1 = a;

        foreach (var (_, __a) in as) {
            a = __a;
            switch (a.type()) {
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
private static (nuint, error) marshalAddrs(slice<byte> b, slice<Addr> @as) {
    nuint _p0 = default;
    error _p0 = default!;

    nuint attrs = default;
    {
        var a__prev1 = a;

        foreach (var (__i, __a) in as) {
            i = __i;
            a = __a;
            switch (a.type()) {
                case ptr<LinkAddr> a:
                    var (l, err) = a.marshal(b);
                    if (err != null) {
                        return (0, error.As(err)!);
                    }
                    b = b[(int)l..];
                    attrs |= 1 << (int)(uint(i));
                    break;
                case ptr<Inet4Addr> a:
                    (l, err) = a.marshal(b);
                    if (err != null) {
                        return (0, error.As(err)!);
                    }
                    b = b[(int)l..];
                    attrs |= 1 << (int)(uint(i));
                    break;
                case ptr<Inet6Addr> a:
                    (l, err) = a.marshal(b);
                    if (err != null) {
                        return (0, error.As(err)!);
                    }
                    b = b[(int)l..];
                    attrs |= 1 << (int)(uint(i));
                    break;
                case ptr<DefaultAddr> a:
                    (l, err) = a.marshal(b);
                    if (err != null) {
                        return (0, error.As(err)!);
                    }
                    b = b[(int)l..];
                    attrs |= 1 << (int)(uint(i));
                    break;
            }
        }
        a = a__prev1;
    }

    return (attrs, error.As(null!)!);
}

private static (slice<Addr>, error) parseAddrs(nuint attrs, Func<nint, slice<byte>, (nint, Addr, error)> fn, slice<byte> b) {
    slice<Addr> _p0 = default;
    error _p0 = default!;

    array<Addr> @as = new array<Addr>(sysRTAX_MAX);
    var af = int(sysAF_UNSPEC);
    for (var i = uint(0); i < sysRTAX_MAX && len(b) >= roundup(0); i++) {
        if (attrs & (1 << (int)(i)) == 0) {
            continue;
        }
        if (i <= sysRTAX_BRD) {

            if (b[1] == sysAF_LINK) 
                var (a, err) = parseLinkAddr(b);
                if (err != null) {
                    return (null, error.As(err)!);
                }
                as[i] = a;
                var l = roundup(int(b[0]));
                if (len(b) < l) {
                    return (null, error.As(errMessageTooShort)!);
                }
                b = b[(int)l..];
            else if (b[1] == sysAF_INET || b[1] == sysAF_INET6) 
                af = int(b[1]);
                (a, err) = parseInetAddr(af, b);
                if (err != null) {
                    return (null, error.As(err)!);
                }
                as[i] = a;
                l = roundup(int(b[0]));
                if (len(b) < l) {
                    return (null, error.As(errMessageTooShort)!);
                }
                b = b[(int)l..];
            else 
                var (l, a, err) = fn(af, b);
                if (err != null) {
                    return (null, error.As(err)!);
                }
                as[i] = a;
                var ll = roundup(l);
                if (len(b) < ll) {
                    b = b[(int)l..];
                }
                else
 {
                    b = b[(int)ll..];
                }
                    }
        else
 {
            (a, err) = parseDefaultAddr(b);
            if (err != null) {
                return (null, error.As(err)!);
            }
            as[i] = a;
            l = roundup(int(b[0]));
            if (len(b) < l) {
                return (null, error.As(errMessageTooShort)!);
            }
            b = b[(int)l..];
        }
    }
    return (as[..], error.As(null!)!);
}

} // end route_package
