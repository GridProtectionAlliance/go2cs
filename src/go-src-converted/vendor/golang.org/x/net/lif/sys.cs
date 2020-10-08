// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2020 October 08 05:01:25 UTC
// import "vendor/golang.org/x/net/lif" ==> using lif = go.vendor.golang.org.x.net.lif_package
// Original source: C:\Go\src\vendor\golang.org\x\net\lif\sys.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        private static binaryByteOrder nativeEndian = default;

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

        }
    }
}}}}}
