// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2020 August 29 10:12:38 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\sys.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static binaryByteOrder nativeEndian = default;        private static long kernelAlign = default;        private static map<long, ref wireFormat> wireFormats = default;

        private static void init()
        {
            var i = uint32(1L);
            ref array<byte> b = new ptr<ref array<byte>>(@unsafe.Pointer(ref i));
            if (b[0L] == 1L)
            {
                nativeEndian = littleEndian;
            }
            else
            {
                nativeEndian = bigEndian;
            }
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
}}}}}
