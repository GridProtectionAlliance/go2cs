// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found src the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

using runtime = runtime_package;

partial class chacha20_package {

// Platforms that have fast unaligned 32-bit little endian accesses.
internal const bool unaligned = /* runtime.GOARCH == "386" ||
	runtime.GOARCH == "amd64" ||
	runtime.GOARCH == "arm64" ||
	runtime.GOARCH == "ppc64le" ||
	runtime.GOARCH == "s390x" */ true;

// addXor reads a little endian uint32 from src, XORs it with (a + b) and
// places the result in little endian byte order in dst.
internal static void addXor(slice<byte> dst, slice<byte> src, uint32 a, uint32 b) {
    var _ = src[3];
    var _ = dst[3];
    // bounds check elimination hint
    if (unaligned){
        // The compiler should optimize this code into
        // 32-bit unaligned little endian loads and stores.
        // TODO: delete once the compiler does a reliably
        // good job with the generic code below.
        // See issue #25111 for more details.
        var v = ((uint32)src[0]);
        v |= (uint32)(((uint32)src[1]) << (int)(8));
        v |= (uint32)(((uint32)src[2]) << (int)(16));
        v |= (uint32)(((uint32)src[3]) << (int)(24));
        v ^= (uint32)(a + b);
        dst[0] = ((byte)v);
        dst[1] = ((byte)(v >> (int)(8)));
        dst[2] = ((byte)(v >> (int)(16)));
        dst[3] = ((byte)(v >> (int)(24)));
    } else {
        a += b;
        dst[0] = (byte)(src[0] ^ ((byte)a));
        dst[1] = (byte)(src[1] ^ ((byte)(a >> (int)(8))));
        dst[2] = (byte)(src[2] ^ ((byte)(a >> (int)(16))));
        dst[3] = (byte)(src[3] ^ ((byte)(a >> (int)(24))));
    }
}

} // end chacha20_package
