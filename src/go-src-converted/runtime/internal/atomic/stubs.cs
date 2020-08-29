// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2020 August 29 08:16:30 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\stubs.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class atomic_package
    {
        //go:noescape
        public static bool Cas(ref uint ptr, uint old, uint @new)
;

        // NO go:noescape annotation; see atomic_pointer.go.
        public static bool Casp1(ref unsafe.Pointer ptr, unsafe.Pointer old, unsafe.Pointer @new)
;

        //go:noescape
        public static bool Casuintptr(ref System.UIntPtr ptr, System.UIntPtr old, System.UIntPtr @new)
;

        //go:noescape
        public static void Storeuintptr(ref System.UIntPtr ptr, System.UIntPtr @new)
;

        //go:noescape
        public static System.UIntPtr Loaduintptr(ref System.UIntPtr ptr)
;

        //go:noescape
        public static ulong Loaduint(ref ulong ptr)
;

        // TODO(matloob): Should these functions have the go:noescape annotation?

        //go:noescape
        public static long Loadint64(ref long ptr)
;

        //go:noescape
        public static long Xaddint64(ref long ptr, long delta)
;
    }
}}}
