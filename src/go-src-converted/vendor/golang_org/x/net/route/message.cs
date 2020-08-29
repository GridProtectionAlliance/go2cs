// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 August 29 10:12:34 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\message.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        // A Message represents a routing message.
        public partial interface Message
        {
            slice<Sys> Sys();
        }

        // A Sys reprensents operating system-specific information.
        public partial interface Sys
        {
            SysType SysType();
        }

        // A SysType represents a type of operating system-specific
        // information.
        public partial struct SysType // : long
        {
        }

        public static readonly SysType SysMetrics = iota;
        public static readonly var SysStats = 0;

        // ParseRIB parses b as a routing information base and returns a list
        // of routing messages.
        public static (slice<Message>, error) ParseRIB(RIBType typ, slice<byte> b)
        {
            if (!typ.parseable())
            {
                return (null, errUnsupportedMessage);
            }
            slice<Message> msgs = default;
            long nmsgs = 0L;
            long nskips = 0L;
            while (len(b) > 4L)
            {
                nmsgs++;
                var l = int(nativeEndian.Uint16(b[..2L]));
                if (l == 0L)
                {
                    return (null, errInvalidMessage);
                }
                if (len(b) < l)
                {
                    return (null, errMessageTooShort);
                }
                if (b[2L] != sysRTM_VERSION)
                {
                    b = b[l..];
                    continue;
                }
                {
                    var (w, ok) = wireFormats[int(b[3L])];

                    if (!ok)
                    {
                        nskips++;
                    }
                    else
                    {
                        var (m, err) = w.parse(typ, b);
                        if (err != null)
                        {
                            return (null, err);
                        }
                        if (m == null)
                        {
                            nskips++;
                        }
                        else
                        {
                            msgs = append(msgs, m);
                        }
                    }

                }
                b = b[l..];
            } 
            // We failed to parse any of the messages - version mismatch?
 
            // We failed to parse any of the messages - version mismatch?
            if (nmsgs != len(msgs) + nskips)
            {
                return (null, errMessageMismatch);
            }
            return (msgs, null);
        }
    }
}}}}}
