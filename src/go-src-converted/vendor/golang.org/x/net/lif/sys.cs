// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2022 March 13 06:46:23 UTC
// import "vendor/golang.org/x/net/lif" ==> using lif = go.vendor.golang.org.x.net.lif_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\lif\sys.go
namespace go.vendor.golang.org.x.net;

using @unsafe = @unsafe_package;

public static partial class lif_package {

private static binaryByteOrder nativeEndian = default;

private static void init() {
    ref var i = ref heap(uint32(1), out ptr<var> _addr_i);
    ptr<array<byte>> b = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_i));
    if (b[0] == 1) {
        nativeEndian = littleEndian;
    }
    else
 {
        nativeEndian = bigEndian;
    }
}

} // end lif_package
