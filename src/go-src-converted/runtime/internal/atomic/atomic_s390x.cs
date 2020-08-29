// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2020 August 29 08:16:30 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_s390x.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static unsafe partial class atomic_package
    {
        //go:nosplit
        //go:noinline
        public static uint Load(ref uint ptr)
        {
            return ptr.Value;
        }

        //go:nosplit
        //go:noinline
        public static unsafe.Pointer Loadp(unsafe.Pointer ptr)
        {
            return ptr.Value;
        }

        //go:nosplit
        //go:noinline
        public static ulong Load64(ref ulong ptr)
        {
            return ptr.Value;
        }

        //go:noinline
        //go:nosplit
        public static void Store(ref uint ptr, uint val)
        {
            ptr.Value = val;
        }

        //go:noinline
        //go:nosplit
        public static void Store64(ref ulong ptr, ulong val)
        {
            ptr.Value = val;
        }

        // NO go:noescape annotation; see atomic_pointer.go.
        //go:noinline
        //go:nosplit
        public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val)
        {
            (uintptr.Value)(ptr).Value;

            uintptr(val);
        }

        //go:noescape
        public static void And8(ref byte ptr, byte val)
;

        //go:noescape
        public static void Or8(ref byte ptr, byte val)
;

        // NOTE: Do not add atomicxor8 (XOR is not idempotent).

        //go:noescape
        public static uint Xadd(ref uint ptr, int delta)
;

        //go:noescape
        public static ulong Xadd64(ref ulong ptr, long delta)
;

        //go:noescape
        public static System.UIntPtr Xadduintptr(ref System.UIntPtr ptr, System.UIntPtr delta)
;

        //go:noescape
        public static uint Xchg(ref uint ptr, uint @new)
;

        //go:noescape
        public static ulong Xchg64(ref ulong ptr, ulong @new)
;

        //go:noescape
        public static System.UIntPtr Xchguintptr(ref System.UIntPtr ptr, System.UIntPtr @new)
;

        //go:noescape
        public static bool Cas64(ref ulong ptr, ulong old, ulong @new)
;
    }
}}}
