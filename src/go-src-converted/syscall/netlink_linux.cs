// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Netlink sockets and messages

// package syscall -- go2cs converted at 2022 March 06 22:26:39 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\netlink_linux.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    // Round the length of a netlink message up to align it properly.
private static nint nlmAlignOf(nint msglen) {
    return (msglen + NLMSG_ALIGNTO - 1) & ~(NLMSG_ALIGNTO - 1);
}

// Round the length of a netlink route attribute up to align it
// properly.
private static nint rtaAlignOf(nint attrlen) {
    return (attrlen + RTA_ALIGNTO - 1) & ~(RTA_ALIGNTO - 1);
}

// NetlinkRouteRequest represents a request message to receive routing
// and link states from the kernel.
public partial struct NetlinkRouteRequest {
    public NlMsghdr Header;
    public RtGenmsg Data;
}

private static slice<byte> toWireFormat(this ptr<NetlinkRouteRequest> _addr_rr) {
    ref NetlinkRouteRequest rr = ref _addr_rr.val;

    var b = make_slice<byte>(rr.Header.Len) * (uint32.val)(@unsafe.Pointer(_addr_b[(int)0..(int)4][0]));

    rr.Header.Len * (uint16.val)(@unsafe.Pointer(_addr_b[(int)4..(int)6][0]));

    rr.Header.Type * (uint16.val)(@unsafe.Pointer(_addr_b[(int)6..(int)8][0]));

    rr.Header.Flags * (uint32.val)(@unsafe.Pointer(_addr_b[(int)8..(int)12][0]));

    rr.Header.Seq * (uint32.val)(@unsafe.Pointer(_addr_b[(int)12..(int)16][0]));

    rr.Header.Pid;
    b[16] = byte(rr.Data.Family);
    return b;

}

private static slice<byte> newNetlinkRouteRequest(nint proto, nint seq, nint family) {
    ptr<NetlinkRouteRequest> rr = addr(new NetlinkRouteRequest());
    rr.Header.Len = uint32(NLMSG_HDRLEN + SizeofRtGenmsg);
    rr.Header.Type = uint16(proto);
    rr.Header.Flags = NLM_F_DUMP | NLM_F_REQUEST;
    rr.Header.Seq = uint32(seq);
    rr.Data.Family = uint8(family);
    return rr.toWireFormat();
}

// NetlinkRIB returns routing information base, as known as RIB, which
// consists of network facility information, states and parameters.
public static (slice<byte>, error) NetlinkRIB(nint proto, nint family) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (s, err) = cloexecSocket(AF_NETLINK, SOCK_RAW, NETLINK_ROUTE);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(Close(s));
    ptr<SockaddrNetlink> lsa = addr(new SockaddrNetlink(Family:AF_NETLINK));
    {
        var err__prev1 = err;

        var err = Bind(s, lsa);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    var wb = newNetlinkRouteRequest(proto, 1, family);
    {
        var err__prev1 = err;

        err = Sendto(s, wb, 0, lsa);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    slice<byte> tab = default;
    var rbNew = make_slice<byte>(Getpagesize());
done:
    while (true) {
        var rb = rbNew;
        var (nr, _, err) = Recvfrom(s, rb, 0);
        if (err != null) {
            return (null, error.As(err)!);
        }
        if (nr < NLMSG_HDRLEN) {
            return (null, error.As(EINVAL)!);
        }
        rb = rb[..(int)nr];
        tab = append(tab, rb);
        var (msgs, err) = ParseNetlinkMessage(rb);
        if (err != null) {
            return (null, error.As(err)!);
        }
        foreach (var (_, m) in msgs) {
            var (lsa, err) = Getsockname(s);
            if (err != null) {
                return (null, error.As(err)!);
            }
            switch (lsa.type()) {
                case ptr<SockaddrNetlink> v:
                    if (m.Header.Seq != 1 || m.Header.Pid != v.Pid) {
                        return (null, error.As(EINVAL)!);
                    }
                    break;
                default:
                {
                    var v = lsa.type();
                    return (null, error.As(EINVAL)!);
                    break;
                }
            }
            if (m.Header.Type == NLMSG_DONE) {
                _breakdone = true;
                break;
            }

            if (m.Header.Type == NLMSG_ERROR) {
                return (null, error.As(EINVAL)!);
            }

        }
    }
    return (tab, error.As(null!)!);

});

