// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 October 08 03:33:22 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\sys.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static binaryByteOrder nativeEndian = default;        private static long kernelAlign = default;        private static byte rtmVersion = default;        private static map<long, ptr<wireFormat>> wireFormats = default;

        private static void init()
        {
            ref var i = ref heap(uint32(1L), out ptr<var> _addr_i);
            ptr<array<byte>> b = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_i));
            if (b[0L] == 1L)
            {
                nativeEndian = littleEndian;
            }
            else
            {
                nativeEndian = bigEndian;
            } 
            // might get overridden in probeRoutingStack
            rtmVersion = sysRTM_VERSION;
            kernelAlign, wireFormats = probeRoutingStack();

        }

        private static long roundup(long l)
        {
            if (l == 0L)
            {
                return kernelAlign;
            }

            return (l + kernelAlign - 1L) & ~(kernelAlign - 1L);

        }

        private partial struct wireFormat
        {
            public long extOff; // offset of header extension
            public long bodyOff; // offset of message body
            public Func<RIBType, slice<byte>, (Message, error)> parse;
        }
    }
}}}}
