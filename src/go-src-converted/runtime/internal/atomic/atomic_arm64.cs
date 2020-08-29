// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm64

// package atomic -- go2cs converted at 2020 August 29 08:16:29 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_arm64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class atomic_package
    {
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
        public static uint Load(ref uint ptr)
;

        //go:noescape
        public static ulong Load64(ref ulong ptr)
;

        //go:noescape
        public static unsafe.Pointer Loadp(unsafe.Pointer ptr)
;

        //go:noescape
        public static void Or8(ref byte ptr, byte val)
;

        //go:noescape
        public static void And8(ref byte ptr, byte val)
;

        //go:noescape
        public static bool Cas64(ref ulong ptr, ulong old, ulong @new)
;

        //go:noescape
        public static void Store(ref uint ptr, uint val)
;

        //go:noescape
        public static void Store64(ref ulong ptr, ulong val)
;

        // NO go:noescape annotation; see atomic_pointer.go.
        public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val)
;
    }
}}}
