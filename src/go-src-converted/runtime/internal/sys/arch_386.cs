// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2020 August 29 08:16:26 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\arch_386.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        public static readonly var ArchFamily = I386;
        public static readonly var BigEndian = false;
        public static readonly long CacheLineSize = 64L;
        public static readonly var DefaultPhysPageSize = GoosNacl * 65536L + (1L - GoosNacl) * 4096L; // 4k normally; 64k on NaCl
        public static readonly long PCQuantum = 1L;
        public static readonly long Int64Align = 4L;
        public static readonly long HugePageSize = 1L << (int)(21L);
        public static readonly long MinFrameSize = 0L;

        public partial struct Uintreg // : uint
        {
        }
    }
}}}
