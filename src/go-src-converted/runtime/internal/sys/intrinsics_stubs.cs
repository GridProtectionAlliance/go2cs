// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386

// package sys -- go2cs converted at 2020 October 09 04:45:30 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\intrinsics_stubs.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        public static long Ctz64(ulong x)
;
        public static long Ctz32(uint x)
;
        public static long Ctz8(byte x)
;
        public static ulong Bswap64(ulong x)
;
        public static uint Bswap32(uint x)
;
    }
}}}
