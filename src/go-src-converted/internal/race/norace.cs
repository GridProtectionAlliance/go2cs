// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !race

// package race -- go2cs converted at 2020 October 09 04:45:27 UTC
// import "internal/race" ==> using race = go.@internal.race_package
// Original source: C:\Go\src\internal\race\norace.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class race_package
    {
        public static readonly var Enabled = false;



        public static void Acquire(unsafe.Pointer addr)
        {
        }

        public static void Release(unsafe.Pointer addr)
        {
        }

        public static void ReleaseMerge(unsafe.Pointer addr)
        {
        }

        public static void Disable()
        {
        }

        public static void Enable()
        {
        }

        public static void Read(unsafe.Pointer addr)
        {
        }

        public static void Write(unsafe.Pointer addr)
        {
        }

        public static void ReadRange(unsafe.Pointer addr, long len)
        {
        }

        public static void WriteRange(unsafe.Pointer addr, long len)
        {
        }

        public static long Errors()
        {
            return 0L;
        }
    }
}}
