// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd

// package route -- go2cs converted at 2020 August 29 10:12:35 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\route_classic.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (slice<byte>, error) marshal(this ref RouteMessage m)
        {
            var (w, ok) = wireFormats[m.Type];
            if (!ok)
            {
                return (null, errUnsupportedMessage);
            }
            var l = w.bodyOff + addrsSpace(m.Addrs);
            if (runtime.GOOS == "darwin")
            { 
                // Fix stray pointer writes on macOS.
                // See golang.org/issue/22456.
                l += 1024L;
            }
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
            nativeEndian.PutUint32(b[8L..12L], uint32(m.Flags));
            nativeEndian.PutUint16(b[4L..6L], uint16(m.Index));
            nativeEndian.PutUint32(b[16L..20L], uint32(m.ID));
            nativeEndian.PutUint32(b[20L..24L], uint32(m.Seq));
            var (attrs, err) = marshalAddrs(b[w.bodyOff..], m.Addrs);
            if (err != null)
            {
                return (null, err);
            }
            if (attrs > 0L)
            {
                nativeEndian.PutUint32(b[12L..16L], uint32(attrs));
            }
            return (b, null);
        }

        private static (Message, error) parseRouteMessage(this ref wireFormat w, RIBType typ, slice<byte> b)
        {
            if (len(b) < w.bodyOff)
            {
                return (null, errMessageTooShort);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, errInvalidMessage);
            }
            RouteMessage m = ref new RouteMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[4:6])),ID:uintptr(nativeEndian.Uint32(b[16:20])),Seq:int(nativeEndian.Uint32(b[20:24])),extOff:w.extOff,raw:b[:l],);
            var errno = syscall.Errno(nativeEndian.Uint32(b[28L..32L]));
            if (errno != 0L)
            {
                m.Err = errno;
            }
            error err = default;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[12L..16L])), parseKernelInetAddr, b[w.bodyOff..]);
            if (err != null)
            {
                return (null, err);
            }
            return (m, null);
        }
    }
}}}}}
