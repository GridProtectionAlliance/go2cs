// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 October 08 03:33:18 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\message.go

using static go.builtin;

namespace go {
namespace golang.org {
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

        public static readonly SysType SysMetrics = (SysType)iota;
        public static readonly var SysStats = (var)0;


        // ParseRIB parses b as a routing information base and returns a list
        // of routing messages.
        public static (slice<Message>, error) ParseRIB(RIBType typ, slice<byte> b)
        {
            slice<Message> _p0 = default;
            error _p0 = default!;

            if (!typ.parseable())
            {
                return (null, error.As(errUnsupportedMessage)!);
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
                    return (null, error.As(errInvalidMessage)!);
                }

                if (len(b) < l)
                {
                    return (null, error.As(errMessageTooShort)!);
                }

                if (b[2L] != rtmVersion)
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
                            return (null, error.As(err)!);
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
                return (null, error.As(errMessageMismatch)!);
            }

            return (msgs, error.As(null!)!);

        }
    }
}}}}
