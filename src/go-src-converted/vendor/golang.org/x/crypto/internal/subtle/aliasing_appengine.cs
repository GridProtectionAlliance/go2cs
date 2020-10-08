// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build appengine

// Package subtle implements functions that are often useful in cryptographic
// code but require careful thought to use correctly.
// package subtle -- go2cs converted at 2020 October 08 05:00:15 UTC
// import "vendor/golang.org/x/crypto/internal/subtle" ==> using subtle = go.vendor.golang.org.x.crypto.@internal.subtle_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\internal\subtle\aliasing_appengine.go
// import "golang.org/x/crypto/internal/subtle"

// This is the Google App Engine standard variant based on reflect
// because the unsafe package and cgo are disallowed.

using reflect = go.reflect_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto {
namespace @internal
{
    public static partial class subtle_package
    {
        // AnyOverlap reports whether x and y share memory at any (not necessarily
        // corresponding) index. The memory beyond the slice length is ignored.
        public static bool AnyOverlap(slice<byte> x, slice<byte> y)
        {
            return len(x) > 0L && len(y) > 0L && reflect.ValueOf(_addr_x[0L]).Pointer() <= reflect.ValueOf(_addr_y[len(y) - 1L]).Pointer() && reflect.ValueOf(_addr_y[0L]).Pointer() <= reflect.ValueOf(_addr_x[len(x) - 1L]).Pointer();
        }

        // InexactOverlap reports whether x and y share memory at any non-corresponding
        // index. The memory beyond the slice length is ignored. Note that x and y can
        // have different lengths and still not have any inexact overlap.
        //
        // InexactOverlap can be used to implement the requirements of the crypto/cipher
        // AEAD, Block, BlockMode and Stream interfaces.
        public static bool InexactOverlap(slice<byte> x, slice<byte> y)
        {
            if (len(x) == 0L || len(y) == 0L || _addr_x[0L] == _addr_y[0L])
            {
                return false;
            }

            return AnyOverlap(x, y);

        }
    }
}}}}}}
