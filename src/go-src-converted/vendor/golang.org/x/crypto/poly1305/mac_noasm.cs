// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!ppc64le,!s390x gccgo purego

// package poly1305 -- go2cs converted at 2020 October 09 06:06:33 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\poly1305\mac_noasm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        private partial struct mac
        {
            public ref macGeneric macGeneric => ref macGeneric_val;
        }
    }
}}}}}
