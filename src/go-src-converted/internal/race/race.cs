// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build race

// package race -- go2cs converted at 2020 October 08 00:34:04 UTC
// import "internal/race" ==> using race = go.@internal.race_package
// Original source: C:\Go\src\internal\race\race.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class race_package
    {
        public static readonly var Enabled = (var)true;



        public static void Acquire(unsafe.Pointer addr)
        {
            runtime.RaceAcquire(addr);
        }

        public static void Release(unsafe.Pointer addr)
        {
            runtime.RaceRelease(addr);
        }

        public static void ReleaseMerge(unsafe.Pointer addr)
        {
            runtime.RaceReleaseMerge(addr);
        }

        public static void Disable()
        {
            runtime.RaceDisable();
        }

        public static void Enable()
        {
            runtime.RaceEnable();
        }

        public static void Read(unsafe.Pointer addr)
        {
            runtime.RaceRead(addr);
        }

        public static void Write(unsafe.Pointer addr)
        {
            runtime.RaceWrite(addr);
        }

        public static void ReadRange(unsafe.Pointer addr, long len)
        {
            runtime.RaceReadRange(addr, len);
        }

        public static void WriteRange(unsafe.Pointer addr, long len)
        {
            runtime.RaceWriteRange(addr, len);
        }

        public static long Errors()
        {
            return runtime.RaceErrors();
        }
    }
}}
