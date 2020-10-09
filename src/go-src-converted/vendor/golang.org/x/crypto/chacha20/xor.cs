// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found src the LICENSE file.

// package chacha20 -- go2cs converted at 2020 October 09 06:06:15 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20\xor.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20_package
    {
        // Platforms that have fast unaligned 32-bit little endian accesses.
        private static readonly var unaligned = runtime.GOARCH == "386" || runtime.GOARCH == "amd64" || runtime.GOARCH == "arm64" || runtime.GOARCH == "ppc64le" || runtime.GOARCH == "s390x";

        // addXor reads a little endian uint32 from src, XORs it with (a + b) and
        // places the result in little endian byte order in dst.


        // addXor reads a little endian uint32 from src, XORs it with (a + b) and
        // places the result in little endian byte order in dst.
        private static void addXor(slice<byte> dst, slice<byte> src, uint a, uint b)
        {
            _ = src[3L];
            _ = dst[3L]; // bounds check elimination hint
            if (unaligned)
            { 
                // The compiler should optimize this code into
                // 32-bit unaligned little endian loads and stores.
                // TODO: delete once the compiler does a reliably
                // good job with the generic code below.
                // See issue #25111 for more details.
                var v = uint32(src[0L]);
                v |= uint32(src[1L]) << (int)(8L);
                v |= uint32(src[2L]) << (int)(16L);
                v |= uint32(src[3L]) << (int)(24L);
                v ^= a + b;
                dst[0L] = byte(v);
                dst[1L] = byte(v >> (int)(8L));
                dst[2L] = byte(v >> (int)(16L));
                dst[3L] = byte(v >> (int)(24L));

            }
            else
            {
                a += b;
                dst[0L] = src[0L] ^ byte(a);
                dst[1L] = src[1L] ^ byte(a >> (int)(8L));
                dst[2L] = src[2L] ^ byte(a >> (int)(16L));
                dst[3L] = src[3L] ^ byte(a >> (int)(24L));
            }

        }
    }
}}}}}
