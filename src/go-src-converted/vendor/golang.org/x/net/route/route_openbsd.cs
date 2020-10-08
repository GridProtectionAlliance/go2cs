// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 October 08 05:01:42 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Go\src\vendor\golang.org\x\net\route\route_openbsd.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (slice<byte>, error) marshal(this ptr<RouteMessage> _addr_m)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref RouteMessage m = ref _addr_m.val;

            var l = sizeofRtMsghdr + addrsSpace(m.Addrs);
            var b = make_slice<byte>(l);
            nativeEndian.PutUint16(b[..2L], uint16(l));
            if (m.Version == 0L)
            {
                b[2L] = sysRTM_VERSION;
            }
            else
            {
                b[2L] = byte(m.Version);
            }
            b[3L] = byte(m.Type);
            nativeEndian.PutUint16(b[4L..6L], uint16(sizeofRtMsghdr));
            nativeEndian.PutUint32(b[16L..20L], uint32(m.Flags));
            nativeEndian.PutUint16(b[6L..8L], uint16(m.Index));
            nativeEndian.PutUint32(b[24L..28L], uint32(m.ID));
            nativeEndian.PutUint32(b[28L..32L], uint32(m.Seq));
            var (attrs, err) = marshalAddrs(b[sizeofRtMsghdr..], m.Addrs);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            if (attrs > 0L)
            {
                nativeEndian.PutUint32(b[12L..16L], uint32(attrs));
            }
            return (b, error.As(null!)!);

        }

        private static (Message, error) parseRouteMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat _p0 = ref _addr__p0.val;

            if (len(b) < sizeofRtMsghdr)
            {
                return (null, error.As(errMessageTooShort)!);
            }

            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            ptr<RouteMessage> m = addr(new RouteMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[16:20])),Index:int(nativeEndian.Uint16(b[6:8])),ID:uintptr(nativeEndian.Uint32(b[24:28])),Seq:int(nativeEndian.Uint32(b[28:32])),raw:b[:l],));
            var ll = int(nativeEndian.Uint16(b[4L..6L]));
            if (len(b) < ll)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            var errno = syscall.Errno(nativeEndian.Uint32(b[32L..36L]));
            if (errno != 0L)
            {
                m.Err = errno;
            }

            var (as, err) = parseAddrs(uint(nativeEndian.Uint32(b[12L..16L])), parseKernelInetAddr, b[ll..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            m.Addrs = as;
            return (m, error.As(null!)!);

        }
    }
}}}}}
