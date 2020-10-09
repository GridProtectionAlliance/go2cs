// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd

// package route -- go2cs converted at 2020 October 09 04:51:39 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\route_classic.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
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

            var (w, ok) = wireFormats[m.Type];
            if (!ok)
            {
                return (null, error.As(errUnsupportedMessage)!);
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
                b[2L] = rtmVersion;
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
                return (null, error.As(err)!);
            }
            if (attrs > 0L)
            {
                nativeEndian.PutUint32(b[12L..16L], uint32(attrs));
            }
            return (b, error.As(null!)!);

        }

        private static (Message, error) parseRouteMessage(this ptr<wireFormat> _addr_w, RIBType typ, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat w = ref _addr_w.val;

            if (len(b) < w.bodyOff)
            {
                return (null, error.As(errMessageTooShort)!);
            }

            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }

            ptr<RouteMessage> m = addr(new RouteMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[4:6])),ID:uintptr(nativeEndian.Uint32(b[16:20])),Seq:int(nativeEndian.Uint32(b[20:24])),extOff:w.extOff,raw:b[:l],));
            var errno = syscall.Errno(nativeEndian.Uint32(b[28L..32L]));
            if (errno != 0L)
            {
                m.Err = errno;
            }

            error err = default!;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[12L..16L])), parseKernelInetAddr, b[w.bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (m, error.As(null!)!);

        }
    }
}}}}
