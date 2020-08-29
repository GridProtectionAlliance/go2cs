// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2020 August 29 10:12:18 UTC
// import "vendor/golang_org/x/net/lif" ==> using lif = go.vendor.golang_org.x.net.lif_package
// Original source: C:\Go\src\vendor\golang_org\x\net\lif\sys.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        private static binaryByteOrder nativeEndian = default;

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
        }
    }
}}}}}
