// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64 gccgo appengine purego

// package curve25519 -- go2cs converted at 2020 October 09 06:06:31 UTC
// import "vendor/golang.org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang.org.x.crypto.curve25519_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\curve25519\curve25519_noasm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class curve25519_package
    {
        private static void scalarMult(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_@in, ptr<array<byte>> _addr_@base)
        {
            ref array<byte> @out = ref _addr_@out.val;
            ref array<byte> @in = ref _addr_@in.val;
            ref array<byte> @base = ref _addr_@base.val;

            scalarMultGeneric(out, in, base);
        }
    }
}}}}}
