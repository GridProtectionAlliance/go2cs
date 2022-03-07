// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2022 March 06 22:32:24 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Program Files\Go\src\cmd\internal\objabi\stack.go
using buildcfg = go.@internal.buildcfg_package;

namespace go.cmd.@internal;

public static partial class objabi_package {

    // For the linkers. Must match Go definitions.
public static readonly nint STACKSYSTEM = 0;
public static readonly var StackSystem = STACKSYSTEM;
public static readonly nint StackBig = 4096;
public static readonly nint StackSmall = 128;


// Initialize StackGuard and StackLimit according to target system.
public static nint StackGuard = 928 * stackGuardMultiplier() + StackSystem;
public static var StackLimit = StackGuard - StackSystem - StackSmall;

// stackGuardMultiplier returns a multiplier to apply to the default
// stack guard size. Larger multipliers are used for non-optimized
// builds that have larger stack frames or for specific targets.
private static nint stackGuardMultiplier() { 
    // On AIX, a larger stack is needed for syscalls.
    if (buildcfg.GOOS == "aix") {
        return 2;
    }
    return stackGuardMultiplierDefault;

}

} // end objabi_package
