// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 August 29 08:46:20 UTC
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
        public static readonly long STACKSYSTEM = 0L;
        public static readonly var StackSystem = STACKSYSTEM;
        public static readonly long StackBig = 4096L;
        public static readonly long StackGuard = 880L * stackGuardMultiplier + StackSystem;
        public static readonly long StackSmall = 128L;
        public static readonly var StackLimit = StackGuard - StackSystem - StackSmall;

        public static readonly long StackPreempt = -1314L; // 0xfff...fade
    }
}}}
