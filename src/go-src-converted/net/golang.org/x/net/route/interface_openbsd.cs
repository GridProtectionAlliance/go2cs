// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 October 09 04:51:38 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\interface_openbsd.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat _p0 = ref _addr__p0.val;

            if (len(b) < 32L)
            {
                return (null, error.As(errMessageTooShort)!);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }
            var attrs = uint(nativeEndian.Uint32(b[12L..16L]));
            if (attrs & sysRTA_IFP == 0L)
            {
                return (null, error.As(null!)!);
            }
            ptr<InterfaceMessage> m = addr(new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[16:20])),Index:int(nativeEndian.Uint16(b[6:8])),Addrs:make([]Addr,sysRTAX_MAX),raw:b[:l],));
            var ll = int(nativeEndian.Uint16(b[4L..6L]));
            if (len(b) < ll)
            {
                return (null, error.As(errInvalidMessage)!);
            }
            var (a, err) = parseLinkAddr(b[ll..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            m.Addrs[sysRTAX_IFP] = a;
            m.Name = a._<ptr<LinkAddr>>().Name;
            return (m, error.As(null!)!);

        }

        private static (Message, error) parseInterfaceAddrMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat _p0 = ref _addr__p0.val;

            if (len(b) < 24L)
            {
                return (null, error.As(errMessageTooShort)!);
            }

            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            var bodyOff = int(nativeEndian.Uint16(b[4L..6L]));
            if (len(b) < bodyOff)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            ptr<InterfaceAddrMessage> m = addr(new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[12:16])),Index:int(nativeEndian.Uint16(b[6:8])),raw:b[:l],));
            error err = default!;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[12L..16L])), parseKernelInetAddr, b[bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (m, error.As(null!)!);

        }

        private static (Message, error) parseInterfaceAnnounceMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat _p0 = ref _addr__p0.val;

            if (len(b) < 26L)
            {
                return (null, error.As(errMessageTooShort)!);
            }

            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            ptr<InterfaceAnnounceMessage> m = addr(new InterfaceAnnounceMessage(Version:int(b[2]),Type:int(b[3]),Index:int(nativeEndian.Uint16(b[6:8])),What:int(nativeEndian.Uint16(b[8:10])),raw:b[:l],));
            for (long i = 0L; i < 16L; i++)
            {
                if (b[10L + i] != 0L)
                {
                    continue;
                }

                m.Name = string(b[10L..10L + i]);
                break;

            }

            return (m, error.As(null!)!);

        }
    }
}}}}
