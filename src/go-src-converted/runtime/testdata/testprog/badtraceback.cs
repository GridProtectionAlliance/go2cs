// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 03:43:31 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\badtraceback.go
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("BadTraceback", BadTraceback);
        }

        public static void BadTraceback()
        { 
            // Disable GC to prevent traceback at unexpected time.
            debug.SetGCPercent(-1L); 

            // Run badLR1 on its own stack to minimize the stack size and
            // exercise the stack bounds logic in the hex dump.
            go_(() => badLR1());

        }

        //go:noinline
        private static void badLR1()
        { 
            // We need two frames on LR machines because we'll smash this
            // frame's saved LR.
            badLR2(0L);

        }

        //go:noinline
        private static void badLR2(long arg) => func((_, panic, __) =>
        { 
            // Smash the return PC or saved LR.
            var lrOff = @unsafe.Sizeof(uintptr(0L));
            if (runtime.GOARCH == "ppc64" || runtime.GOARCH == "ppc64le")
            {
                lrOff = 32L; // FIXED_FRAME or sys.MinFrameSize
            }

            var lrPtr = (uintptr.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(_addr_arg)) - lrOff));
            lrPtr.val = 0xbadUL; 

            // Print a backtrace. This should include diagnostics for the
            // bad return PC and a hex dump.
            panic("backtrace");

        });
    }
}
