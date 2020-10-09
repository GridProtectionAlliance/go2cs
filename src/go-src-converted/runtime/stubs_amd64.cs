// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs_amd64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Called from compiled code; declared for vet; do NOT call from Go.
        private static void gcWriteBarrierCX()
;
        private static void gcWriteBarrierDX()
;
        private static void gcWriteBarrierBX()
;
        private static void gcWriteBarrierBP()
;
        private static void gcWriteBarrierSI()
;
        private static void gcWriteBarrierR8()
;
        private static void gcWriteBarrierR9()
;

        // stackcheck checks that SP is in range [g->stack.lo, g->stack.hi).
        private static void stackcheck()
;

        // Called from assembly only; declared for go vet.
        private static void settls()
; // argument in DI

        // Retpolines, used by -spectre=ret flag in cmd/asm, cmd/compile.
        private static void retpolineAX()
;
        private static void retpolineCX()
;
        private static void retpolineDX()
;
        private static void retpolineBX()
;
        private static void retpolineBP()
;
        private static void retpolineSI()
;
        private static void retpolineDI()
;
        private static void retpolineR8()
;
        private static void retpolineR9()
;
        private static void retpolineR10()
;
        private static void retpolineR11()
;
        private static void retpolineR12()
;
        private static void retpolineR13()
;
        private static void retpolineR14()
;
        private static void retpolineR15()
;
    }
}
