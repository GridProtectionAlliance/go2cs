// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:18 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\badtraceback.go
namespace go;

using runtime = runtime_package;
using debug = runtime.debug_package;
using @unsafe = @unsafe_package;
using System.Threading;

public static partial class main_package {

private static void init() {
    register("BadTraceback", BadTraceback);
}

public static void BadTraceback() { 
    // Disable GC to prevent traceback at unexpected time.
    debug.SetGCPercent(-1); 

    // Run badLR1 on its own stack to minimize the stack size and
    // exercise the stack bounds logic in the hex dump.
    go_(() => badLR1());
}

//go:noinline
private static void badLR1() { 
    // We need two frames on LR machines because we'll smash this
    // frame's saved LR.
    badLR2(0);
}

//go:noinline
private static void badLR2(nint arg) => func((_, panic, _) => { 
    // Smash the return PC or saved LR.
    var lrOff = @unsafe.Sizeof(uintptr(0));
    if (runtime.GOARCH == "ppc64" || runtime.GOARCH == "ppc64le") {
        lrOff = 32; // FIXED_FRAME or sys.MinFrameSize
    }
    var lrPtr = (uintptr.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(_addr_arg)) - lrOff));
    lrPtr.val = 0xbad; 

    // Print a backtrace. This should include diagnostics for the
    // bad return PC and a hex dump.
    panic("backtrace");
});

} // end main_package