// NetlinkMessage represents a netlink message.
public partial struct NetlinkMessage {
    public NlMsghdr Header;
    public slice<byte> Data;
}

// ParseNetlinkMessage parses b as an array of netlink messages and
// returns the slice containing the NetlinkMessage structures.
public static (slice<NetlinkMessage>, error) ParseNetlinkMessage(slice<byte> b) {
    slice<NetlinkMessage> _p0 = default;
    error _p0 = default!;

    slice<NetlinkMessage> msgs = default;
    while (len(b) >= NLMSG_HDRLEN) {
        var (h, dbuf, dlen, err) = netlinkMessageHeaderAndData(b);
        if (err != null) {
            return (null, error.As(err)!);
        }
        NetlinkMessage m = new NetlinkMessage(Header:*h,Data:dbuf[:int(h.Len)-NLMSG_HDRLEN]);
        msgs = append(msgs, m);
        b = b[(int)dlen..];

    }
    return (msgs, error.As(null!)!);

}

private static (ptr<NlMsghdr>, slice<byte>, nint, error) netlinkMessageHeaderAndData(slice<byte> b) {
    ptr<NlMsghdr> _p0 = default!;
    slice<byte> _p0 = default;
    nint _p0 = default;
    error _p0 = default!;

    var h = (NlMsghdr.val)(@unsafe.Pointer(_addr_b[0]));
    var l = nlmAlignOf(int(h.Len));
    if (int(h.Len) < NLMSG_HDRLEN || l > len(b)) {
        return (_addr_null!, null, 0, error.As(EINVAL)!);
    }
    return (_addr_h!, b[(int)NLMSG_HDRLEN..], l, error.As(null!)!);

}

// NetlinkRouteAttr represents a netlink route attribute.
public partial struct NetlinkRouteAttr {
    public RtAttr Attr;
    public slice<byte> Value;
}

// ParseNetlinkRouteAttr parses m's payload as an array of netlink
// route attributes and returns the slice containing the
// NetlinkRouteAttr structures.
public static (slice<NetlinkRouteAttr>, error) ParseNetlinkRouteAttr(ptr<NetlinkMessage> _addr_m) {
    slice<NetlinkRouteAttr> _p0 = default;
    error _p0 = default!;
    ref NetlinkMessage m = ref _addr_m.val;

    slice<byte> b = default;

    if (m.Header.Type == RTM_NEWLINK || m.Header.Type == RTM_DELLINK) 
        b = m.Data[(int)SizeofIfInfomsg..];
    else if (m.Header.Type == RTM_NEWADDR || m.Header.Type == RTM_DELADDR) 
        b = m.Data[(int)SizeofIfAddrmsg..];
    else if (m.Header.Type == RTM_NEWROUTE || m.Header.Type == RTM_DELROUTE) 
        b = m.Data[(int)SizeofRtMsg..];
    else 
        return (null, error.As(EINVAL)!);
        slice<NetlinkRouteAttr> attrs = default;
    while (len(b) >= SizeofRtAttr) {
        var (a, vbuf, alen, err) = netlinkRouteAttrAndValue(b);
        if (err != null) {
            return (null, error.As(err)!);
        }
        NetlinkRouteAttr ra = new NetlinkRouteAttr(Attr:*a,Value:vbuf[:int(a.Len)-SizeofRtAttr]);
        attrs = append(attrs, ra);
        b = b[(int)alen..];

    }
    return (attrs, error.As(null!)!);

}

private static (ptr<RtAttr>, slice<byte>, nint, error) netlinkRouteAttrAndValue(slice<byte> b) {
    ptr<RtAttr> _p0 = default!;
    slice<byte> _p0 = default;
    nint _p0 = default;
    error _p0 = default!;

    var a = (RtAttr.val)(@unsafe.Pointer(_addr_b[0]));
    if (int(a.Len) < SizeofRtAttr || int(a.Len) > len(b)) {
        return (_addr_null!, null, 0, error.As(EINVAL)!);
    }
    return (_addr_a!, b[(int)SizeofRtAttr..], rtaAlignOf(int(a.Len)), error.As(null!)!);

}

} // end syscall_package
