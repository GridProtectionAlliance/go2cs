// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2020 August 29 08:16:26 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\arch_mips64le.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        public static readonly var ArchFamily = MIPS64;
        public static readonly var BigEndian = false;
        public static readonly long CacheLineSize = 32L;
        public static readonly long DefaultPhysPageSize = 16384L;
        public static readonly long PCQuantum = 4L;
        public static readonly long Int64Align = 8L;
        public static readonly long HugePageSize = 0L;
        public static readonly long MinFrameSize = 8L;

        public partial struct Uintreg // : ulong
        {
        }
    }
}}}
