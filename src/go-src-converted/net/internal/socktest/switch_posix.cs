// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9

// package socktest -- go2cs converted at 2020 October 09 05:00:24 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Go\src\net\internal\socktest\switch_posix.go
using fmt = go.fmt_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        private static @string familyString(long family)
        {

            if (family == syscall.AF_INET) 
                return "inet4";
            else if (family == syscall.AF_INET6) 
                return "inet6";
            else if (family == syscall.AF_UNIX) 
                return "local";
            else 
                return fmt.Sprintf("%d", family);
            
        }

        private static @string typeString(long sotype)
        {
            @string s = default;

            if (sotype & 0xffUL == syscall.SOCK_STREAM) 
                s = "stream";
            else if (sotype & 0xffUL == syscall.SOCK_DGRAM) 
                s = "datagram";
            else if (sotype & 0xffUL == syscall.SOCK_RAW) 
                s = "raw";
            else if (sotype & 0xffUL == syscall.SOCK_SEQPACKET) 
                s = "seqpacket";
            else 
                s = fmt.Sprintf("%d", sotype & 0xffUL);
                        {
                var flags = uint(sotype) & ~uint(0xffUL);

                if (flags != 0L)
                {
                    s += fmt.Sprintf("|%#x", flags);
                }

            }

            return s;

        }

        private static @string protocolString(long proto)
        {

            if (proto == 0L) 
                return "default";
            else if (proto == syscall.IPPROTO_TCP) 
                return "tcp";
            else if (proto == syscall.IPPROTO_UDP) 
                return "udp";
            else 
                return fmt.Sprintf("%d", proto);
            
        }
    }
}}}
