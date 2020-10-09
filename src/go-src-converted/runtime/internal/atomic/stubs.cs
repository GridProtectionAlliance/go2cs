// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !wasm

// package atomic -- go2cs converted at 2020 October 09 04:45:34 UTC
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
        public static bool Cas(ptr<uint> ptr, uint old, uint @new)
;

        // NO go:noescape annotation; see atomic_pointer.go.
        public static bool Casp1(ptr<unsafe.Pointer> ptr, unsafe.Pointer old, unsafe.Pointer @new)
;

        //go:noescape
        public static bool Casuintptr(ptr<System.UIntPtr> ptr, System.UIntPtr old, System.UIntPtr @new)
;

        //go:noescape
        public static void Storeuintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new)
;

        //go:noescape
        public static System.UIntPtr Loaduintptr(ptr<System.UIntPtr> ptr)
;

        //go:noescape
        public static ulong Loaduint(ptr<ulong> ptr)
;

        // TODO(matloob): Should these functions have the go:noescape annotation?

        //go:noescape
        public static long Loadint64(ptr<long> ptr)
;

        //go:noescape
        public static long Xaddint64(ptr<long> ptr, long delta)
;
    }
}}}
