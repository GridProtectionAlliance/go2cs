// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd

// package route -- go2cs converted at 2020 October 09 04:51:37 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\interface_announce.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceAnnounceMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b)
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
            ptr<InterfaceAnnounceMessage> m = addr(new InterfaceAnnounceMessage(Version:int(b[2]),Type:int(b[3]),Index:int(nativeEndian.Uint16(b[4:6])),What:int(nativeEndian.Uint16(b[22:24])),raw:b[:l],));
            for (long i = 0L; i < 16L; i++)
            {
                if (b[6L + i] != 0L)
                {
                    continue;
                }
                m.Name = string(b[6L..6L + i]);
                break;

            }
            return (m, error.As(null!)!);

        }
    }
}}}}
