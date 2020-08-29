// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !race

// Dummy race detection API, used when not built with -race.

// package runtime -- go2cs converted at 2020 August 29 08:19:44 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\race0.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var raceenabled = false;

        // Because raceenabled is false, none of these functions should be called.



        // Because raceenabled is false, none of these functions should be called.

        private static void raceReadObjectPC(ref _type t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            throw("race");

        }
        private static void raceWriteObjectPC(ref _type t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            throw("race");

        }
        private static (System.UIntPtr, System.UIntPtr) raceinit()
        {
            throw("race");

            return (0L, 0L);
        }
        private static void racefini()
        {
            throw("race");

        }
        private static System.UIntPtr raceproccreate()
        {
            throw("race");

            return 0L;
        }
        private static void raceprocdestroy(System.UIntPtr ctx)
        {
            throw("race");

        }
        private static void racemapshadow(unsafe.Pointer addr, System.UIntPtr size)
        {
            throw("race");

        }
        private static void racewritepc(unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            throw("race");

        }
        private static void racereadpc(unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            throw("race");

        }
        private static void racereadrangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            throw("race");

        }
        private static void racewriterangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            throw("race");

        }
        private static void raceacquire(unsafe.Pointer addr)
        {
            throw("race");

        }
        private static void raceacquireg(ref g gp, unsafe.Pointer addr)
        {
            throw("race");

        }
        private static void racerelease(unsafe.Pointer addr)
        {
            throw("race");

        }
        private static void racereleaseg(ref g gp, unsafe.Pointer addr)
        {
            throw("race");

        }
        private static void racereleasemerge(unsafe.Pointer addr)
        {
            throw("race");

        }
        private static void racereleasemergeg(ref g gp, unsafe.Pointer addr)
        {
            throw("race");

        }
        private static void racefingo()
        {
            throw("race");

        }
        private static void racemalloc(unsafe.Pointer p, System.UIntPtr sz)
        {
            throw("race");

        }
        private static void racefree(unsafe.Pointer p, System.UIntPtr sz)
        {
            throw("race");

        }
        private static System.UIntPtr racegostart(System.UIntPtr pc)
        {
            throw("race");

            return 0L;
        }
        private static void racegoend()
        {
            throw("race");

        }
    }
}
