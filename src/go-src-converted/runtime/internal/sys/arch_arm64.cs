// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2020 October 08 03:19:07 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\arch_arm64.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        public static readonly var ArchFamily = (var)ARM64;
        public static readonly var BigEndian = (var)false;
        public static readonly long DefaultPhysPageSize = (long)65536L;
        public static readonly long PCQuantum = (long)4L;
        public static readonly long Int64Align = (long)8L;
        public static readonly long MinFrameSize = (long)8L;


        public partial struct Uintreg // : ulong
        {
        }
    }
}}}
