// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 October 09 06:07:47 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Go\src\vendor\golang.org\x\net\route\interface_freebsd.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMessage(this ptr<wireFormat> _addr_w, RIBType typ, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat w = ref _addr_w.val;

            long extOff = default;            long bodyOff = default;

            if (typ == sysNET_RT_IFLISTL)
            {
                if (len(b) < 20L)
                {
                    return (null, error.As(errMessageTooShort)!);
                }
                extOff = int(nativeEndian.Uint16(b[18L..20L]));
                bodyOff = int(nativeEndian.Uint16(b[16L..18L]));

            }
            else
            {
                extOff = w.extOff;
                bodyOff = w.bodyOff;
            }
            if (len(b) < extOff || len(b) < bodyOff)
            {
                return (null, error.As(errInvalidMessage)!);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }
            var attrs = uint(nativeEndian.Uint32(b[4L..8L]));
            if (attrs & sysRTA_IFP == 0L)
            {
                return (null, error.As(null!)!);
            }
            ptr<InterfaceMessage> m = addr(new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),Addrs:make([]Addr,sysRTAX_MAX),extOff:extOff,raw:b[:l],));
            var (a, err) = parseLinkAddr(b[bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            m.Addrs[sysRTAX_IFP] = a;
            m.Name = a._<ptr<LinkAddr>>().Name;
            return (m, error.As(null!)!);

        }

        private static (Message, error) parseInterfaceAddrMessage(this ptr<wireFormat> _addr_w, RIBType typ, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat w = ref _addr_w.val;

            long bodyOff = default;
            if (typ == sysNET_RT_IFLISTL)
            {
                if (len(b) < 24L)
                {
                    return (null, error.As(errMessageTooShort)!);
                }

                bodyOff = int(nativeEndian.Uint16(b[16L..18L]));

            }
            else
            {
                bodyOff = w.bodyOff;
            }

            if (len(b) < bodyOff)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            ptr<InterfaceAddrMessage> m = addr(new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),raw:b[:l],));
            error err = default!;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[4L..8L])), parseKernelInetAddr, b[bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (m, error.As(null!)!);

        }
    }
}}}}}
