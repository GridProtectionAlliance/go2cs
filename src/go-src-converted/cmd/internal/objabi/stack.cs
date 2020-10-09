// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 October 09 05:08:53 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\stack.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // For the linkers. Must match Go definitions.
        public static readonly long STACKSYSTEM = (long)0L;
        public static readonly var StackSystem = STACKSYSTEM;
        public static readonly long StackBig = (long)4096L;
        public static readonly long StackSmall = (long)128L;


        public static readonly long StackPreempt = (long)-1314L; // 0xfff...fade

        // Initialize StackGuard and StackLimit according to target system.
        public static long StackGuard = 928L * stackGuardMultiplier() + StackSystem;
        public static var StackLimit = StackGuard - StackSystem - StackSmall;

        // stackGuardMultiplier returns a multiplier to apply to the default
        // stack guard size. Larger multipliers are used for non-optimized
        // builds that have larger stack frames or for specific targets.
        private static long stackGuardMultiplier()
        { 
            // On AIX, a larger stack is needed for syscalls.
            if (GOOS == "aix")
            {
                return 2L;
            }

            return stackGuardMultiplierDefault;

        }
    }
}}}
