// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x,!ppc64le,!arm64

// package crc32 -- go2cs converted at 2020 October 09 04:50:07 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32_otherarch.go

using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        private static bool archAvailableIEEE()
        {
            return false;
        }
        private static void archInitIEEE() => func((_, panic, __) =>
        {
            panic("not available");
        });
        private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            panic("not available");
        });

        private static bool archAvailableCastagnoli()
        {
            return false;
        }
        private static void archInitCastagnoli() => func((_, panic, __) =>
        {
            panic("not available");
        });
        private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            panic("not available");
        });
    }
}}
