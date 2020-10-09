// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2020 October 09 04:45:30 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\stubs.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        // Declarations for runtime services implemented in C or assembly.
        public static readonly long PtrSize = (long)4L << (int)((~uintptr(0L) >> (int)(63L))); // unsafe.Sizeof(uintptr(0)) but an ideal const
 // unsafe.Sizeof(uintptr(0)) but an ideal const
        public static readonly long RegSize = (long)4L << (int)((~Uintreg(0L) >> (int)(63L))); // unsafe.Sizeof(uintreg(0)) but an ideal const
 // unsafe.Sizeof(uintreg(0)) but an ideal const
        public static readonly long SpAlign = (long)1L * (1L - GoarchArm64) + 16L * GoarchArm64; // SP alignment: 1 normally, 16 for ARM64

 // SP alignment: 1 normally, 16 for ARM64

        public static @string DefaultGoroot = default; // set at link time

        // AIX requires a larger stack for syscalls.
        public static readonly var StackGuardMultiplier = StackGuardMultiplierDefault * (1L - GoosAix) + 2L * GoosAix;

    }
}}}
