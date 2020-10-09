// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build ppc64 ppc64le

// package atomic -- go2cs converted at 2020 October 09 04:45:33 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_ppc64x.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class atomic_package
    {
        //go:noescape
        public static uint Xadd(ptr<uint> ptr, int delta)
;

        //go:noescape
        public static ulong Xadd64(ptr<ulong> ptr, long delta)
;

        //go:noescape
        public static System.UIntPtr Xadduintptr(ptr<System.UIntPtr> ptr, System.UIntPtr delta)
;

        //go:noescape
        public static uint Xchg(ptr<uint> ptr, uint @new)
;

        //go:noescape
        public static ulong Xchg64(ptr<ulong> ptr, ulong @new)
;

        //go:noescape
        public static System.UIntPtr Xchguintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new)
;

        //go:noescape
        public static uint Load(ptr<uint> ptr)
;

        //go:noescape
        public static byte Load8(ptr<byte> ptr)
;

        //go:noescape
        public static ulong Load64(ptr<ulong> ptr)
;

        // NO go:noescape annotation; *ptr escapes if result escapes (#31525)
        public static unsafe.Pointer Loadp(unsafe.Pointer ptr)
;

        //go:noescape
        public static uint LoadAcq(ptr<uint> ptr)
;

        //go:noescape
        public static void And8(ptr<byte> ptr, byte val)
;

        //go:noescape
        public static void Or8(ptr<byte> ptr, byte val)
;

        // NOTE: Do not add atomicxor8 (XOR is not idempotent).

        //go:noescape
        public static bool Cas64(ptr<ulong> ptr, ulong old, ulong @new)
;

        //go:noescape
        public static bool CasRel(ptr<uint> ptr, uint old, uint @new)
;

        //go:noescape
        public static void Store(ptr<uint> ptr, uint val)
;

        //go:noescape
        public static void Store8(ptr<byte> ptr, byte val)
;

        //go:noescape
        public static void Store64(ptr<ulong> ptr, ulong val)
;

        //go:noescape
        public static void StoreRel(ptr<uint> ptr, uint val)
;

        // NO go:noescape annotation; see atomic_pointer.go.
        public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val)
;
    }
}}}
