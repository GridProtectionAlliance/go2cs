// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Netlink sockets and messages

// package syscall -- go2cs converted at 2020 August 29 08:37:19 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\netlink_linux.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Round the length of a netlink message up to align it properly.
        private static long nlmAlignOf(long msglen)
        {
            return (msglen + NLMSG_ALIGNTO - 1L) & ~(NLMSG_ALIGNTO - 1L);
        }

        // Round the length of a netlink route attribute up to align it
        // properly.
        private static long rtaAlignOf(long attrlen)
        {
            return (attrlen + RTA_ALIGNTO - 1L) & ~(RTA_ALIGNTO - 1L);
        }

        // NetlinkRouteRequest represents a request message to receive routing
        // and link states from the kernel.
        public partial struct NetlinkRouteRequest
        {
            public NlMsghdr Header;
            public RtGenmsg Data;
        }

        private static slice<byte> toWireFormat(this ref NetlinkRouteRequest rr)
        {
            var b = make_slice<byte>(rr.Header.Len) * (uint32.Value)(@unsafe.Pointer(ref b[0L..4L][0L]));

            rr.Header.Len * (uint16.Value)(@unsafe.Pointer(ref b[4L..6L][0L]));

            rr.Header.Type * (uint16.Value)(@unsafe.Pointer(ref b[6L..8L][0L]));

            rr.Header.Flags * (uint32.Value)(@unsafe.Pointer(ref b[8L..12L][0L]));

            rr.Header.Seq * (uint32.Value)(@unsafe.Pointer(ref b[12L..16L][0L]));

            rr.Header.Pid;
            b[16L] = byte(rr.Data.Family);
            return b;
        }

        private static slice<byte> newNetlinkRouteRequest(long proto, long seq, long family)
        {
            NetlinkRouteRequest rr = ref new NetlinkRouteRequest();
            rr.Header.Len = uint32(NLMSG_HDRLEN + SizeofRtGenmsg);
            rr.Header.Type = uint16(proto);
            rr.Header.Flags = NLM_F_DUMP | NLM_F_REQUEST;
            rr.Header.Seq = uint32(seq);
            rr.Data.Family = uint8(family);
            return rr.toWireFormat();
        }

        // NetlinkRIB returns routing information base, as known as RIB, which
        // consists of network facility information, states and parameters.
        public static (slice<byte>, error) NetlinkRIB(long proto, long family) => func((defer, _, __) =>
        {
            var (s, err) = Socket(AF_NETLINK, SOCK_RAW, NETLINK_ROUTE);
            if (err != null)
            {
                return (null, err);
            }
            defer(Close(s));
            SockaddrNetlink lsa = ref new SockaddrNetlink(Family:AF_NETLINK);
            {
                var err__prev1 = err;

                var err = Bind(s, lsa);

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }
            var wb = newNetlinkRouteRequest(proto, 1L, family);
            {
                var err__prev1 = err;

                err = Sendto(s, wb, 0L, lsa);

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }
            slice<byte> tab = default;
            var rbNew = make_slice<byte>(Getpagesize());
done:
            while (true)
            {
                var rb = rbNew;
                var (nr, _, err) = Recvfrom(s, rb, 0L);
                if (err != null)
                {
                    return (null, err);
                }
                if (nr < NLMSG_HDRLEN)
                {
                    return (null, EINVAL);
                }
                rb = rb[..nr];
                tab = append(tab, rb);
                var (msgs, err) = ParseNetlinkMessage(rb);
                if (err != null)
                {
                    return (null, err);
                }
                foreach (var (_, m) in msgs)
                {
                    var (lsa, err) = Getsockname(s);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    switch (lsa.type())
                    {
                        case ref SockaddrNetlink v:
                            if (m.Header.Seq != 1L || m.Header.Pid != v.Pid)
                            {
                                return (null, EINVAL);
                            }
                            break;
                        default:
                        {
                            var v = lsa.type();
                            return (null, EINVAL);
                            break;
                        }
                    }
                    if (m.Header.Type == NLMSG_DONE)
                    {
                        _breakdone = true;
                        break;
                    }
                    if (m.Header.Type == NLMSG_ERROR)
                    {
                        return (null, EINVAL);
                    }
                }
            }
            return (tab, null);
        });

        // NetlinkMessage represents a netlink message.
        public partial struct NetlinkMessage
        {
            public NlMsghdr Header;
            public slice<byte> Data;
        }

        // ParseNetlinkMessage parses b as an array of netlink messages and
        // returns the slice containing the NetlinkMessage structures.
        public static (slice<NetlinkMessage>, error) ParseNetlinkMessage(slice<byte> b)
        {
            slice<NetlinkMessage> msgs = default;
            while (len(b) >= NLMSG_HDRLEN)
            {
                var (h, dbuf, dlen, err) = netlinkMessageHeaderAndData(b);
                if (err != null)
                {
                    return (null, err);
                }
                NetlinkMessage m = new NetlinkMessage(Header:*h,Data:dbuf[:int(h.Len)-NLMSG_HDRLEN]);
                msgs = append(msgs, m);
                b = b[dlen..];
            }

            return (msgs, null);
        }

        private static (ref NlMsghdr, slice<byte>, long, error) netlinkMessageHeaderAndData(slice<byte> b)
        {
            var h = (NlMsghdr.Value)(@unsafe.Pointer(ref b[0L]));
            var l = nlmAlignOf(int(h.Len));
            if (int(h.Len) < NLMSG_HDRLEN || l > len(b))
            {
                return (null, null, 0L, EINVAL);
            }
            return (h, b[NLMSG_HDRLEN..], l, null);
        }

        // NetlinkRouteAttr represents a netlink route attribute.
        public partial struct NetlinkRouteAttr
        {
            public RtAttr Attr;
            public slice<byte> Value;
        }

        // ParseNetlinkRouteAttr parses m's payload as an array of netlink
        // route attributes and returns the slice containing the
        // NetlinkRouteAttr structures.
        public static (slice<NetlinkRouteAttr>, error) ParseNetlinkRouteAttr(ref NetlinkMessage m)
        {
            slice<byte> b = default;

            if (m.Header.Type == RTM_NEWLINK || m.Header.Type == RTM_DELLINK) 
                b = m.Data[SizeofIfInfomsg..];
            else if (m.Header.Type == RTM_NEWADDR || m.Header.Type == RTM_DELADDR) 
                b = m.Data[SizeofIfAddrmsg..];
            else if (m.Header.Type == RTM_NEWROUTE || m.Header.Type == RTM_DELROUTE) 
                b = m.Data[SizeofRtMsg..];
            else 
                return (null, EINVAL);
                        slice<NetlinkRouteAttr> attrs = default;
            while (len(b) >= SizeofRtAttr)
            {
                var (a, vbuf, alen, err) = netlinkRouteAttrAndValue(b);
                if (err != null)
                {
                    return (null, err);
                }
                NetlinkRouteAttr ra = new NetlinkRouteAttr(Attr:*a,Value:vbuf[:int(a.Len)-SizeofRtAttr]);
                attrs = append(attrs, ra);
                b = b[alen..];
            }

            return (attrs, null);
        }

        private static (ref RtAttr, slice<byte>, long, error) netlinkRouteAttrAndValue(slice<byte> b)
        {
            var a = (RtAttr.Value)(@unsafe.Pointer(ref b[0L]));
            if (int(a.Len) < SizeofRtAttr || int(a.Len) > len(b))
            {
                return (null, null, 0L, EINVAL);
            }
            return (a, b[SizeofRtAttr..], rtaAlignOf(int(a.Len)), null);
        }
    }
}
