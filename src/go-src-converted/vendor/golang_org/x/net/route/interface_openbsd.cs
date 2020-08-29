// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 August 29 10:12:34 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\interface_openbsd.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMessage(this ref wireFormat _p0, RIBType _, slice<byte> b)
        {
            if (len(b) < 32L)
            {
                return (null, errMessageTooShort);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, errInvalidMessage);
            }
            var attrs = uint(nativeEndian.Uint32(b[12L..16L]));
            if (attrs & sysRTA_IFP == 0L)
            {
                return (null, null);
            }
            InterfaceMessage m = ref new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[16:20])),Index:int(nativeEndian.Uint16(b[6:8])),Addrs:make([]Addr,sysRTAX_MAX),raw:b[:l],);
            var ll = int(nativeEndian.Uint16(b[4L..6L]));
            if (len(b) < ll)
            {
                return (null, errInvalidMessage);
            }
            var (a, err) = parseLinkAddr(b[ll..]);
            if (err != null)
            {
                return (null, err);
            }
            m.Addrs[sysRTAX_IFP] = a;
            m.Name = a._<ref LinkAddr>().Name;
            return (m, null);
        }

        private static (Message, error) parseInterfaceAddrMessage(this ref wireFormat _p0, RIBType _, slice<byte> b)
        {
            if (len(b) < 24L)
            {
                return (null, errMessageTooShort);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, errInvalidMessage);
            }
            var bodyOff = int(nativeEndian.Uint16(b[4L..6L]));
            if (len(b) < bodyOff)
            {
                return (null, errInvalidMessage);
            }
            InterfaceAddrMessage m = ref new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[12:16])),Index:int(nativeEndian.Uint16(b[6:8])),raw:b[:l],);
            error err = default;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[12L..16L])), parseKernelInetAddr, b[bodyOff..]);
            if (err != null)
            {
                return (null, err);
            }
            return (m, null);
        }

        private static (Message, error) parseInterfaceAnnounceMessage(this ref wireFormat _p0, RIBType _, slice<byte> b)
        {
            if (len(b) < 26L)
            {
                return (null, errMessageTooShort);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, errInvalidMessage);
            }
            InterfaceAnnounceMessage m = ref new InterfaceAnnounceMessage(Version:int(b[2]),Type:int(b[3]),Index:int(nativeEndian.Uint16(b[6:8])),What:int(nativeEndian.Uint16(b[8:10])),raw:b[:l],);
            for (long i = 0L; i < 16L; i++)
            {
                if (b[10L + i] != 0L)
                {
                    continue;
                }
                m.Name = string(b[10L..10L + i]);
                break;
            }

            return (m, null);
        }
    }
}}}}}
