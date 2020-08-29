// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2020 August 29 08:16:21 UTC
// import "sync/atomic" ==> using atomic = go.sync.atomic_package
// Original source: C:\Go\src\sync\atomic\64bit_arm.go

using static go.builtin;

namespace go {
namespace sync
{
    public static partial class atomic_package
    {
        private static ulong loadUint64(ref ulong addr)
        {
            while (true)
            {
                val = addr.Value;
                if (CompareAndSwapUint64(addr, val, val))
                {
                    break;
                }
            }
            return;
        }

        private static void storeUint64(ref ulong addr, ulong val)
        {
            while (true)
            {
                var old = addr.Value;
                if (CompareAndSwapUint64(addr, old, val))
                {
                    break;
                }
            }

            return;
        }

        private static ulong addUint64(ref ulong val, ulong delta)
        {
            while (true)
            {
                var old = val.Value;
                new = old + delta;
                if (CompareAndSwapUint64(val, old, new))
                {
                    break;
                }
            }

            return;
        }

        private static ulong swapUint64(ref ulong addr, ulong @new)
        {
            while (true)
            {
                old = addr.Value;
                if (CompareAndSwapUint64(addr, old, new))
                {
                    break;
                }
            }

            return;
        }

        // Additional ARM-specific assembly routines.
        // Declaration here to give assembly routines correct stack maps for arguments.
        private static bool armCompareAndSwapUint32(ref uint addr, uint old, uint @new)
;
        private static bool armCompareAndSwapUint64(ref ulong addr, ulong old, ulong @new)
;
        private static bool generalCAS64(ref ulong addr, ulong old, ulong @new)
;
        private static uint armAddUint32(ref uint addr, uint delta)
;
        private static ulong armAddUint64(ref ulong addr, ulong delta)
;
        private static uint armSwapUint32(ref uint addr, uint @new)
;
        private static ulong armSwapUint64(ref ulong addr, ulong @new)
;
        private static ulong armLoadUint64(ref ulong addr)
;
        private static void armStoreUint64(ref ulong addr, ulong val)
;
    }
}}
